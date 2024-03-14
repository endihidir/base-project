using System;
using System.Collections.Generic;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class TaskManager : ITaskDataService, IAppPresenterDataService
    {
        private readonly IDictionary<string, bool> _tasks;
        public static Action<string> OnTaskComplete { get; }

        public TaskManager()
        {
            _tasks = new Dictionary<string, bool>();
        }
        
        public void Initialize() { }
        public void Start() { }

        public void Dispose() { }

        public void CreateTask(string key)
        {
            if (_tasks.ContainsKey(key))
            {
                Debug.LogError($"{key} task can not create twice!!");
                return;
            }
            
            _tasks.Add(key, GetTaskValue(key));
        }

        public bool IsTaskCompleted(string key)
        {
            if (_tasks.TryGetValue(key, out var isCompleted))
            {
                return isCompleted;
            }

            throw new ArgumentException($"You should create {key} before checking the Task!!");
        }

        public void CompleteTask(string key)
        {
            if (!_tasks.TryGetValue(key, out var isCompleted))
                throw new ArgumentException($"You should create {key} before completing the Task!!");

            if(isCompleted) return;
            
            _tasks[key] = true;
            
            SetTaskValue(key, _tasks[key]);

            OnTaskComplete?.Invoke(key);
        }
        
        private bool GetTaskValue(string key) => PlayerPrefs.GetInt(key, 0) == 1;
        private void SetTaskValue(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}