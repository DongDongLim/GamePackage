using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NPOI.SS.UserModel;
using Packages.ExcelParserPack.Runtime;
using UnityEditor;
using UnityEngine;

namespace Packages.ExcelParserPack.Editor
{
    public class TableBake
    {
        private TableFileGroup _fileGroup;
        private TableFileGroup FileGroup => _fileGroup ??= new TableFileGroup(TableTool.DirectoryLoadTable);
        private readonly List<TableFileBake> _bakeList = new();
        private readonly ExelTableCreate _tableCreate = new();
        
        public IReadOnlyList<TableFileBake> BakeList => _bakeList;
        
        public TableBake()
        {
            _bakeList.Clear();
            var isSuccess = true;
            var editorTableList = _tableCreate.CreateEditorTableList();
            foreach (var (className, fileDic) in editorTableList)
            {
                foreach (var (fileName, sheetNames) in fileDic)
                {
                    var fileBake = new TableFileBake(className);
                    var find = _bakeList.Find(_ => _.ClassName == className);
                    if (find != null)
                    {
                        Debug.LogError($"[TableBake()] have className : {className}");
                        continue;
                    }
                    
                    _bakeList.Add(fileBake);
                    foreach (var sheetName in sheetNames)
                    {
                        if(fileBake.AddFile(FileGroup.AddData(fileName, sheetName)))
                        {
                            continue;
                        }
                        
                        isSuccess = false;
                    }
                }

                if (!isSuccess)
                {
                    EditorUtility.DisplayDialog("Error", "Check For Console Details", "OK");
                }
            }
        }
        
        public bool BakeAll()
        {
            _fileGroup.ResetData();
            var isSuccess = true;
            foreach (var tableFileBake in _bakeList)
            {
                var isSuccessBakeData = tableFileBake.BakeData();
                var isSuccessBakeScript = tableFileBake.BakeScript();

                if (isSuccessBakeData && isSuccessBakeScript)
                {
                    continue;
                }
                
                isSuccess = false;
            }

            return isSuccess;
        }
        
        public bool BakeData(TableFileBake select)
        {
            if (select == null)
            {
                Debug.LogError("[BakeData()] select == null");
                return false;
            }

            return select.BakeData();
        }

        public bool CreateTableScript(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
            {
                EditorUtility.DisplayDialog("Error", "className == null", "OK");
                return false;
            }
            
            var path = $"{TableTool.DirectoryCreateTableScript}/{className}.cs";

            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", $"exist file {path}", "OK");
                return false;
            }
            
            var sb = new StringBuilder();
            sb.Append("using UnityEngine;\n");
            sb.Append("using System;\n");
            sb.Append("using Packages.ExcelParserPack.Runtime;\n");
            sb.Append("[Serializable]\n");
            sb.Append($"public class {className}Record : Record \n");
            sb.Append("{\n");
            sb.Append("\n}\n");
            sb.Append($"public class {className}Table : Table<{className}Record>\n");
            sb.Append("{\n");
            sb.Append($"\tpublic static {className}Table Instance => TableManager.Instance.GetTable<{className}Table>();\n");
            sb.Append("\n}\n");
            
            if(!Util.Save(path, sb.ToString()))
            {
                return false;
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "Create TableScript", "OK");
            return true;
        }
    }
}
