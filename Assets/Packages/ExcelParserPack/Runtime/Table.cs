
    using System.Collections.Generic;
    using UnityEngine;

    namespace Packages.ExcelParserPack.Runtime
    {
        public class Table<T> : ITable where T : Record, new()
        {
            public class CompareRecord : IComparer<T>
            {
                public int Compare(T left, T right) => left.Id.CompareTo(right.Id);
            }
            public List<T> RecordList = new();
            private readonly T _search = new();
            private readonly CompareRecord _comparer = new();
        
            public void Sort()
            {
                RecordList.Sort(_comparer);
            }
        
            public void Clear()
            {
                RecordList.Clear();
            }
        
            public T Get(int id, bool showLog = true)
            {
                _search.Id = id;
                var searchIndex = RecordList.BinarySearch(_search, _comparer);
                if (searchIndex < 0)
                {
                    if (showLog)
                    {
                        Debug.LogError($"{typeof(T)} : {id}");
                    }
                    return null;
                }
                return RecordList[searchIndex];
            }
        
            public bool IsHas(int id)
            {
                _search.Id = id;
                return RecordList.BinarySearch(_search, _comparer) >= 0;
            }
        
            public bool Add(T record)
            {
                if (IsHas(record.Id))
                {
                    Debug.LogError($"{typeof(T)}::Add() have id {record.Id}");
                    return false;
                }
                RecordList.Add(record);
                Sort();
                return true;
            }
        
            public bool Delete(T record)
            {
                if(!IsHas(record.Id))
                {
                    Debug.LogError($"{typeof(T)}::Delete() not have id {record.Id}");
                    return false;
                }
            
                RecordList.Remove(record);
                Sort();
                return true;
            }
        
            public void Load(string path)
            {
                var data = Util.Load(path);
                RecordList = JsonUtility.FromJson<List<T>>(data);
                Sort();
            }

            public void Save(string path)
            {
                var data = JsonUtility.ToJson(RecordList);
                Util.Save(path, data);
            }
        }
    }