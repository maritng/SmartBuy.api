using SmartBuy.Core.Common.Responses;
using Orion.Domain.Orion;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace SmartBuy.Core.Common.Mappers
{
    /// <summary>
    /// Mapea la respuesta cruda de Orion (diccionarios/listas de Dapper) a
    /// StandarResponse tipado. Tolerante: nombres con guiones bajos del SQL se
    /// normalizan contra las propiedades del modelo (sitio_web -> SitioWeb).
    /// </summary>
    public static class OrionMapperExtensions
    {
        public static StandarResponse<T> ToStandard<T>(this OrionResponse response)
        {
            return new StandarResponse<T>
            {
                Success = response.Success,
                Errors = response.Errors,
                Execution = MapExecution(response.Execution),
                Result = MapResult<T>(response.Result)
            };
        }

        private static T? MapResult<T>(object? result)
        {
            if (result == null)
                return default;

            var targetType = typeof(T);

            if (result is T t)
                return t;

            // Caso lista
            if (result is IEnumerable enumerable && targetType != typeof(string))
            {
                if (targetType.IsGenericType)
                {
                    var itemType = targetType.GetGenericArguments()[0];
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;

                    foreach (var item in enumerable)
                    {
                        var mappedItem = MapObject(item, itemType);
                        list.Add(mappedItem);
                    }

                    return (T)list;
                }
            }

            // Caso objeto
            return (T?)MapObject(result, targetType);
        }

        private static StandarExecutionInfo MapExecution(OrionExecutionInfo execution)
        {
            if (execution == null)
                return new StandarExecutionInfo();

            return new StandarExecutionInfo
            {
                Action = execution.Action,
                Executor = execution.Executor,
                Query = execution.Query
            };
        }

        private static object MapObject(object source, Type targetType)
        {
            var target = Activator.CreateInstance(targetType)!;
            var targetProps = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var sourceDict = ToDictionary(source);

            var normalizedDict = sourceDict
                .ToDictionary(k => Normalize(k.Key), v => v.Value);

            foreach (var prop in targetProps)
            {
                var normalizedName = Normalize(prop.Name);

                if (!normalizedDict.TryGetValue(normalizedName, out var value))
                    continue;

                if (value == null)
                    continue;

                try
                {
                    prop.SetValue(target, ConvertValue(value, prop.PropertyType));
                }
                catch
                {
                    // Ignora errores de conversión (robustez)
                }
            }

            return target;
        }

        private static IDictionary<string, object?> ToDictionary(object source)
        {
            if (source is IDictionary<string, object?> dict)
                return dict;

            if (source is IEnumerable enumerable && source is not string)
            {
                var first = enumerable.Cast<object>().FirstOrDefault();

                if (first != null)
                {
                    if (first is IDictionary<string, object?> firstDict)
                        return firstDict;

                    var valueProp = first.GetType().GetProperty("Value");
                    if (valueProp != null)
                    {
                        var inner = valueProp.GetValue(first);
                        if (inner != null)
                            return ToDictionary(inner);
                    }
                }
            }

            return source.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0)
                .ToDictionary(p => p.Name, p => p.GetValue(source));
        }

        private static object? ConvertValue(object value, Type targetType)
        {
            if (value == null)
                return null;

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlyingType == typeof(bool))
            {
                if (value is int i) return i == 1;
                if (value is string s) return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, value.ToString()!, true);

            if (underlyingType == typeof(DateTime))
            {
                switch (value)
                {
                    case DateTime dt: return dt;
                    case DateOnly d: return d.ToDateTime(TimeOnly.MinValue);
                    case DateTimeOffset dto: return dto.DateTime;
                    default:
                        return DateTime.Parse(value.ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }
            }

            if (underlyingType == typeof(DateOnly))
            {
                switch (value)
                {
                    case DateOnly d: return d;
                    case DateTime dt: return DateOnly.FromDateTime(dt);
                    case DateTimeOffset dto: return DateOnly.FromDateTime(dto.DateTime);
                    default:
                        return DateOnly.Parse(value.ToString()!, CultureInfo.InvariantCulture);
                }
            }

            return Convert.ChangeType(value, underlyingType);
        }

        private static string Normalize(string input)
        {
            return input
                .Trim()
                .Replace("_", "")
                .Replace(" ", "")
                .ToLowerInvariant();
        }
    }
}
