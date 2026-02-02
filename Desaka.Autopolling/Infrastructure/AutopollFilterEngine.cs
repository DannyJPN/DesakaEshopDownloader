using System.Reflection;
using Desaka.DataAccess.Entities;
using Desaka.Contracts.Common;

namespace Desaka.Autopolling.Infrastructure;

public sealed class AutopollFilterEngine
{
    private readonly IReadOnlyDictionary<string, PropertyInfo> _properties;

    public AutopollFilterEngine()
    {
        _properties = JzShopColumns.All
            .Select(column => new { column, prop = typeof(ProductsCurrent).GetProperty(JzShopColumns.ColumnToPropertyName(column)) })
            .Where(x => x.prop != null)
            .ToDictionary(x => x.column, x => x.prop!, StringComparer.OrdinalIgnoreCase);
    }

    public bool Matches(ProductsCurrent product, string? filterDefinition)
    {
        if (string.IsNullOrWhiteSpace(filterDefinition))
        {
            return true;
        }

        var clauses = filterDefinition.Split(new[] { ";", "&&" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var clause in clauses)
        {
            if (!EvaluateClause(product, clause))
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateClause(ProductsCurrent product, string clause)
    {
        var op = DetectOperator(clause);
        if (op == null)
        {
            return true;
        }

        var parts = clause.Split(op, 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return true;
        }

        var column = parts[0];
        var expected = parts[1];
        if (!_properties.TryGetValue(column, out var prop))
        {
            return true;
        }

        var raw = prop.GetValue(product);
        var actual = raw?.ToString() ?? string.Empty;

        return op switch
        {
            "!=" => !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "=*" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            "!*" => !actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            "=" => string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }

    private static string? DetectOperator(string clause)
    {
        if (clause.Contains("!="))
        {
            return "!=";
        }

        if (clause.Contains("!*"))
        {
            return "!*";
        }

        if (clause.Contains("=*"))
        {
            return "=*";
        }

        if (clause.Contains("="))
        {
            return "=";
        }

        return null;
    }
}
