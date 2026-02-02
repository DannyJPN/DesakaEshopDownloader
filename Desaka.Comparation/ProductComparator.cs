using System.Reflection;
using Desaka.Contracts.Common;
using Desaka.DataAccess.Entities;

namespace Desaka.Comparation;

public sealed record FieldChange(string Field, string? OldValue, string? NewValue);

public sealed record ComparisonResult(bool IsMatch, IReadOnlyList<FieldChange> Changes);

public interface IProductComparator
{
    ComparisonResult Compare(ProductsCurrent current, AutopollSnapshot snapshot);
}

public sealed class ProductComparator : IProductComparator
{
    private readonly IReadOnlyList<string> _columns;
    private readonly IReadOnlyDictionary<string, PropertyInfo> _currentProps;
    private readonly IReadOnlyDictionary<string, PropertyInfo> _snapshotProps;

    public ProductComparator()
    {
        _columns = JzShopColumns.All;
        _currentProps = BuildPropertyMap(typeof(ProductsCurrent));
        _snapshotProps = BuildPropertyMap(typeof(AutopollSnapshot));
    }

    public ComparisonResult Compare(ProductsCurrent current, AutopollSnapshot snapshot)
    {
        var changes = new List<FieldChange>();
        foreach (var column in _columns)
        {
            var currentValue = GetValue(_currentProps, current, column);
            var snapshotValue = GetValue(_snapshotProps, snapshot, column);

            if (!AreEqual(currentValue, snapshotValue))
            {
                changes.Add(new FieldChange(column, currentValue, snapshotValue));
            }
        }

        return new ComparisonResult(changes.Count == 0, changes);
    }

    private static string? GetValue(IReadOnlyDictionary<string, PropertyInfo> props, object instance, string column)
    {
        if (!props.TryGetValue(column, out var prop))
        {
            return null;
        }

        var value = prop.GetValue(instance);
        return value switch
        {
            null => null,
            DateTime date => date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            bool flag => flag ? "1" : "0",
            decimal dec => dec.ToString(System.Globalization.CultureInfo.InvariantCulture),
            double dbl => dbl.ToString(System.Globalization.CultureInfo.InvariantCulture),
            float flt => flt.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    private static IReadOnlyDictionary<string, PropertyInfo> BuildPropertyMap(Type type)
    {
        var map = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var column in JzShopColumns.All)
        {
            var propertyName = JzShopColumns.ColumnToPropertyName(column);
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                map[column] = prop;
            }
        }

        return map;
    }

    private static bool AreEqual(string? a, string? b)
    {
        return string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
