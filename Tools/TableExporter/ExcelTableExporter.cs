using System.Globalization;
using CardTables;
using ClosedXML.Excel;

namespace TableExporterApp
{
    public sealed class ExportReport
    {
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public List<string> Lines { get; } = new List<string>();
        public string TargetDirectory { get; set; } = string.Empty;
    }

    public static class ExcelTableExporter
    {
        public const string TargetRelativePath = "Assets/StreamingAssets/Tables";
        const int HeaderRow = 1;
        const int TypeRow = 2;
        const int CommentRow = 3;
        const int FirstDataRow = 4;

        public static string GetTargetDirectory(string projectRoot)
        {
            return Path.Combine(projectRoot, "Assets", "StreamingAssets", "Tables");
        }

        public static bool TryFindProjectRoot(string startDirectory, out string projectRoot, out string error)
        {
            var dir = new DirectoryInfo(startDirectory);
            while (dir != null)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, "Assets")) &&
                    Directory.Exists(Path.Combine(dir.FullName, "ProjectSettings")))
                {
                    projectRoot = dir.FullName;
                    error = string.Empty;
                    return true;
                }

                dir = dir.Parent;
            }

            projectRoot = string.Empty;
            error = "没有找到 Unity 工程。请从本工程里的“导表工具”运行。";
            return false;
        }

        public static ExportReport Export(string projectRoot, string sourceDirectory)
        {
            var report = new ExportReport();
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                FailBatch(report, "没有找到 Unity 工程。");
                return report;
            }

            var targetDirectory = GetTargetDirectory(projectRoot);
            report.TargetDirectory = targetDirectory;
            if (string.IsNullOrWhiteSpace(sourceDirectory))
            {
                FailBatch(report, "请先选择源文件夹。");
                return report;
            }

            string sourceFull;
            try
            {
                sourceFull = Path.GetFullPath(sourceDirectory);
            }
            catch (Exception exception)
            {
                FailBatch(report, "源文件夹无效：" + exception.Message);
                return report;
            }

            if (!Directory.Exists(sourceFull))
            {
                FailBatch(report, "源文件夹不存在：" + sourceFull);
                return report;
            }

            var targetFull = Path.GetFullPath(targetDirectory);
            if (PathsOverlap(sourceFull, targetFull))
            {
                FailBatch(report, "源文件夹不能与目标文件夹相同，也不能互相包含。");
                return report;
            }

            Directory.CreateDirectory(targetFull);
            var files = Directory.GetFiles(sourceFull, "*.xlsx", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            var workbooks = new List<string>();
            for (var i = 0; i < files.Length; i++)
            {
                var fileName = Path.GetFileName(files[i]);
                if (fileName.StartsWith("~$", StringComparison.Ordinal))
                {
                    continue;
                }

                workbooks.Add(files[i]);
            }

            if (workbooks.Count == 0)
            {
                report.Lines.Add("没有找到 .xlsx 文件。只读取源文件夹这一层，不包含子文件夹。");
                report.Lines.Add("完成：成功 0，失败 0");
                return report;
            }

            var usedTableNames = new HashSet<string>(StringComparer.Ordinal);
            var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < workbooks.Count; i++)
            {
                ExportWorkbook(workbooks[i], targetFull, usedTableNames, usedFileNames, report);
            }

            report.Lines.Add("完成：成功 " + report.Succeeded + "，失败 " + report.Failed);
            return report;
        }

        static void ExportWorkbook(
            string filePath,
            string targetDirectory,
            HashSet<string> usedTableNames,
            HashSet<string> usedFileNames,
            ExportReport report)
        {
            var displayName = Path.GetFileName(filePath);
            FileStream? stream = null;
            XLWorkbook? workbook = null;
            try
            {
                stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                workbook = new XLWorkbook(stream);
            }
            catch (Exception exception)
            {
                workbook?.Dispose();
                stream?.Dispose();
                report.Failed++;
                report.Lines.Add(displayName + "：无法读取。" + exception.Message);
                return;
            }

            using (stream)
            using (workbook)
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    try
                    {
                        ExportWorksheet(displayName, worksheet, targetDirectory, usedTableNames, usedFileNames, report);
                    }
                    catch (SheetExportException exception)
                    {
                        report.Failed++;
                        report.Lines.Add(displayName + " / " + worksheet.Name + "：" + exception.Message);
                    }
                    catch (Exception exception)
                    {
                        report.Failed++;
                        report.Lines.Add(displayName + " / " + worksheet.Name + "：导出失败。" + exception.Message);
                    }
                }
            }
        }

        static void ExportWorksheet(
            string fileName,
            IXLWorksheet worksheet,
            string targetDirectory,
            HashSet<string> usedTableNames,
            HashSet<string> usedFileNames,
            ExportReport report)
        {
            var tableName = worksheet.Name.Trim();
            if (tableName.Length == 0 || tableName.StartsWith("#", StringComparison.Ordinal))
            {
                report.Lines.Add(fileName + " / " + worksheet.Name + "：已跳过。");
                return;
            }

            if (!TryMapColumns(worksheet, out var mappedColumns, out var mapError))
            {
                if (mapError == null)
                {
                    report.Lines.Add(fileName + " / " + tableName + "：已跳过空表。");
                    return;
                }

                throw new SheetExportException(mapError);
            }

            var columns = new TableColumn[mappedColumns.Count];
            var idColumn = -1;
            for (var i = 0; i < mappedColumns.Count; i++)
            {
                columns[i] = new TableColumn(mappedColumns[i].Name, mappedColumns[i].Type, 0);
                if (mappedColumns[i].IsId)
                {
                    idColumn = i;
                }
            }

            if (idColumn < 0)
            {
                throw new SheetExportException("缺少 Id 列。Id 列的类型只能是 int、long 或 string。");
            }

            var rowValues = new List<object?[]>();
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            if (lastRow - CommentRow > TableBinary.MaxRows)
            {
                throw new SheetExportException("数据行超过 " + TableBinary.MaxRows + "。");
            }

            for (var rowNumber = FirstDataRow; rowNumber <= lastRow; rowNumber++)
            {
                if (IsRowEmpty(worksheet, rowNumber, mappedColumns))
                {
                    continue;
                }

                var values = new object?[mappedColumns.Count];
                for (var i = 0; i < mappedColumns.Count; i++)
                {
                    var cell = ReadCell(worksheet, rowNumber, mappedColumns[i].ColumnNumber);
                    try
                    {
                        values[i] = ConvertCell(cell, mappedColumns[i].Type, mappedColumns[i].IsId);
                    }
                    catch (FormatException exception)
                    {
                        throw new SheetExportException("第 " + rowNumber + " 行字段 " + mappedColumns[i].Name + " " + exception.Message);
                    }
                }

                if (values[idColumn] == null || values[idColumn] is string idText && idText.Length == 0)
                {
                    throw new SheetExportException("第 " + rowNumber + " 行 Id 为空。");
                }

                rowValues.Add(values);
            }

            var ids = new string[rowValues.Count];
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            for (var row = 0; row < rowValues.Count; row++)
            {
                ids[row] = IdKey(rowValues[row][idColumn], columns[idColumn].Type);
                if (!seenIds.Add(ids[row]))
                {
                    throw new SheetExportException("Id 重复：" + ids[row]);
                }
            }

            var sizedColumns = new TableColumn[columns.Length];
            for (var i = 0; i < columns.Length; i++)
            {
                sizedColumns[i] = new TableColumn(columns[i].Name, columns[i].Type, rowValues.Count);
                for (var row = 0; row < rowValues.Count; row++)
                {
                    Assign(sizedColumns[i], row, rowValues[row][i]);
                }
            }

            if (!usedTableNames.Add(tableName))
            {
                throw new SheetExportException("表名与本次已导出的表重名。");
            }

            var outputName = SanitizeFileName(tableName);
            if (!usedFileNames.Add(outputName))
            {
                usedTableNames.Remove(tableName);
                throw new SheetExportException("输出文件名与本次已导出的表冲突。");
            }

            var destination = Path.Combine(targetDirectory, outputName + ".bytes");
            var bytes = TableBinary.Write(tableName, sizedColumns, ids);
            try
            {
                WriteAtomically(destination, bytes);
            }
            catch (Exception exception)
            {
                usedTableNames.Remove(tableName);
                usedFileNames.Remove(outputName);
                throw new SheetExportException("无法写入目标文件。" + exception.Message);
            }

            report.Succeeded++;
            report.Lines.Add(fileName + " / " + tableName + " -> " + TargetRelativePath + "/" + outputName + ".bytes（" + ids.Length + " 行）");
        }

        static bool TryMapColumns(IXLWorksheet worksheet, out List<MappedColumn> columns, out string? error)
        {
            columns = new List<MappedColumn>();
            error = null;
            var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            if (lastColumn == 0)
            {
                return false;
            }

            if (lastColumn > TableBinary.MaxFields)
            {
                error = "字段数量超过 " + TableBinary.MaxFields + "。";
                return false;
            }

            var names = new HashSet<string>(StringComparer.Ordinal);
            var sawName = false;
            for (var columnNumber = 1; columnNumber <= lastColumn; columnNumber++)
            {
                var headerCell = worksheet.Cell(HeaderRow, columnNumber);
                if (headerCell.IsMerged())
                {
                    var origin = headerCell.MergedRange().FirstCell();
                    if (origin.Address.RowNumber != HeaderRow || origin.Address.ColumnNumber != columnNumber)
                    {
                        continue;
                    }
                }

                var name = headerCell.GetString().Trim();
                if (name.Length == 0)
                {
                    continue;
                }

                sawName = true;
                if (!names.Add(name))
                {
                    error = "字段名重复：" + name;
                    return false;
                }

                var typeText = Origin(worksheet, TypeRow, columnNumber).GetString();
                if (!TryParseType(typeText, out var type))
                {
                    error = string.IsNullOrWhiteSpace(typeText)
                        ? "字段 " + name + " 缺少类型。"
                        : "字段 " + name + " 的类型无法识别：" + typeText.Trim();
                    return false;
                }

                var isId = string.Equals(name, "Id", StringComparison.OrdinalIgnoreCase);
                if (isId && type != TableFieldType.Int && type != TableFieldType.Long && type != TableFieldType.String)
                {
                    error = "Id 列的类型只能是 int、long 或 string。";
                    return false;
                }

                columns.Add(new MappedColumn(columnNumber, name, type, isId));
            }

            if (!sawName)
            {
                return false;
            }

            if (columns.Count == 0)
            {
                error = "没有有效字段。";
                return false;
            }

            var idCount = 0;
            for (var i = 0; i < columns.Count; i++)
            {
                if (columns[i].IsId)
                {
                    idCount++;
                }
            }

            if (idCount > 1)
            {
                error = "Id 列重复。";
                return false;
            }

            return true;
        }

        static bool IsRowEmpty(IXLWorksheet worksheet, int rowNumber, List<MappedColumn> columns)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                if (!IsBlank(ReadCell(worksheet, rowNumber, columns[i].ColumnNumber)))
                {
                    return false;
                }
            }

            return true;
        }

        static XLCellValue ReadCell(IXLWorksheet worksheet, int rowNumber, int columnNumber)
        {
            return Origin(worksheet, rowNumber, columnNumber).Value;
        }

        static IXLCell Origin(IXLWorksheet worksheet, int rowNumber, int columnNumber)
        {
            var cell = worksheet.Cell(rowNumber, columnNumber);
            if (cell.IsMerged())
            {
                return cell.MergedRange().FirstCell();
            }

            return cell;
        }

        static bool IsBlank(XLCellValue value)
        {
            return value.Type == XLDataType.Blank;
        }

        static object? ConvertCell(XLCellValue value, TableFieldType type, bool isId)
        {
            if (value.Type == XLDataType.Error)
            {
                throw new FormatException("是 Excel 错误值。");
            }

            if (IsBlank(value))
            {
                if (isId)
                {
                    return null;
                }

                return DefaultValue(type);
            }

            switch (type)
            {
                case TableFieldType.Int:
                    return checked((int)RequireInteger(value));
                case TableFieldType.Long:
                    return RequireInteger(value);
                case TableFieldType.Float:
                    return (float)RequireNumber(value);
                case TableFieldType.Double:
                    return RequireNumber(value);
                case TableFieldType.Bool:
                    return RequireBool(value);
                case TableFieldType.String:
                    return RequireText(value);
                case TableFieldType.IntArray:
                    return ParseArray(RequireText(value), text => checked((int)ParseIntegerText(text)));
                case TableFieldType.LongArray:
                    return ParseArray(RequireText(value), ParseIntegerText);
                case TableFieldType.FloatArray:
                    return ParseArray(RequireText(value), text => (float)ParseNumberText(text));
                case TableFieldType.DoubleArray:
                    return ParseArray(RequireText(value), ParseNumberText);
                case TableFieldType.BoolArray:
                    return ParseArray(RequireText(value), ParseBoolText);
                case TableFieldType.StringArray:
                    return SplitText(RequireText(value));
                default:
                    throw new FormatException("类型无效。");
            }
        }

        static object DefaultValue(TableFieldType type)
        {
            switch (type)
            {
                case TableFieldType.Int:
                    return 0;
                case TableFieldType.Long:
                    return 0L;
                case TableFieldType.Float:
                    return 0f;
                case TableFieldType.Double:
                    return 0d;
                case TableFieldType.Bool:
                    return false;
                case TableFieldType.String:
                    return string.Empty;
                case TableFieldType.IntArray:
                    return Array.Empty<int>();
                case TableFieldType.LongArray:
                    return Array.Empty<long>();
                case TableFieldType.FloatArray:
                    return Array.Empty<float>();
                case TableFieldType.DoubleArray:
                    return Array.Empty<double>();
                case TableFieldType.BoolArray:
                    return Array.Empty<bool>();
                case TableFieldType.StringArray:
                    return Array.Empty<string>();
                default:
                    throw new FormatException("类型无效。");
            }
        }

        static long RequireInteger(XLCellValue value)
        {
            if (value.Type == XLDataType.Number)
            {
                return ToInteger(value.GetNumber());
            }

            if (value.Type == XLDataType.Text)
            {
                return ParseIntegerText(value.GetText());
            }

            throw new FormatException("无法解析为整数。");
        }

        static double RequireNumber(XLCellValue value)
        {
            if (value.Type == XLDataType.Number)
            {
                return value.GetNumber();
            }

            if (value.Type == XLDataType.Text)
            {
                return ParseNumberText(value.GetText());
            }

            throw new FormatException("无法解析为数字。");
        }

        static bool RequireBool(XLCellValue value)
        {
            if (value.Type == XLDataType.Boolean)
            {
                return value.GetBoolean();
            }

            if (value.Type == XLDataType.Number)
            {
                var number = value.GetNumber();
                if (number == 0d)
                {
                    return false;
                }

                if (number == 1d)
                {
                    return true;
                }
            }

            if (value.Type == XLDataType.Text && TryParseBool(value.GetText(), out var parsed))
            {
                return parsed;
            }

            throw new FormatException("无法解析为 bool。可用 1、0、true、false、yes、no、是、否。");
        }

        static string RequireText(XLCellValue value)
        {
            switch (value.Type)
            {
                case XLDataType.Text:
                    return value.GetText().Trim();
                case XLDataType.Number:
                    return NumberToText(value.GetNumber());
                case XLDataType.Boolean:
                    return value.GetBoolean() ? "true" : "false";
                case XLDataType.DateTime:
                    return FormatDate(value.GetDateTime());
                case XLDataType.TimeSpan:
                    return value.GetTimeSpan().ToString("c", CultureInfo.InvariantCulture);
                default:
                    throw new FormatException("无法解析为文本。");
            }
        }

        static long ParseIntegerText(string text)
        {
            if (long.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            throw new FormatException("无法解析为整数：" + text);
        }

        static double ParseNumberText(string text)
        {
            if (double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
                !double.IsNaN(value) &&
                !double.IsInfinity(value))
            {
                return value;
            }

            throw new FormatException("无法解析为数字：" + text);
        }

        static bool ParseBoolText(string text)
        {
            if (TryParseBool(text, out var value))
            {
                return value;
            }

            throw new FormatException("无法解析为 bool：" + text);
        }

        static bool TryParseBool(string text, out bool value)
        {
            switch (text.Trim().ToLowerInvariant())
            {
                case "1":
                case "true":
                case "yes":
                case "是":
                    value = true;
                    return true;
                case "0":
                case "false":
                case "no":
                case "否":
                    value = false;
                    return true;
                default:
                    value = false;
                    return false;
            }
        }

        static long ToInteger(double number)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
            {
                throw new FormatException("无法解析为整数。");
            }

            var rounded = Math.Round(number, MidpointRounding.ToEven);
            if (Math.Abs(number - rounded) > 0.0000001d || rounded < long.MinValue || rounded > long.MaxValue)
            {
                throw new FormatException("不是整数：" + NumberToText(number));
            }

            return (long)rounded;
        }

        static string NumberToText(double number)
        {
            var rounded = Math.Round(number, MidpointRounding.ToEven);
            if (Math.Abs(number - rounded) <= 0.0000001d && Math.Abs(rounded) < 1e15)
            {
                return rounded.ToString("0", CultureInfo.InvariantCulture);
            }

            return number.ToString("G17", CultureInfo.InvariantCulture);
        }

        static string FormatDate(DateTime value)
        {
            if (value.TimeOfDay == TimeSpan.Zero)
            {
                return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            return value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        static T[] ParseArray<T>(string text, Func<string, T> parse)
        {
            var parts = SplitText(text);
            if (parts.Length > TableBinary.MaxArrayLength)
            {
                throw new FormatException("数组长度超过 " + TableBinary.MaxArrayLength + "。");
            }

            var values = new T[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                values[i] = parse(parts[i]);
            }

            return values;
        }

        static string[] SplitText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<string>();
            }

            var parts = text.Split('|');
            var values = new List<string>(parts.Length);
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                if (part.Length == 0)
                {
                    continue;
                }

                values.Add(part);
            }

            return values.ToArray();
        }

        static string IdKey(object? value, TableFieldType type)
        {
            switch (type)
            {
                case TableFieldType.Int:
                    return ((int)value!).ToString(CultureInfo.InvariantCulture);
                case TableFieldType.Long:
                    return ((long)value!).ToString(CultureInfo.InvariantCulture);
                case TableFieldType.String:
                    return (string)value!;
                default:
                    throw new SheetExportException("Id 列的类型只能是 int、long 或 string。");
            }
        }

        static void Assign(TableColumn column, int row, object? value)
        {
            switch (column.Type)
            {
                case TableFieldType.Int:
                    column.Ints[row] = (int)value!;
                    break;
                case TableFieldType.Long:
                    column.Longs[row] = (long)value!;
                    break;
                case TableFieldType.Float:
                    column.Floats[row] = (float)value!;
                    break;
                case TableFieldType.Double:
                    column.Doubles[row] = (double)value!;
                    break;
                case TableFieldType.Bool:
                    column.Bools[row] = (bool)value!;
                    break;
                case TableFieldType.String:
                    column.Strings[row] = (string)value!;
                    break;
                case TableFieldType.IntArray:
                    column.IntArrays[row] = (int[])value!;
                    break;
                case TableFieldType.LongArray:
                    column.LongArrays[row] = (long[])value!;
                    break;
                case TableFieldType.FloatArray:
                    column.FloatArrays[row] = (float[])value!;
                    break;
                case TableFieldType.DoubleArray:
                    column.DoubleArrays[row] = (double[])value!;
                    break;
                case TableFieldType.BoolArray:
                    column.BoolArrays[row] = (bool[])value!;
                    break;
                case TableFieldType.StringArray:
                    column.StringArrays[row] = (string[])value!;
                    break;
            }
        }

        static bool TryParseType(string text, out TableFieldType type)
        {
            var key = text.Trim().Replace(" ", string.Empty).ToLowerInvariant();
            switch (key)
            {
                case "int":
                case "int32":
                case "integer":
                    type = TableFieldType.Int;
                    return true;
                case "long":
                case "int64":
                    type = TableFieldType.Long;
                    return true;
                case "float":
                case "single":
                case "float32":
                    type = TableFieldType.Float;
                    return true;
                case "double":
                case "float64":
                    type = TableFieldType.Double;
                    return true;
                case "bool":
                case "boolean":
                    type = TableFieldType.Bool;
                    return true;
                case "string":
                case "str":
                case "text":
                    type = TableFieldType.String;
                    return true;
                case "int[]":
                case "int32[]":
                case "integer[]":
                    type = TableFieldType.IntArray;
                    return true;
                case "long[]":
                case "int64[]":
                    type = TableFieldType.LongArray;
                    return true;
                case "float[]":
                case "single[]":
                case "float32[]":
                    type = TableFieldType.FloatArray;
                    return true;
                case "double[]":
                case "float64[]":
                    type = TableFieldType.DoubleArray;
                    return true;
                case "bool[]":
                case "boolean[]":
                    type = TableFieldType.BoolArray;
                    return true;
                case "string[]":
                case "str[]":
                case "text[]":
                    type = TableFieldType.StringArray;
                    return true;
                default:
                    type = default;
                    return false;
            }
        }

        static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = name.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalid, chars[i]) >= 0)
                {
                    chars[i] = '_';
                }
            }

            var sanitized = new string(chars).Trim().TrimEnd('.', ' ');
            if (sanitized.Length == 0)
            {
                sanitized = "_";
            }

            if (IsReservedDeviceName(sanitized))
            {
                sanitized += "_table";
            }

            return sanitized;
        }

        static bool IsReservedDeviceName(string name)
        {
            var stem = name;
            var dot = stem.IndexOf('.');
            if (dot >= 0)
            {
                stem = stem.Substring(0, dot);
            }

            switch (stem.ToUpperInvariant())
            {
                case "CON":
                case "PRN":
                case "AUX":
                case "NUL":
                case "COM1":
                case "COM2":
                case "COM3":
                case "COM4":
                case "COM5":
                case "COM6":
                case "COM7":
                case "COM8":
                case "COM9":
                case "LPT1":
                case "LPT2":
                case "LPT3":
                case "LPT4":
                case "LPT5":
                case "LPT6":
                case "LPT7":
                case "LPT8":
                case "LPT9":
                    return true;
                default:
                    return false;
            }
        }

        static void WriteAtomically(string destination, byte[] bytes)
        {
            var tempPath = destination + ".tmp";
            try
            {
                File.WriteAllBytes(tempPath, bytes);
                File.Move(tempPath, destination, overwrite: true);
            }
            catch
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                throw;
            }
        }

        static bool PathsOverlap(string left, string right)
        {
            return IsSameOrNested(left, right) || IsSameOrNested(right, left);
        }

        static bool IsSameOrNested(string parent, string child)
        {
            parent = Path.TrimEndingDirectorySeparator(Path.GetFullPath(parent));
            child = Path.TrimEndingDirectorySeparator(Path.GetFullPath(child));
            if (string.Equals(parent, child, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return child.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        static void FailBatch(ExportReport report, string message)
        {
            report.Failed = 1;
            report.Lines.Add(message);
            report.Lines.Add("完成：成功 0，失败 1");
        }

        sealed class MappedColumn
        {
            public MappedColumn(int columnNumber, string name, TableFieldType type, bool isId)
            {
                ColumnNumber = columnNumber;
                Name = name;
                Type = type;
                IsId = isId;
            }

            public int ColumnNumber { get; }
            public string Name { get; }
            public TableFieldType Type { get; }
            public bool IsId { get; }
        }

        sealed class SheetExportException : Exception
        {
            public SheetExportException(string message) : base(message)
            {
            }
        }
    }
}
