using System;
using UnityBase.Extensions;
using UnityBase.TutorialCore;

namespace UnityBase.Service
{
    public interface ITutorialDataService
    { 
        public T GetTutorial<T>(PositionSpace spawnSpace, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default) where T : Tutorial;
        public bool TryGetTutorial<T>(PositionSpace spawnSpace, out T tutorial, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false) where T : Tutorial;
        public void HideTutorial(Tutorial tutorial, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false);
        public void HideAllTutorialOfType<T>(float duration = 0f, float delay = 0f, Action onComplete = default) where T : Tutorial;
        public void HideAllTutorials(float duration = 0f, float delay = 0f);
        public void RemoveTutorialPool<T>(bool readLogs = false) where T : Tutorial;
    }
}