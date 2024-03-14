using System;
using UnityBase.Extensions;
using UnityBase.TutorialCore;

namespace UnityBase.Service
{
    public interface ITutorialDataService
    { 
        public T GetTutorial<T>(PositionSpace spawnSpace) where T : Tutorial;
        public bool TryGetTutorial<T>(PositionSpace spawnSpace, out T tutorial, bool readLogs = false) where T : Tutorial;
        public void HideTutorial(Tutorial tutorial, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false);
        public void HideAllTutorialOfType<T>(float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false) where T : Tutorial;
        public void HideAllTutorials(float duration = 0f, float delay = 0f);
        public void RemoveTutorial(Tutorial tutorial, bool readLogs = false);
        public void RemoveTutorialPool<T>(bool readLogs = false) where T : Tutorial;
    }
}