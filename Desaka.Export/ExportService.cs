using System.Globalization;
using Desaka.Contracts.Common;
using Desaka.DataAccess;
using Desaka.DataAccess.Entities;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Desaka.Export;

public enum ExportFormat
{
    Csv = 0,
    Xls = 1,
    Xlsx = 2
}

public sealed class ExportOptions
{
    public string OutputDirectory { get; set; } = ".";
    public ExportFormat Format { get; set; } = ExportFormat.Csv;
    public string FileNamePrefix { get; set; } = "Output";
    public bool ProtectCsvInjection { get; set; } = true;
    public char CsvSeparator { get; set; } = ';';
}

public sealed class ExportResult
{
    public string RelativePath { get; set; } = "";
    public string FullPath { get; set; } = "";
    public long Size { get; set; }
}

public sealed class ExportService
{
    private readonly IFileService _fileService;

    public ExportService(IFileService fileService)
    {
        _fileService = fileService;
    }

    public async Task<ExportResult> ExportProductsAsync(IEnumerable<ProductsCurrent> products, ExportOptions options, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        var filename = $"{options.FileNamePrefix}_{timestamp}.{FormatExtension(options.Format)}";
        var relativePath = Path.Combine(timestamp, filename);

        byte[] content = options.Format switch
        {
            ExportFormat.Csv => BuildCsv(products, options),
            ExportFormat.Xls => BuildExcel(products, options, isXlsx: false),
            ExportFormat.Xlsx => BuildExcel(products, options, isXlsx: true),
            _ => BuildCsv(products, options)
        };

        var result = await _fileService.WriteBytesAsync(options.OutputDirectory, relativePath, content, cancellationToken);
        return new ExportResult
        {
            RelativePath = relativePath,
            FullPath = result.FullPath,
            Size = result.Size
        };
    }

    private static string FormatExtension(ExportFormat format)
        => format switch
        {
            ExportFormat.Xls => "xls",
            ExportFormat.Xlsx => "xlsx",
            _ => "csv"
        };

    private static byte[] BuildCsv(IEnumerable<ProductsCurrent> products, ExportOptions options)
    {
        var builder = new System.Text.StringBuilder();
        builder.AppendLine(string.Join(options.CsvSeparator, JzShopColumns.All));

        foreach (var product in products)
        {
            var values = JzShopColumns.All.Select(column => FormatValue(GetColumnValue(product, column), options));
            builder.AppendLine(string.Join(options.CsvSeparator, values));
        }

        return System.Text.Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] BuildExcel(IEnumerable<ProductsCurrent> products, ExportOptions options, bool isXlsx)
    {
        IWorkbook workbook = isXlsx ? new XSSFWorkbook() : new HSSFWorkbook();
        var sheet = workbook.CreateSheet("Output");

        var headerRow = sheet.CreateRow(0);
        for (var i = 0; i < JzShopColumns.All.Count; i++)
        {
            headerRow.CreateCell(i).SetCellValue(JzShopColumns.All[i]);
        }

        var rowIndex = 1;
        foreach (var product in products)
        {
            var row = sheet.CreateRow(rowIndex++);
            for (var i = 0; i < JzShopColumns.All.Count; i++)
            {
                var value = GetColumnValue(product, JzShopColumns.All[i]);
                row.CreateCell(i).SetCellValue(value ?? string.Empty);
            }
        }

        using var stream = new MemoryStream();
        workbook.Write(stream, leaveOpen: true);
        return stream.ToArray();
    }

    private static string? GetColumnValue(ProductsCurrent product, string column)
    {
        var isVariant = string.Equals(product.Typ, "varianta", StringComparison.OrdinalIgnoreCase);
        var defaults = isVariant ? ExportDefaults.VariantDefaults : ExportDefaults.MainDefaults;
        var propName = JzShopColumns.ColumnToPropertyName(column);
        var prop = typeof(ProductsCurrent).GetProperty(propName);
        if (prop == null)
        {
            return defaults.TryGetValue(column, out var fallback) ? fallback : null;
        }

        var value = prop.GetValue(product);
        if (value == null)
        {
            return defaults.TryGetValue(column, out var fallback) ? fallback : null;
        }

        return value switch
        {
            DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bool flag => flag ? "1" : "0",
            decimal dec => dec.ToString(CultureInfo.InvariantCulture),
            double dbl => dbl.ToString(CultureInfo.InvariantCulture),
            float flt => flt.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    private static string FormatValue(string? value, ExportOptions options)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var safe = options.ProtectCsvInjection ? ProtectCsv(value) : value;
        var escaped = safe.Replace("\"", "\"\"");
        if (escaped.Contains(options.CsvSeparator) || escaped.Contains('\n') || escaped.Contains('\r'))
        {
            return $"\"{escaped}\"";
        }

        return escaped;
    }

    private static string ProtectCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var first = value[0];
        if (first is '=' or '+' or '-' or '@')
        {
            return "'" + value;
        }

        return value;
    }
}
