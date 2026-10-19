using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CardTables
{
    public static class TableDatabase
    {
        static Dictionary<string, TableData> tables;
        static List<string> loadErrors = new List<string>();

        public static IReadOnlyList<string> LoadErrors
        {
            get
            {
                EnsureLoaded();
                return loadErrors;
            }
        }

        public static IReadOnlyCollection<string> TableNames
        {
            get
            {
                EnsureLoaded();
                return tables.Keys;
            }
        }

        public static void Reload()
        {
            var next = new Dictionary<string, TableData>(System.StringComparer.Ordinal);
            var errors = new List<string>();
            var directory = Path.Combine(Application.streamingAssetsPath, "Tables");
            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, "*.bytes");
                System.Array.Sort(files, System.StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    try
                    {
                        var data = TableBinary.Read(File.ReadAllBytes(file));
                        if (next.ContainsKey(data.Name))
                        {
                            errors.Add("表名重复：" + data.Name + "（" + Path.GetFileName(file) + "）");
                            continue;
                        }

                        next.Add(data.Name, data);
                    }
                    catch (System.Exception exception)
                    {
                        errors.Add(Path.GetFileName(file) + "：" + exception.Message);
                    }
                }
            }

            tables = next;
            loadErrors = errors;
            for (var i = 0; i < errors.Count; i++)
            {
                Debug.LogError("读表失败：" + errors[i]);
            }
        }

        public static bool TryGet(string tableName, out TableData table)
        {
            EnsureLoaded();
            if (tableName != null && tables.TryGetValue(tableName, out table))
            {
                return true;
            }

            table = null;
            return false;
        }

        public static TableData Get(string tableName)
        {
            if (TryGet(tableName, out var table))
            {
                return table;
            }

            throw new InvalidDataException("没有找到表「" + tableName + "」");
        }

        static void EnsureLoaded()
        {
            if (tables == null)
            {
                Reload();
            }
        }
    }
}
