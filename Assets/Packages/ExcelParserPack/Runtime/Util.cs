using System;
using System.IO;
using UnityEngine;

namespace Packages.ExcelParserPack.Runtime
{
    public static class Util
    {
        public static bool Save(string path, string data)
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(data))
            {
                Debug.LogError("Util::Save()[ path or data is null ]");
                return false;
            }
            
            try
            {
                var directory = Path.GetDirectoryName(path);

                if (string.IsNullOrWhiteSpace(directory))
                {
                    throw new NullReferenceException();
                }
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllText(path, data);
            }
            catch(Exception e)
            {
                Debug.LogError($"FileUtil::Save() [Exception] path : {path}, Exception {e}");
                
                return false;
            }

            return true;
        }

        public static string Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogError("Util::Load()[ path is null ]");
                return null;
            }

            if (!File.Exists(path))
            {
                Debug.LogError($"Util::Load() path : {path}");
                return null;
            }
            
            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"Util::Load() [Exception] path : {path}, Exception : {e}");
                return null;
            }
        }
    }
}