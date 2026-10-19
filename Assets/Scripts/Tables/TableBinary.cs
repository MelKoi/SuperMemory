using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CardTables
{
    public enum TableFieldType : byte
    {
        Int = 1,
        Long = 2,
        Float = 3,
        Double = 4,
        Bool = 5,
        String = 6,
        IntArray = 7,
        LongArray = 8,
        FloatArray = 9,
        DoubleArray = 10,
        BoolArray = 11,
        StringArray = 12,
    }

    public sealed class TableField
    {
        public TableField(string name, TableFieldType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public TableFieldType Type { get; }
    }

    public sealed class TableColumn
    {
        public TableColumn(string name, TableFieldType type, int rowCount)
        {
            Name = name;
            Type = type;
            Ints = Array.Empty<int>();
            Longs = Array.Empty<long>();
            Floats = Array.Empty<float>();
            Doubles = Array.Empty<double>();
            Bools = Array.Empty<bool>();
            Strings = Array.Empty<string>();
            IntArrays = Array.Empty<int[]>();
            LongArrays = Array.Empty<long[]>();
            FloatArrays = Array.Empty<float[]>();
            DoubleArrays = Array.Empty<double[]>();
            BoolArrays = Array.Empty<bool[]>();
            StringArrays = Array.Empty<string[]>();
            switch (type)
            {
                case TableFieldType.Int:
                    Ints = new int[rowCount];
                    break;
                case TableFieldType.Long:
                    Longs = new long[rowCount];
                    break;
                case TableFieldType.Float:
                    Floats = new float[rowCount];
                    break;
                case TableFieldType.Double:
                    Doubles = new double[rowCount];
                    break;
                case TableFieldType.Bool:
                    Bools = new bool[rowCount];
                    break;
                case TableFieldType.String:
                    Strings = new string[rowCount];
                    break;
                case TableFieldType.IntArray:
                    IntArrays = new int[rowCount][];
                    break;
                case TableFieldType.LongArray:
                    LongArrays = new long[rowCount][];
                    break;
                case TableFieldType.FloatArray:
                    FloatArrays = new float[rowCount][];
                    break;
                case TableFieldType.DoubleArray:
                    DoubleArrays = new double[rowCount][];
                    break;
                case TableFieldType.BoolArray:
                    BoolArrays = new bool[rowCount][];
                    break;
                case TableFieldType.StringArray:
                    StringArrays = new string[rowCount][];
                    break;
                default:
                    throw new InvalidDataException("未知字段类型：" + type);
            }
        }

        public string Name { get; }
        public TableFieldType Type { get; }
        public int[] Ints { get; }
        public long[] Longs { get; }
        public float[] Floats { get; }
        public double[] Doubles { get; }
        public bool[] Bools { get; }
        public string[] Strings { get; }
        public int[][] IntArrays { get; }
        public long[][] LongArrays { get; }
        public float[][] FloatArrays { get; }
        public double[][] DoubleArrays { get; }
        public bool[][] BoolArrays { get; }
        public string[][] StringArrays { get; }
    }

    public sealed class TableData
    {
        readonly Dictionary<string, int> fieldIndex;
        readonly Dictionary<string, int> idIndex;

        public TableData(string name, TableColumn[] columns, string[] ids)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidDataException("表名是空的");
            }

            Name = name;
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Ids = ids ?? throw new ArgumentNullException(nameof(ids));
            var fields = new TableField[columns.Length];
            Fields = fields;
            fieldIndex = new Dictionary<string, int>(columns.Length, StringComparer.Ordinal);
            for (var i = 0; i < columns.Length; i++)
            {
                fields[i] = new TableField(columns[i].Name, columns[i].Type);
                if (fieldIndex.ContainsKey(columns[i].Name))
                {
                    throw new InvalidDataException("字段名重复：" + columns[i].Name);
                }

                fieldIndex.Add(columns[i].Name, i);
            }

            idIndex = new Dictionary<string, int>(ids.Length, StringComparer.Ordinal);
            for (var i = 0; i < ids.Length; i++)
            {
                if (string.IsNullOrEmpty(ids[i]))
                {
                    throw new InvalidDataException("存在空的 Id");
                }

                if (idIndex.ContainsKey(ids[i]))
                {
                    throw new InvalidDataException("Id 重复：" + ids[i]);
                }

                idIndex.Add(ids[i], i);
            }
        }

        public string Name { get; }
        public IReadOnlyList<TableField> Fields { get; }
        public IReadOnlyList<TableColumn> Columns { get; }
        public IReadOnlyList<string> Ids { get; }
        public int RowCount => Ids.Count;

        public bool Contains(string id)
        {
            return !string.IsNullOrEmpty(id) && idIndex.ContainsKey(id);
        }

        public bool Contains(int id)
        {
            return Contains(id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public int GetInt(string id, string field) => GetIntAt(RequireRow(id), field);
        public int GetInt(int id, string field) => GetInt(IdText(id), field);
        public long GetLong(string id, string field) => GetLongAt(RequireRow(id), field);
        public long GetLong(int id, string field) => GetLong(IdText(id), field);
        public float GetFloat(string id, string field) => GetFloatAt(RequireRow(id), field);
        public float GetFloat(int id, string field) => GetFloat(IdText(id), field);
        public double GetDouble(string id, string field) => GetDoubleAt(RequireRow(id), field);
        public double GetDouble(int id, string field) => GetDouble(IdText(id), field);
        public bool GetBool(string id, string field) => GetBoolAt(RequireRow(id), field);
        public bool GetBool(int id, string field) => GetBool(IdText(id), field);
        public string GetString(string id, string field) => GetStringAt(RequireRow(id), field);
        public string GetString(int id, string field) => GetString(IdText(id), field);
        public int[] GetIntArray(string id, string field) => GetIntArrayAt(RequireRow(id), field);
        public int[] GetIntArray(int id, string field) => GetIntArray(IdText(id), field);
        public long[] GetLongArray(string id, string field) => GetLongArrayAt(RequireRow(id), field);
        public long[] GetLongArray(int id, string field) => GetLongArray(IdText(id), field);
        public float[] GetFloatArray(string id, string field) => GetFloatArrayAt(RequireRow(id), field);
        public float[] GetFloatArray(int id, string field) => GetFloatArray(IdText(id), field);
        public double[] GetDoubleArray(string id, string field) => GetDoubleArrayAt(RequireRow(id), field);
        public double[] GetDoubleArray(int id, string field) => GetDoubleArray(IdText(id), field);
        public bool[] GetBoolArray(string id, string field) => GetBoolArrayAt(RequireRow(id), field);
        public bool[] GetBoolArray(int id, string field) => GetBoolArray(IdText(id), field);
        public string[] GetStringArray(string id, string field) => GetStringArrayAt(RequireRow(id), field);
        public string[] GetStringArray(int id, string field) => GetStringArray(IdText(id), field);

        public int GetIntAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.Int);
            return column.Ints[rowIndex];
        }

        public long GetLongAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.Long);
            return column.Longs[rowIndex];
        }

        public float GetFloatAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.Float);
            return column.Floats[rowIndex];
        }

        public double GetDoubleAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.Double);
            return column.Doubles[rowIndex];
        }

        public bool GetBoolAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.Bool);
            return column.Bools[rowIndex];
        }

        public string GetStringAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.String);
            return column.Strings[rowIndex];
        }

        public int[] GetIntArrayAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.IntArray);
            return column.IntArrays[rowIndex];
        }

        public long[] GetLongArrayAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.LongArray);
            return column.LongArrays[rowIndex];
        }

        public float[] GetFloatArrayAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.FloatArray);
            return column.FloatArrays[rowIndex];
        }

        public double[] GetDoubleArrayAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.DoubleArray);
            return column.DoubleArrays[rowIndex];
        }

        public bool[] GetBoolArrayAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.BoolArray);
            return column.BoolArrays[rowIndex];
        }

        public string[] GetStringArrayAt(int rowIndex, string field)
        {
            var column = RequireColumn(field, TableFieldType.StringArray);
            return column.StringArrays[rowIndex];
        }

        int RequireRow(string id)
        {
            if (id != null && idIndex.TryGetValue(id, out var row))
            {
                return row;
            }

            throw new InvalidDataException("表「" + Name + "」没有 Id「" + id + "」");
        }

        TableColumn RequireColumn(string field, TableFieldType type)
        {
            if (field == null || !fieldIndex.TryGetValue(field, out var index))
            {
                throw new InvalidDataException("表「" + Name + "」没有字段「" + field + "」");
            }

            var column = Columns[index];
            if (column.Type != type)
            {
                throw new InvalidDataException(
                    "表「" + Name + "」字段「" + field + "」是 " + column.Type + "，不能按 " + type + " 读取");
            }

            return column;
        }

        static string IdText(int id)
        {
            return id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public static class TableBinary
    {
        public const int Version = 1;
        public const int MaxFields = 512;
        public const int MaxRows = 100000;
        public const int MaxArrayLength = 10000;
        public const int MaxStringBytes = 4 * 1024 * 1024;

        static readonly byte[] Magic = { (byte)'C', (byte)'T', (byte)'B', (byte)'L' };

        public static byte[] Write(string tableName, TableColumn[] columns, string[] ids)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            if (columns.Length == 0 || columns.Length > MaxFields)
            {
                throw new InvalidDataException("字段数量无效");
            }

            if (ids.Length > MaxRows)
            {
                throw new InvalidDataException("行数超过 " + MaxRows);
            }

            for (var i = 0; i < columns.Length; i++)
            {
                if (ColumnLength(columns[i]) != ids.Length)
                {
                    throw new InvalidDataException("字段「" + columns[i].Name + "」的行数与 Id 不一致");
                }
            }

            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(Magic);
                writer.Write(Version);
                writer.Write(columns.Length);
                writer.Write(ids.Length);
                WriteUtf8(writer, tableName);
                for (var i = 0; i < columns.Length; i++)
                {
                    WriteUtf8(writer, columns[i].Name);
                    writer.Write((byte)columns[i].Type);
                }

                for (var row = 0; row < ids.Length; row++)
                {
                    WriteUtf8(writer, ids[row]);
                    for (var col = 0; col < columns.Length; col++)
                    {
                        WriteValue(writer, columns[col], row);
                    }
                }
            }

            return stream.ToArray();
        }

        public static TableData Read(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 16)
            {
                throw new InvalidDataException("表文件已损坏");
            }

            using var stream = new MemoryStream(bytes, writable: false);
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            var magic = reader.ReadBytes(4);
            if (magic.Length != 4 || magic[0] != Magic[0] || magic[1] != Magic[1] || magic[2] != Magic[2] || magic[3] != Magic[3])
            {
                throw new InvalidDataException("表文件已损坏");
            }

            var version = reader.ReadInt32();
            if (version != Version)
            {
                throw new InvalidDataException("不支持的表版本：" + version);
            }

            var fieldCount = reader.ReadInt32();
            var rowCount = reader.ReadInt32();
            if (fieldCount <= 0 || fieldCount > MaxFields || rowCount < 0 || rowCount > MaxRows)
            {
                throw new InvalidDataException("表文件已损坏");
            }

            var tableName = ReadUtf8(reader, stream);
            var columns = new TableColumn[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                var fieldName = ReadUtf8(reader, stream);
                var typeValue = reader.ReadByte();
                if (typeValue < (byte)TableFieldType.Int || typeValue > (byte)TableFieldType.StringArray)
                {
                    throw new InvalidDataException("未知字段类型");
                }

                columns[i] = new TableColumn(fieldName, (TableFieldType)typeValue, rowCount);
            }

            var ids = new string[rowCount];
            for (var row = 0; row < rowCount; row++)
            {
                ids[row] = ReadUtf8(reader, stream);
                for (var col = 0; col < fieldCount; col++)
                {
                    ReadValue(reader, stream, columns[col], row);
                }
            }

            if (stream.Position != stream.Length)
            {
                throw new InvalidDataException("表文件已损坏");
            }

            return new TableData(tableName, columns, ids);
        }

        static void WriteValue(BinaryWriter writer, TableColumn column, int row)
        {
            switch (column.Type)
            {
                case TableFieldType.Int:
                    writer.Write(column.Ints[row]);
                    break;
                case TableFieldType.Long:
                    writer.Write(column.Longs[row]);
                    break;
                case TableFieldType.Float:
                    writer.Write(column.Floats[row]);
                    break;
                case TableFieldType.Double:
                    writer.Write(column.Doubles[row]);
                    break;
                case TableFieldType.Bool:
                    writer.Write(column.Bools[row]);
                    break;
                case TableFieldType.String:
                    WriteUtf8(writer, column.Strings[row] ?? string.Empty);
                    break;
                case TableFieldType.IntArray:
                    WriteArray(writer, column.IntArrays[row], (w, value) => w.Write(value));
                    break;
                case TableFieldType.LongArray:
                    WriteArray(writer, column.LongArrays[row], (w, value) => w.Write(value));
                    break;
                case TableFieldType.FloatArray:
                    WriteArray(writer, column.FloatArrays[row], (w, value) => w.Write(value));
                    break;
                case TableFieldType.DoubleArray:
                    WriteArray(writer, column.DoubleArrays[row], (w, value) => w.Write(value));
                    break;
                case TableFieldType.BoolArray:
                    WriteArray(writer, column.BoolArrays[row], (w, value) => w.Write(value));
                    break;
                case TableFieldType.StringArray:
                    var texts = column.StringArrays[row] ?? Array.Empty<string>();
                    writer.Write(texts.Length);
                    for (var i = 0; i < texts.Length; i++)
                    {
                        WriteUtf8(writer, texts[i] ?? string.Empty);
                    }
                    break;
                default:
                    throw new InvalidDataException("未知字段类型");
            }
        }

        static void ReadValue(BinaryReader reader, Stream stream, TableColumn column, int row)
        {
            switch (column.Type)
            {
                case TableFieldType.Int:
                    column.Ints[row] = reader.ReadInt32();
                    break;
                case TableFieldType.Long:
                    column.Longs[row] = reader.ReadInt64();
                    break;
                case TableFieldType.Float:
                    column.Floats[row] = reader.ReadSingle();
                    break;
                case TableFieldType.Double:
                    column.Doubles[row] = reader.ReadDouble();
                    break;
                case TableFieldType.Bool:
                    column.Bools[row] = reader.ReadBoolean();
                    break;
                case TableFieldType.String:
                    column.Strings[row] = ReadUtf8(reader, stream);
                    break;
                case TableFieldType.IntArray:
                    column.IntArrays[row] = ReadArray(reader, stream, r => r.ReadInt32());
                    break;
                case TableFieldType.LongArray:
                    column.LongArrays[row] = ReadArray(reader, stream, r => r.ReadInt64());
                    break;
                case TableFieldType.FloatArray:
                    column.FloatArrays[row] = ReadArray(reader, stream, r => r.ReadSingle());
                    break;
                case TableFieldType.DoubleArray:
                    column.DoubleArrays[row] = ReadArray(reader, stream, r => r.ReadDouble());
                    break;
                case TableFieldType.BoolArray:
                    column.BoolArrays[row] = ReadArray(reader, stream, r => r.ReadBoolean());
                    break;
                case TableFieldType.StringArray:
                    var count = ReadCount(reader, stream);
                    var texts = new string[count];
                    for (var i = 0; i < count; i++)
                    {
                        texts[i] = ReadUtf8(reader, stream);
                    }

                    column.StringArrays[row] = texts;
                    break;
                default:
                    throw new InvalidDataException("未知字段类型");
            }
        }

        static void WriteArray<T>(BinaryWriter writer, T[] values, Action<BinaryWriter, T> writeItem)
        {
            values ??= Array.Empty<T>();
            if (values.Length > MaxArrayLength)
            {
                throw new InvalidDataException("数组长度超过 " + MaxArrayLength);
            }

            writer.Write(values.Length);
            for (var i = 0; i < values.Length; i++)
            {
                writeItem(writer, values[i]);
            }
        }

        static T[] ReadArray<T>(BinaryReader reader, Stream stream, Func<BinaryReader, T> readItem)
        {
            var count = ReadCount(reader, stream);
            var values = new T[count];
            for (var i = 0; i < count; i++)
            {
                values[i] = readItem(reader);
            }

            return values;
        }

        static int ReadCount(BinaryReader reader, Stream stream)
        {
            if (stream.Length - stream.Position < 4)
            {
                throw new InvalidDataException("表文件已损坏");
            }

            var count = reader.ReadInt32();
            if (count < 0 || count > MaxArrayLength)
            {
                throw new InvalidDataException("表文件已损坏");
            }

            return count;
        }

        static int ColumnLength(TableColumn column)
        {
            switch (column.Type)
            {
                case TableFieldType.Int:
                    return column.Ints.Length;
                case TableFieldType.Long:
                    return column.Longs.Length;
                case TableFieldType.Float:
                    return column.Floats.Length;
                case TableFieldType.Double:
                    return column.Doubles.Length;
                case TableFieldType.Bool:
                    return column.Bools.Length;
                case TableFieldType.String:
                    return column.Strings.Length;
                case TableFieldType.IntArray:
                    return column.IntArrays.Length;
                case TableFieldType.LongArray:
                    return column.LongArrays.Length;
                case TableFieldType.FloatArray:
                    return column.FloatArrays.Length;
                case TableFieldType.DoubleArray:
                    return column.DoubleArrays.Length;
                case TableFieldType.BoolArray:
                    return column.BoolArrays.Length;
                case TableFieldType.StringArray:
                    return column.StringArrays.Length;
                default:
                    throw new InvalidDataException("未知字段类型");
            }
        }

        static void WriteUtf8(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            if (bytes.Length > MaxStringBytes)
            {
                throw new InvalidDataException("文本超过长度限制");
            }

            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        static string ReadUtf8(BinaryReader reader, Stream stream)
        {
            if (stream.Length - stream.Position < 4)
            {
                throw new InvalidDataException("表文件已损坏");
            }

            var length = reader.ReadInt32();
            if (length < 0 || length > MaxStringBytes || stream.Length - stream.Position < length)
            {
                throw new InvalidDataException("表文件已损坏");
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new InvalidDataException("表文件已损坏");
            }

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
