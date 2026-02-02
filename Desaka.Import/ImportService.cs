using System.Globalization;
using Desaka.Contracts.Common;
using Desaka.DataAccess.Entities;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Desaka.Import;

public enum ImportFormat
{
    Csv = 0,
    Xls = 1,
    Xlsx = 2
}

public sealed class ImportOptions
{
    public ImportFormat Format { get; set; } = ImportFormat.Csv;
    public char CsvSeparator { get; set; } = ';';
}

public sealed class ImportService
{
    public IReadOnlyList<ProductsCurrent> ImportProducts(Stream input, ImportOptions options)
    {
        return options.Format switch
        {
            ImportFormat.Csv => ImportCsv(input, options),
            ImportFormat.Xls => ImportExcel(input),
            ImportFormat.Xlsx => ImportExcel(input),
            _ => ImportCsv(input, options)
        };
    }

    private static IReadOnlyList<ProductsCurrent> ImportCsv(Stream input, ImportOptions options)
    {
        using var reader = new StreamReader(input, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var lines = new List<string[]>();
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            lines.Add(ParseCsvLine(line, options.CsvSeparator));
        }

        if (lines.Count == 0)
        {
            return Array.Empty<ProductsCurrent>();
        }

        var header = lines[0];
        var results = new List<ProductsCurrent>();
        for (var i = 1; i < lines.Count; i++)
        {
            var row = lines[i];
            var entity = new ProductsCurrent();
            MapRow(header, row, entity);
            results.Add(entity);
        }

        return results;
    }

    private static IReadOnlyList<ProductsCurrent> ImportExcel(Stream input)
    {
        IWorkbook workbook = input.Length == 0 ? new XSSFWorkbook() : WorkbookFactory.Create(input);
        var sheet = workbook.GetSheetAt(0);
        var headerRow = sheet.GetRow(0);
        if (headerRow == null)
        {
            return Array.Empty<ProductsCurrent>();
        }

        var header = new string[headerRow.LastCellNum];
        for (var i = 0; i < header.Length; i++)
        {
            header[i] = headerRow.GetCell(i)?.ToString() ?? string.Empty;
        }

        var results = new List<ProductsCurrent>();
        for (var r = 1; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null)
            {
                continue;
            }

            var values = new string[header.Length];
            for (var i = 0; i < header.Length; i++)
            {
                values[i] = row.GetCell(i)?.ToString() ?? string.Empty;
            }

            var entity = new ProductsCurrent();
            MapRow(header, values, entity);
            results.Add(entity);
        }

        return results;
    }

    private static void MapRow(string[] header, string[] values, ProductsCurrent entity)
    {
        for (var i = 0; i < header.Length && i < values.Length; i++)
        {
            var column = header[i];
            if (string.IsNullOrWhiteSpace(column))
            {
                continue;
            }

            var propName = JzShopColumns.ColumnToPropertyName(column.Trim());
            var prop = typeof(ProductsCurrent).GetProperty(propName);
            if (prop == null || !prop.CanWrite)
            {
                continue;
            }

            var converted = ConvertValue(values[i], prop.PropertyType);
            prop.SetValue(entity, converted);
        }
    }

    private static object? ConvertValue(string value, Type type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        try
        {
            if (targetType == typeof(string))
            {
                return value;
            }

            if (targetType == typeof(bool))
            {
                return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            if (targetType == typeof(int))
            {
                return int.Parse(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(long))
            {
                return long.Parse(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(decimal))
            {
                return decimal.Parse(value, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(DateTime))
            {
                return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            }

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private static string[] ParseCsvLine(string line, char separator)
    {
        var values = new List<string>();
        var builder = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    builder.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (ch == separator && !inQuotes)
            {
                values.Add(builder.ToString());
                builder.Clear();
            }
            else
            {
                builder.Append(ch);
            }
        }

        values.Add(builder.ToString());
        return values.ToArray();
    }
}
