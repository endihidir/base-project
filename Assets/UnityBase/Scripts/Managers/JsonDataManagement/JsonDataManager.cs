using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityBase.Service;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;


namespace UnityBase.Manager
{
    public class JsonDataManager : IJsonDataService, IAppPresenterDataService
    {
        private const string DirectoryName = "JsonData";

#if UNITY_EDITOR
        private static string DirectoryPath => $"{Application.dataPath}/{DirectoryName}";
#else
        private static string DirectoryPath => $"{Application.persistentDataPath}/{DirectoryName}";
#endif
        
        public void Initialize() { }
        public void Start() { }
        
        public bool Save<T>(string key, T data)
        {
            EnsureDirectoryExists();

            var filePath = GetFilePath(key);

            var jsonData = JsonConvert.SerializeObject(data);
            
            File.WriteAllText(filePath, jsonData);

#if UNITY_EDITOR
            if(!Application.isPlaying)
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

            var jsonData = File.ReadAllText(filePath);
                
            var data = JsonConvert.DeserializeObject<T>(jsonData);
            
            return data;
        }

        public async UniTask<bool> SaveAsync<T>(string key, T data)
        {
            EnsureDirectoryExists();

            var filePath = GetFilePath(key);
            
            try
            {
                var jsonData = JsonConvert.SerializeObject(data);
                
                await File.WriteAllTextAsync(filePath, jsonData);
                
#if UNITY_EDITOR
                if(!Application.isPlaying)
                    AssetDatabase.Refresh();
#endif
                
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                
                return false;
            }
        }

        public async UniTask<T> LoadAsync<T>(string key, T defaultData = default, bool autoSaveDefaultData = true)
        {
            EnsureDirectoryExists();

            var filePath = GetFilePath(key);

            if (!File.Exists(filePath))
            {
                if (defaultData is not null && autoSaveDefaultData)
                {
                    await SaveAsync(key, defaultData);
                }

                return defaultData;
            }

            try
            {
                var jsonData = await File.ReadAllTextAsync(filePath);
                
                var data = JsonConvert.DeserializeObject<T>(jsonData);
                
                return data;

            }
            catch (Exception e)
            {
                Debug.Log(e);
                
                return defaultData;
            }
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
            AssetDatabase.Refresh();
#endif
        }

        private string GetFilePath(string key) => Path.Combine(DirectoryPath, $"{key}.json");
        public void Dispose() { }
    }
}