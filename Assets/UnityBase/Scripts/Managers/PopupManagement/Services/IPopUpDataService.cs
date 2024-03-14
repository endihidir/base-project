using System;
using UnityBase.PopUpCore;

namespace UnityBase.Service
{
    public interface IPopUpDataService
    {
        public T GetPopUp<T>(float duration = 0.2f, float delay = 0f) where T : PopUp;
        public void HidePopUp(PopUp popUp, float duration = 0.2f, float delay = 0f, Action onComplete = default, bool readLogs = false);
        public void HideAllPopUpOfType<T>(float duration = 0.2f, float delay = 0f, Action onComplete = default, bool readLogs = false) where T : PopUp;
        public void HideAllPopUp(float duration = 0.2f, float delay = 0f);
        public void RemovePopUp(PopUp popUp, bool readLogs = false);
        public void RemovePopUpPool<T>(bool readLogs = false) where T : PopUp;
    }
}