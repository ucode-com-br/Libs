using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace UCode.Extensions
{
    /// <summary>
    /// Exports data
    /// </summary>
    public static class ExportsExtension
    {
        /// <summary>
        /// Generate Csf file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itens"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static MemoryStream ToCsv<T>(this IEnumerable<T> itens, Action<CsvHelper.Configuration.CsvConfiguration>? configureAction = null)
        {
            var result = new MemoryStream();

            using (var writer = new StreamWriter(result, leaveOpen: true))
            {
                var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);

                configureAction?.Invoke(config);

                using (var csv = new CsvWriter(writer, config, true))
                {
                    var properties = typeof(T).GetProperties();
                    foreach (var property in properties)
                    {
                        csv.WriteField(property.Name);
                    }
                    csv.NextRecord();

                    foreach (var item in itens)
                    {
                        foreach (var property in properties)
                        {
                            var value = property.GetValue(item);

                            csv.WriteField(value);
                        }
                        csv.NextRecord();
                    }
                }
            }

            result.Position = 0;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object? GetDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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

                        var propertyElementName = propertyInfo?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
                        var fieldElementName = fieldInfo?.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;

                        if (ShouldIgnore(propertyInfo, obj) || ShouldIgnore(fieldInfo, obj))
                        {
                            continue;
                        }

                        var newPrefix = string.IsNullOrEmpty(prefix) ? propertyElementName ?? fieldElementName : $"{prefix}.{propertyElementName ?? fieldElementName}";
                        var value = propertyInfo?.GetValue(obj) ?? fieldInfo?.GetValue(obj);

                        if (value != null)
                        {
                            FlattenObjectInternal(propertyValue, value, newPrefix, addAction);
                        }
                    }
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    var enumerator = ((IEnumerable)obj).GetEnumerator();
                    foreach (var arrayElement in jsonElement.EnumerateArray())
                    {
                        var newPrefix = $"{prefix}[{index}]";
                        enumerator.MoveNext();

                        var value = enumerator.Current;

                        if (value != null)
                        {
                            FlattenObjectInternal(arrayElement, value, newPrefix, addAction);
                        }
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                    if (jsonElement.TryGetGuid(out var guidValue))
                    {
                        addAction((prefix, guidValue));
                    }
                    else if (jsonElement.TryGetDateTimeOffset(out var datetimeoffsetValue))
                    {
                        addAction((prefix, datetimeoffsetValue));
                    }
                    else if (jsonElement.TryGetDateTime(out var datetimeValue))
                    {
                        addAction((prefix, datetimeValue));
                    }
                    else if (DateOnly.TryParse(jsonElement.GetRawText().Trim('"'), out var dateValue))
                    {
                        addAction((prefix, datetimeValue));
                    }
                    else
                    {
                        addAction((prefix, jsonElement.GetRawText().Trim('"')));
                    }
                    break;
                case JsonValueKind.Number:
                    if (jsonElement.TryGetDecimal(out var decimalValue))
                    {
                        addAction((prefix, decimalValue));
                    }
                    else if (jsonElement.TryGetDouble(out var doubleValue))
                    {
                        addAction((prefix, doubleValue));
                    }
                    else if (jsonElement.TryGetSingle(out var singleValue))
                    {
                        addAction((prefix, singleValue));
                    }
                    else if (jsonElement.TryGetInt64(out var int64Value))
                    {
                        addAction((prefix, int64Value));
                    }
                    else
                    {
                        addAction((prefix, jsonElement.GetInt32()));
                    }
                    break;
                case JsonValueKind.True:
                    addAction((prefix, jsonElement.GetBoolean()));
                    break;
                case JsonValueKind.False:
                    addAction((prefix, jsonElement.GetBoolean()));
                    break;
                case JsonValueKind.Null:
                    addAction((prefix, ""));
                    break;
                case JsonValueKind.Undefined:
                    addAction((prefix, jsonElement.GetRawText().Trim('"')));
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

                var headerStyle = workbook.CreateCellStyle();
                var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerFont.FontHeightInPoints = 12;
                headerStyle.SetFont(headerFont);
                headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;

                var actionAdd = new Action<(string Key, object value)>(((string Key, object Value) args) =>

                {

                    if (!columns.Contains(args.Key))
                    {
                        columns.Add(args.Key);
                        var cell = rowColumn.CreateCell(columns.Count - 1);
                        cell.SetCellValue(args.Key);
                        cell.CellStyle = headerStyle;
                    }

                    var colIndex = columns.IndexOf(args.Key);

                    var row = excelSheet.GetRow(rowIndex) ?? excelSheet.CreateRow(rowIndex);

                    if (args.Value == null)

                    {

                        row.CreateCell(colIndex).SetCellType(CellType.Blank);

                        row.CreateCell(colIndex).SetBlank();

                    }

                    else

                    {
                        var value = args.Value;
                        var type = value.GetType();

                        if (type == typeof(void))
                        {
                            return;
                        }

                        if (type == typeof(int) || type == typeof(double) || type == typeof(float) || type == typeof(decimal) ||
                            type == typeof(long) || type == typeof(short) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(uint))
                        {

                            row.CreateCell(colIndex).SetCellType(CellType.Numeric);

                            row.CreateCell(colIndex).SetCellValue(Convert.ToDouble(value));
                        }
                        else if (type == typeof(byte))
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.Numeric);

                            row.CreateCell(colIndex).SetCellValue(Convert.ToDouble(value));
                        }
                        else if (type == typeof(string))
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.String);

                            row.CreateCell(colIndex).SetCellValue((string)value);
                        }
                        else if (type == typeof(bool))
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.Boolean);

                            row.CreateCell(colIndex).SetCellValue((bool)value);
                        }
                        else if (type == typeof(DateOnly))
                        {
                            var dateCell = row.CreateCell(colIndex);

                            dateCell.SetCellValue(((DateOnly)value).ToDateTime(TimeOnly.MinValue));

                            var style = workbook.CreateCellStyle();

                            style.DataFormat = workbook.CreateDataFormat().GetFormat("dd/MM/yyyy");

                            dateCell.CellStyle = style;
                        }
                        else if (type == typeof(DateTime))
                        {
                            var dateCell = row.CreateCell(colIndex);

                            dateCell.SetCellValue((DateTime)value);

                            var style = workbook.CreateCellStyle();

                            style.DataFormat = workbook.CreateDataFormat().GetFormat("dd/MM/yyyy");

                            dateCell.CellStyle = style;
                        }
                        else
                        {
                            row.CreateCell(colIndex).SetCellType(CellType.String);

                            row.CreateCell(colIndex).SetCellValue(value.ToString());
                        }

                    }

                });


                foreach (var item in itens)
                {

                    var jsonElement = JsonSerializer.SerializeToElement<T>(item);
                    FlattenObjectInternal(jsonElement, item, "", actionAdd);

                    rowIndex++;
                }

                workbook.Write(result, true);
            }

            return result;

        }
    }


}
