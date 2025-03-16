using System;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityBase.BootService;
using UnityBase.Service;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;


namespace UnityBase.Manager
{
    public class JsonDataManager : IJsonDataManager, IAppBootService
    {
        private const string DirectoryName = "JsonData";

        private readonly JsonSerializer _jsonSerializer = new();

#if UNITY_EDITOR
        private static string DirectoryPath => $"{Application.dataPath}/{DirectoryName}";
#else
        private static string DirectoryPath => $"{Application.persistentDataPath}/{DirectoryName}";
#endif

        public void Initialize() { }
        
        public bool Save<T>(string key, T data)
        {
            EnsureDirectoryExists();
            
            var filePath = GetFilePath(key);

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            
            using var writer = new StreamWriter(fs, Encoding.UTF8, bufferSize: 8192, leaveOpen: false);
            
            using var jsonWriter = new JsonTextWriter(writer);
            
            _jsonSerializer.Serialize(jsonWriter, data);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                AssetDatabase.Refresh();
#endif
            return true;
        }

        public T Load<T>(string key, T defaultData = default, bool autoSaveDefaultData = true)
        {
            EnsureDirectoryExists();
            
            var filePath = GetFilePath(key);

            if (!File.Exists(filePath))
            {
                if (defaultData is not null && autoSaveDefaultData)
                {
                    Save(key, defaultData);
                }
                return defaultData;
            }

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            using var reader = new StreamReader(fs, Encoding.UTF8, bufferSize: 8192, leaveOpen: false, detectEncodingFromByteOrderMarks: false);
            
            using var jsonReader = new JsonTextReader(reader);
            
            return _jsonSerializer.Deserialize<T>(jsonReader);
        }
        
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(DirectoryPath)) 
                Directory.CreateDirectory(DirectoryPath);
        }

        public static void ClearAllSaveLoadData()
        {
            var files = Directory.GetFiles(DirectoryPath).Select(Path.GetFileName).ToArray();

            foreach (string key in files)
            {
                var filePath = $"{DirectoryPath}/{key}";
                
                File.Delete(filePath);
            }
            
#if UNITY_EDITOR
            if(!Application.isPlaying)
                AssetDatabase.Refresh();
#endif
        }

        private string GetFilePath(string key) => Path.Combine(DirectoryPath, $"{key}.json");
        public void Dispose() { }
    }
}