using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Packages.ExcelParserPack.Runtime;
using UnityEngine;

namespace Packages.ExcelParserPack.Editor
{
    public class TableFileBake
    {
        private string _className;
        private readonly HashSet<ExelTableFile> _fileList = new();
        public string ClassName => _className;
        public IReadOnlyCollection<ExelTableFile> FileList => _fileList;
        
        public TableFileBake(string className)
        {
            _className = className;
        }

        public bool BakeData()
        {
            var systemType = GetTypeAssemblies(_className);
            if (systemType == null)
            {
                Debug.LogError("[BakeData()] GetTypeAssemblies() systemType == null");
                return false;
            }
            var asset = Activator.CreateInstance(systemType);
            if (asset == null)
            {
                Debug.LogError("[BakeData()] Activator.CreateInstance() asset == null");
                return false;
            }
            var classFieldArray = systemType.GetFields();
            foreach (var classField in classFieldArray)
            {
                var fileType = classField.FieldType;
                if(!fileType.IsGenericType || fileType.GetGenericTypeDefinition() != typeof(List<>))
                {
                    continue;
                }

                var argumentArray = fileType.GetGenericArguments();
                if (argumentArray.Length != 1)
                {
                    Debug.LogError("[BakeData()] argumentArray.Length != 1");
                    return false;
                }
                
                var argumentType = argumentArray[0];
                var listType = typeof(List<>).MakeGenericType(argumentType);
                var listAddMethode = listType.GetMethod("Add", new Type[] {argumentType});
                var list = Activator.CreateInstance(listType);
                var keyCheckDic = new Dictionary<int, Record>();
                foreach (var exelTableFile in _fileList)
                {
                    var data = exelTableFile.GetData();
                    if (data == null)
                    {
                        Debug.LogError($"no find file : {exelTableFile.FileName} sheet : {exelTableFile.SheetName}");
                        return false;
                    }
                    
                    foreach (var line in data)
                    {
                        var recordObj = (Record)Activator.CreateInstance(argumentType);
                        var recordFieldArray = argumentType.GetFields();
                        var rowClassName = argumentType.ToString();

                        foreach (var recordField in recordFieldArray)
                        {
                            if(!GetRecordValue(out var recordValue, rowClassName, recordField.FieldType, recordField.Name, line))
                            {
                                var recordFieldType = recordField.FieldType;
                                if(recordFieldType.IsGenericType && recordFieldType.GetGenericTypeDefinition() == typeof(List<>))
                                {
                                    var recordArgumentArray = recordFieldType.GetGenericArguments();
                                    if (recordArgumentArray.Length != 1)
                                    {
                                        Debug.LogError("[BakeData()] recordArgumentArray.Length != 1");
                                        continue;
                                    }
                                    
                                    var recordArgumentType = recordArgumentArray[0];
                                    var recordListType = typeof(List<>).MakeGenericType(recordArgumentType);
                                    var recordListAddMethode = recordListType.GetMethod("Add", new Type[] {recordArgumentType});
                                    recordValue = Activator.CreateInstance(recordListType);
                                    
                                    var recordArrayList = GetRecordArrayList(rowClassName, recordArgumentType, recordField.Name, line);
                                    if(recordArrayList == null || recordArrayList.Count == 0)
                                    {
                                        int index = 1;
                                        while (true)
                                        {
                                            var hashKey = TableFileGroup.GetTableHashCode(recordField.Name, index.ToString());
                                            if (!line.ContainsKey(hashKey))
                                            {
                                                break;
                                            }
                                            
                                            GetRecordValue(out var obj, rowClassName, recordArgumentType, hashKey, line);
                                            if (recordValue == null)
                                            {
                                                break;
                                            }
                                            recordListAddMethode.Invoke(recordValue, new object[] {obj});
                                            index++;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var obj in recordArrayList)
                                        {
                                            recordListAddMethode.Invoke(recordValue, new object[] {obj});
                                        }
                                    }
                                }
                                else
                                {
                                    recordValue = GetRecordArrayList(rowClassName, recordFieldType, recordField.Name, line);
                                }
                            }

                            recordField.SetValue(recordObj, recordValue);
                        }
                        
                        if(recordObj == null || recordObj.Id <= 0)
                        {
                            continue;
                        }

                        if (keyCheckDic.ContainsKey(recordObj.Id))
                        {
                            Debug.LogError($"[BakeData() keyCheckDic.ContainsKey] id : {recordObj.Id}");
                            continue;
                        }
                        
                        keyCheckDic.Add(recordObj.Id, recordObj);
                        listAddMethode.Invoke(list, new object[] {recordObj});
                    }
                }

                classField.SetValue(asset, list);
            }
            
            var directory = TableTool.DirectorySaveTable;
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var path = $"{directory}/{_className}.bytes";
            var method = systemType.GetMethod("Save");
            method.Invoke(asset, new object[] {path});
            return true;
        }
        
        private Type GetTypeAssemblies(string typeName)
        {
            var type = Type.GetType(typeName);
            if(type != null)
            {
                return type;
            }
            
            var curAssembly = Assembly.GetExecutingAssembly();
            var assemblyNameArray = curAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in assemblyNameArray)
            {
                var assembly = Assembly.Load(assemblyName);
                type = assembly?.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
        
        private bool GetRecordValue(out object recordValue, string rowClassName, Type type, string sheetName, Dictionary<string, string> data)
        {
            var key = sheetName.ToLower();
            recordValue = null;
            if (!data.ContainsKey(key))
            {
                key += "_1";
                if (data.ContainsKey(key))
                {
                    return false;
                }
            }

            if (type == typeof(int))
            {
                recordValue = Get<int>(rowClassName, data, sheetName);
            }
            else if (type == typeof(string))
            {
                recordValue = Get<string>(rowClassName, data, sheetName);
            }
            else if (type == typeof(float))
            {
                recordValue = Get<float>(rowClassName, data, sheetName);
            }
            else if (type == typeof(long))
            {
                recordValue = Get<long>(rowClassName, data, sheetName);
            }
            else if (type == typeof(bool))
            {
                recordValue = Get<bool>(rowClassName, data, sheetName);
            }
            else if (type == typeof(double))
            {
                recordValue = Get<double>(rowClassName, data, sheetName);
            }
            else if (type.IsEnum)
            {
                var parser = Get<string>(rowClassName, data, sheetName);
                try
                {
                    recordValue = Enum.Parse(type, string.IsNullOrEmpty(parser) ? Enum.GetNames(type)[0] : parser);
                }
                catch (Exception e)
                {
                    Debug.LogError(rowClassName + "::" + type + ", sheetname : " + sheetName + " : " + e.ToString());
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private T Get<T>(string className, Dictionary<string, string> data, string key, T def = default(T))
        {
            key = key.ToLower();
            
            if (!data.ContainsKey(key))
            {
                key += "_1";
                if (data.ContainsKey(key))
                {
                    return def;
                }
                Debug.LogError($"[Get{typeof(T)}] no find key : {key} className : {className}");
            }

            return Get<T>(className, key, data[key], def);
        }
        
        private T Get<T>(string className, string key, string value, T def = default(T))
        {
            if (string.IsNullOrEmpty(value))
            {
                return def;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                Debug.LogError($"[Get{typeof(T)}] className : {className}, key : {key}, value : {value}, Exception : {e}");
                return def;
            }
        }

        private ArrayList GetRecordArrayList(string className, Type type, string sheetName, Dictionary<string, string> data)
        {
            if (type == typeof(int))
            {
                return new ArrayList(GetList<int>(className, data, sheetName));
            }

            if (type == typeof(string))
            {
                return new ArrayList(GetList<string>(className, data, sheetName));
            }

            if (type == typeof(float))
            {
                return new ArrayList(GetList<float>(className, data, sheetName));
            }
            
            if(type == typeof(long))
            {
                return new ArrayList(GetList<long>(className, data, sheetName));
            }
            
            if(type == typeof(bool))
            {
                return new ArrayList(GetList<bool>(className, data, sheetName));
            }
            
            if(type == typeof(double))
            {
                return new ArrayList(GetList<double>(className, data, sheetName));
            }

            return null;
        }

        private List<T> GetList<T>(string className, Dictionary<string, string> data, string key)
        {
            var list = new List<T>();
            var value = Get<string>(className, data, key);
            if (string.IsNullOrEmpty(value))
            {
                return list;
            }

            var firstIndex = Mathf.Clamp(value.IndexOf('[') + 1, 0, value.Length - 1);
            var endIndex = Mathf.Clamp(value.LastIndexOf(']') - 1, 0, value.Length - 1);
            value = value.Substring(firstIndex, endIndex);
            value = value.Replace(" ", "");
            var valueArray = value.Split(',');
            foreach (var valueStr in valueArray)
            {
                list.Add(Get<T>(className, key, valueStr));
            }

            return list;
        }

        public bool BakeScript()
        {
            var isSuccess = true;
            var keyName = TableTool.KeyName.ToLower();
            var path = $"{TableTool.DirectoryCreateTableKeyName}/{_className}_{keyName}.cs";
            var sb = new StringBuilder();
            sb.Append($"public static class {_className}_{keyName}\n");
            sb.Append("{\n");
            var keyHashSet = new HashSet<string>();
            foreach (var exelTableFile in _fileList)
            {
                var data = exelTableFile.GetData();
                if (data == null)
                {
                    isSuccess = false;
                    Debug.LogError($"no find file : {exelTableFile.FileName} sheet : {exelTableFile.SheetName}");
                    continue;
                }
                
                foreach (var line in data)
                {
                    if (!line.TryGetValue(keyName, out var keyValue))
                    {
                        continue;
                    }

                    var key = keyValue.Replace(" ", "");
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    if (!keyHashSet.Add(key))
                    {
                        isSuccess = false;
                        Debug.LogError($"[BakeScript() have key] key : {key}");
                        continue;
                    }

                    sb.Append($"\tpublic const string {key} = \"{key}\";\n");
                }
            }
            sb.Append("\n}");
            var saveSuccess = Util.Save(path, sb.ToString());
            
            return saveSuccess && isSuccess;
        }

        public bool AddFile(ExelTableFile tableFile)
        {
            if (tableFile == null)
            {
                Debug.LogError($"[AddFile()] tableFile == null");
                return false;
            }

            if (_fileList.Add(tableFile))
            {
                return true;
            }
            
            Debug.LogError($"[AddFile()] _fileList.Add() == false");
            return false;
        }
    }
}
    