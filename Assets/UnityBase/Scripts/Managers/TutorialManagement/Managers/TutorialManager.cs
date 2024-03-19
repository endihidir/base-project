using System;
using UnityBase.Extensions;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityBase.TutorialCore;
using UnityEngine;

namespace UnityBase.Manager
{
    public class TutorialManager : ITutorialDataService, IAppPresenterDataService
    {
        private Transform _tutorialsParent;

        private TutorialManagerSO _tutorialManagerSo;

        private readonly IPoolDataService _poolDataService;

        public TutorialManager(ManagerDataHolderSO managerDataHolderSo, IPoolDataService poolDataService)
        {
            _tutorialManagerSo = managerDataHolderSo.tutorialManagerSo;

            _poolDataService = poolDataService;

            _tutorialsParent = _tutorialManagerSo.tutorialsParent; ;
        }

        public void Initialize() { }
        public void Start() { }
        public void Dispose() { }

        public T GetTutorial<T>(PositionSpace spawnSpace, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : Tutorial
        {
            var selectedTutorial = _poolDataService.GetObject<T>(show,duration, delay, onComplete);

            selectedTutorial.transform.SetParent(_tutorialsParent);

            selectedTutorial.SetSpawnSpace(spawnSpace);

            return selectedTutorial;
        }

        public bool TryGetTutorial<T>(PositionSpace spawnSpace, out T tutorial, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false) where T : Tutorial
        {
            tutorial = default;

            var poolCount = _poolDataService.GetPoolCount<T>(readLogs);

            if (poolCount < 1) return false;

            tutorial = _poolDataService.GetObject<T>(show, duration, delay, onComplete);

            tutorial.transform.SetParent(_tutorialsParent);

            tutorial.SetSpawnSpace(spawnSpace);

            return true;
        }
     
        public void HideTutorial(Tutorial tutorial, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false)
        {
            _poolDataService.HideObject(tutorial, duration, delay, onComplete, readLogs);
        }

        public void HideAllTutorialOfType<T>(float duration = 0f, float delay = 0f, Action onComplete = default) where T : Tutorial
        {
            _poolDataService.HideAllObjectsOfType<T>(duration, delay, onComplete);
        }

        public void HideAllTutorials(float duration = 0f, float delay = 0f)
        {
            _poolDataService.HideAllObjectsOfType<Tutorial>(duration, delay);
        }
        
        public void RemoveTutorialPool<T>(bool readLogs = false) where T : Tutorial => _poolDataService.RemovePool<T>(readLogs);
    }
}