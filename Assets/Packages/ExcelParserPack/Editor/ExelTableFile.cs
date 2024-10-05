
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using NPOI.SS.UserModel;
    using UnityEngine;

    namespace Packages.ExcelParserPack.Editor
    {
        public class ExelTableFile
        {
            private string _fileName;
            private string _sheetName;
            private string _filePath;
            private FileInfo _fileInfo;
            private List<Dictionary<string, string>> _data;
        
            public string FileName => _fileName;
            public string SheetName => _sheetName;
        
            public ExelTableFile(string directory, string fileName, string sheetName)
            {
                _fileName = fileName.ToLower();
                _sheetName = sheetName.ToLower();
                _filePath = $"{directory}/{fileName}";
            }

            public string Load()
            {
                _data = new List<Dictionary<string, string>>();
            
                var titleRow = 0;
                var dataRow = 1;

                using (var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var book = WorkbookFactory.Create(stream);
                    if (book == null)
                    {
                        return $"[IWorkbook == null] filename : {_fileName}, sheet :{_sheetName}";
                    }

                    ISheet sheet;
                    IRow nameRow;
                    var isError = false;
                    if (string.IsNullOrEmpty(_sheetName))
                    {
                        for (var i = 0; i < book.NumberOfSheets; i++)
                        {
                            sheet = book.GetSheetAt(i);
                            var sheetName = sheet.SheetName;

                            if (sheetName.Contains('@'))
                            {
                                continue;
                            }

                            nameRow = sheet.GetRow(titleRow);
                            if (nameRow == null)
                            {
                                return
                                    $"[title IRow == null] titleRow : {titleRow}, filename : {_fileName}, sheet :{_sheetName}";
                            }


                            foreach (IRow row in sheet)
                            {
                                if (row.RowNum < dataRow)
                                {
                                    continue;
                                }

                                var parserData = GetReadLine(nameRow, row, _fileName, sheetName);
                                if (parserData == null)
                                {
                                    continue;
                                }

                                if (parserData.ContainsKey("error"))
                                {
                                    isError = true;
                                    continue;
                                }

                                _data.Add(parserData);
                            }
                        }

                        if (isError)
                        {
                            return "Check For Console Details";
                        }

                        return string.Empty;
                    }
                
                    sheet = book.GetSheet(_sheetName);
                    if (sheet == null)
                    {
                        return $"[error GetSheet] filename : {_fileName}, sheet :{_sheetName}";
                    }
                
                    nameRow = sheet.GetRow(titleRow);
                    if (nameRow == null)
                    {
                        return $"[title IRow == null] titleRow : {titleRow}, filename : {_fileName}, sheet :{_sheetName}";
                    }
                
                    foreach (IRow row in sheet)
                    {
                        if (row.RowNum < dataRow)
                        {
                            continue;
                        }

                        var parserData = GetReadLine(nameRow, row, _fileName, _sheetName);
                        if (parserData == null)
                        {
                            continue;
                        }

                        if (parserData.ContainsKey("error"))
                        {
                            isError = true;
                        }

                        _data.Add(parserData);
                    }

                    if (isError)
                    {
                        return "Check For Console Details";
                    }

                    return string.Empty;
                }
            }
        
            private Dictionary<string, string> GetReadLine(IRow titleRow, IRow dataRow, string fileName, string sheetName)
            {
                var resultLine = new Dictionary<string, string>();
            
                for (var i = 0; i < titleRow.LastCellNum; i++)
                {
                    var titleCell = titleRow.GetCell(i);
                    var title = titleCell?.StringCellValue?.ToLower() ?? string.Empty;
                
                    if (string.IsNullOrEmpty(title))
                    {
                        continue;
                    }
                
                    var dataCell = dataRow.GetCell(i);
                
                    if(dataCell == null && i == titleRow.RowNum)
                    {
                        return null;
                    }
                
                    var data = dataCell?.StringCellValue.ToLower() ?? string.Empty;
                    resultLine.Add(title, data);
                
                    if(dataCell == null)
                    {
                        continue;
                    }

                    try
                    {
                        var cellType = dataCell.CellType;
                        if (cellType == CellType.Formula)
                        {
                            cellType = dataCell.CachedFormulaResultType;
                        }

                        if (resultLine.ContainsKey(title))
                        {
                            resultLine.TryAdd("error", String.Empty);
                            Debug.LogError($"[GetReadLine() have key] filenmae : {fileName}, sheetname : {sheetName}, key : {title}");
                            continue;
                        }

                        switch (cellType)
                        {
                            case CellType.Numeric:
                                resultLine.Add(title, dataCell.NumericCellValue.ToString(CultureInfo.InvariantCulture));
                                break;
                            case CellType.String:
                                resultLine.Add(title, dataCell.StringCellValue);
                                break;
                            case CellType.Boolean:
                                resultLine.Add(title, dataCell.BooleanCellValue.ToString());
                                break;
                            case CellType.Error:
                                resultLine.Add(title, dataCell.ErrorCellValue.ToString());
                                break;
                            case CellType.Blank:
                                resultLine.Add(title, string.Empty);
                                break;
                            default:
                                resultLine.TryAdd("error", String.Empty);
                                Debug.LogError(
                                    $"Excel File Load Error {fileName}, {sheetName} : {cellType}, key : {title}, idx : {dataCell.RowIndex}");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        resultLine.TryAdd("error", String.Empty);
                        Debug.LogError($"GetReadLine() filenmae : {fileName}, sheetname : {sheetName} [ {e}");
                    }
                }

                return resultLine;
            }
        
            public void ResetData()
            {
                _data = null;
                _fileInfo = null;
            }
        
            public FileInfo GetFileInfo()
            {
                if (_fileInfo == null)
                {
                    _fileInfo = new FileInfo(_filePath);
                }

                return _fileInfo;
            }
        
            public long GetLastWriteTimeTicks()
            {
                return GetFileInfo().LastWriteTime.Ticks;
            }
        
            public List<Dictionary<string, string>> GetData()
            {
                return _data;
            }
        }
    }
