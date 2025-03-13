using System;
using UnityBase.PopUpCore;

namespace UnityBase.Service
{
    public interface IPopUpManager
    {
        public T GetPopUp<T>(bool show = true, float duration = 0.2f, float delay = 0f) where T : PopUp;
        public void HidePopUp(PopUp popUp, float duration = 0.2f, float delay = 0f, Action onComplete = default);
        public void HideAllPopUpOfType<T>(float duration = 0.2f, float delay = 0f, Action onComplete = default) where T : PopUp;
        public void HideAllPopUp(float duration = 0.2f, float delay = 0f);
        public void RemovePopUpPool<T>() where T : PopUp;
    }
}