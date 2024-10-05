using System;
using System.Collections.Generic;
using UnityEngine;

namespace Packages.ExcelParserPack.Runtime
{
    public class TableManager
    {
        private static TableManager _instance;
        public static TableManager Instance => _instance ??= new TableManager();
    
        private Dictionary<Type, ITable> _tableList;
        
        private TableManager()
        {
            Load();
        }

        public void Load()
        {
            _tableList = new ExelTableCreate().CreateTableList();
            foreach (var (tableType, table) in _tableList)
            {
                table.Load($"Table/{tableType}");
            }
        }
        
        public T GetTable<T>() where T : class, ITable
        {
            var type = typeof(T);

            if (_tableList.TryGetValue(type, out var table))
            {
                return table as T;
            }
            
            Debug.LogError($"TableManager::GetTable() : {type}");
            return null;
        }
    }
}
