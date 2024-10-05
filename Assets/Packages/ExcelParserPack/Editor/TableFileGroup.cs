using System.Collections.Generic;

namespace Packages.ExcelParserPack.Editor
{
    public class TableFileGroup
    {
        private readonly string _directory;
        private readonly Dictionary<string, ExelTableFile> _fileList = new();
        
        public TableFileGroup(string directory)
        {
            _directory = directory;
        }
        
        public static string GetTableHashCode(string fileName, string sheetName)
        {
            return $"{fileName}{sheetName}".ToLower();
        }
        
        public List<Dictionary<string, string>> GetData(string fileName, string sheetName)
        {
            var hashCode = GetTableHashCode(fileName, sheetName);
            _fileList.TryGetValue(hashCode, out var tableFile);
            return tableFile?.GetData();
        }
        
        public long GetLastWriteTime(string fileName, string sheetName)
        {
            var hashCode = GetTableHashCode(fileName, sheetName);
            _fileList.TryGetValue(hashCode, out var tableFile);
            return tableFile?.GetLastWriteTimeTicks() ?? 0;
        }
        
        public ExelTableFile AddData(string fileName, string sheetName)
        {
            var hashCode = GetTableHashCode(fileName, sheetName);
            if (_fileList.TryGetValue(hashCode, out var tableFile))
            {
                return tableFile;
            }
            
            tableFile = new ExelTableFile(_directory, fileName, sheetName);
            _fileList.Add(hashCode, tableFile);
            return tableFile;
        }
        
        public void ResetData()
        {
            foreach (var exelTableFile in _fileList.Values)
            {
                exelTableFile.ResetData();
            }
        }
    }
}