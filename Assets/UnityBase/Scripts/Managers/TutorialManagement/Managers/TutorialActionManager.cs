using System;
using UnityBase.Extensions;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityBase.TutorialCore;
using UnityEngine;

namespace UnityBase.Manager
{
    public class TutorialActionManager : ITutorialActionManagementService, IAppBootService
    {
        private Transform _tutorialsParent;

        private TutorialManagerSO _tutorialManagerSo;

        private readonly IPoolManagementService _poolManagementService;

        public TutorialActionManager(ManagerDataHolderSO managerDataHolderSo, IPoolManagementService poolManagementService)
        {
            _tutorialManagerSo = managerDataHolderSo.tutorialManagerSo;

            _poolManagementService = poolManagementService;

            _tutorialsParent = _tutorialManagerSo.tutorialsParent; ;
        }

        public void Initialize() { }
        public void Dispose() { }

        public T GetTutorial<T>(PositionSpace spawnSpace, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : Tutorial
        {
            var selectedTutorial = _poolManagementService.GetObject<T>(show,duration, delay, onComplete);

            selectedTutorial.transform.SetParent(_tutorialsParent);

            selectedTutorial.SetSpawnSpace(spawnSpace);

            return selectedTutorial;
        }

        public bool TryGetTutorial<T>(PositionSpace spawnSpace, out T tutorial, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : Tutorial
        {
            tutorial = default;

            var poolCount = _poolManagementService.GetPoolCount<T>();

            if (poolCount < 1) return false;

            tutorial = _poolManagementService.GetObject<T>(show, duration, delay, onComplete);

            tutorial.transform.SetParent(_tutorialsParent);

            tutorial.SetSpawnSpace(spawnSpace);

            return true;
        }
     
        public void HideTutorial(Tutorial tutorial, float duration = 0f, float delay = 0f, Action onComplete = default)
        {
            _poolManagementService.HideObject(tutorial, duration, delay, onComplete);
        }

        public void HideAllTutorialOfType<T>(float duration = 0f, float delay = 0f, Action onComplete = default) where T : Tutorial
        {
            _poolManagementService.HideAllObjectsOfType<T>(duration, delay, onComplete);
        }

        public void HideAllTutorials(float duration = 0f, float delay = 0f)
        {
            _poolManagementService.HideAllObjectsOfType<Tutorial>(duration, delay);
        }
        
        public void RemoveTutorialPool<T>() where T : Tutorial => _poolManagementService.RemovePool<T>();
    }
}