using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming.Values;
using NPOI.XSSF.UserModel;

namespace UCode.Extensions
{
    /// <summary>
    /// Exports data
    /// </summary>
    public static class ExportsExtension
    {
        private static bool ShouldIgnore(MemberInfo? memberInfo, object obj)
        {
            if (memberInfo == null)
            {
                return false;
            }

            var jsonIgnoreAttribute = memberInfo.GetCustomAttribute<JsonIgnoreAttribute>();
            if (jsonIgnoreAttribute == null)
            {
                return false;
            }

            var value = memberInfo is PropertyInfo propertyInfo ? propertyInfo.GetValue(obj) : ((FieldInfo)memberInfo).GetValue(obj);

            switch (jsonIgnoreAttribute.Condition)
            {
                case JsonIgnoreCondition.Never:
                    return false;
                case JsonIgnoreCondition.Always:
                    return true;
                case JsonIgnoreCondition.WhenWritingDefault:
                    return value == null || value.Equals(GetDefaultValue(memberInfo.GetType()));
                case JsonIgnoreCondition.WhenWritingNull:
                    return value == null;
                default:
                    throw new ArgumentException($"Unsupported JsonIgnoreCondition: {jsonIgnoreAttribute.Condition}");
            }
        }

        private static object? GetDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        private static void FlattenObjectInternal<T>(JsonElement jsonElement, T obj, string prefix, Action<(string Key, object value)> addAction)
        {
            var type = obj.GetType();

            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    foreach (var property in jsonElement.EnumerateObject())
                    {
                        var propertyName = property.Name;
                        var propertyValue = property.Value;

                        var propertyInfo = properties.FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == propertyName || p.Name == propertyName);
                        var fieldInfo = fields.FirstOrDefault(f => f.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == propertyName || f.Name == propertyName);

                        if (ShouldIgnore(propertyInfo, obj) || ShouldIgnore(fieldInfo, obj))
                        {
                            continue;
                        }

                        var newPrefix = string.IsNullOrEmpty(prefix) ? propertyName : $"{prefix}.{propertyName}";
                        var value = propertyInfo?.GetValue(obj) ?? fieldInfo?.GetValue(obj);

                        if (value != null)
                        {
                            FlattenObjectInternal(propertyValue, value, newPrefix, addAction);
                        }
                    }
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var arrayElement in jsonElement.EnumerateArray())
                    {
                        var newPrefix = $"{prefix}[{index}]";
                        var value = obj.GetType().GetProperty(prefix)?.GetValue(obj) ?? obj.GetType().GetField(prefix)?.GetValue(obj);

                        if (value != null)
                        {
                            FlattenObjectInternal(arrayElement, value, newPrefix, addAction);
                        }
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    addAction((prefix, jsonElement.GetRawText()));
                    break;

                default:
                    throw new ArgumentException($"Unsupported JSON value kind: {jsonElement.ValueKind}");
            }
        }

        /// <summary>
        /// Create .xslx document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itens"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public static MemoryStream ToExcel<T>(this IEnumerable<T> itens, string sheetName)
        {
            var columns = new List<string>();

            var result = new MemoryStream();

            using (var workbook = new XSSFWorkbook(XSSFWorkbookType.XLSX))
            {

                var excelSheet = workbook.CreateSheet(sheetName);
                var rowColumn = excelSheet.CreateRow(0);

                var rowIndex = 1;

                var actionAdd = new Action<(string Key, object value)>(((string Key, object Value) args) =>
                {
                    if (!columns.Contains(args.Key))
                    {
                        columns.Add(args.Key);
                        rowColumn.CreateCell(columns.Count - 1).SetCellValue(args.Key);
                    }

                    var colIndex = columns.IndexOf(args.Key);

                    var row = excelSheet.CreateRow(rowIndex);

                    if (args.Value == null)
                    {
                        row.CreateCell(colIndex).SetCellType(CellType.Blank);
                        row.CreateCell(colIndex).SetBlank();
                    }
                    else
                    {
                        var type = args.Value.GetType();
                        if (type == typeof(int) || type == typeof(double) || type == typeof(float) || type == typeof(decimal) ||
                            type == typeof(long) || type == typeof(short) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(uint))
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.Numeric);

                            row.CreateCell(colIndex).SetCellValue(Convert.ToDouble(args.Value));
                        }
                        else if (type == typeof(byte))
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.Numeric);
                            row.CreateCell(colIndex).SetCellValue(Convert.ToDouble(args.Value));
                        }
                        else if (type == typeof(string))
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.String);
                            row.CreateCell(colIndex).SetCellValue((string)args.Value);
                        }
                        else if (type == typeof(bool))
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.Boolean);
                            row.CreateCell(colIndex).SetCellValue((bool)args.Value);
                        }
                        else if (type == typeof(void)) // Ou qualquer outro tipo que você considere como "Blank"
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.Blank);
                            row.CreateCell(colIndex).SetBlank();
                        }
                        else if (type == typeof(DateOnly))
                        {
                            row.CreateCell(colIndex).SetCellValue((DateOnly)args.Value);
                        }
                        else if (type == typeof(DateTime))
                        {
                            row.CreateCell(colIndex).SetCellValue((DateTime)args.Value);
                        }
                        else
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.Unknown);
                        }
                    }
                });

                foreach (var item in itens)
                {
                    FlattenObjectInternal(JsonSerializer.SerializeToElement(item), item, "", actionAdd);

                    rowIndex++;
                }

                workbook.Write(result, true);
            }

            return result;
        }
    }
}
