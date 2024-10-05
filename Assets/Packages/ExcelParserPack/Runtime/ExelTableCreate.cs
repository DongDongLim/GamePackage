using System;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.ExcelParserPack.Runtime
{
    public class ExelTableCreate
    {
        public Dictionary<Type, ITable> CreateTableList()
        {
            var tableList = new Dictionary<Type, ITable>();
            return tableList;
        }
        
        private void AddTable<T> (ref Dictionary<Type, ITable> list) where T : ITable, new()
        {
            var type = typeof(T);
            if (list.ContainsKey(type))
            {
                Debug.LogError($"have : {type}");
                return;
            }
            list.Add(type, new T());
        }
        
        public Dictionary<string, Dictionary<string, HashSet<string>>> CreateEditorTableList()
        {
            var editorTableList = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            return editorTableList;
        }
        
        private void AddEditorTable(ref Dictionary<string, Dictionary<string, HashSet<string>>> list, string className, string excelFileName, string sheetName)
        {
            list.TryAdd(className, new Dictionary<string, HashSet<string>>());
            list[className].TryAdd(excelFileName, new HashSet<string>());
            if (!list[className][excelFileName].Add(sheetName))
            {
                Debug.LogError($"have sheet : {className}, {excelFileName}, {sheetName}");
            }
        }
    }
}
