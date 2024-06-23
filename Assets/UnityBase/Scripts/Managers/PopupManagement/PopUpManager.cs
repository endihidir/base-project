using System;
using UnityBase.GameDataHolder;
using UnityBase.PopUpCore;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class PopUpManager : IPopUpManager, IAppBootService
    {
        private Transform _popUpParent, _settingsPopUpParent;

        private readonly IPoolManager _poolManager;

        public PopUpManager(GameDataHolderSO gameDataHolderSo, IPoolManager poolManager)
        {
            _poolManager = poolManager;
            _popUpParent = gameDataHolderSo.popUpManagerSo.popUpParent;
            _settingsPopUpParent = gameDataHolderSo.popUpManagerSo.settingsPopUpParent;
        }

        public void Initialize() { }
        public void Dispose() { }
        
        public T GetPopUp<T>(bool show = true, float duration = 0.2f, float delay = 0f) where T : PopUp
        {
            var pos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);

            var popUp = _poolManager.GetObject<T>(show, duration, delay);

            popUp.transform.position = pos;

            popUp.transform.SetParent(popUp.IsSettingsPopUp ? _settingsPopUpParent : _popUpParent);

            popUp.ResetPopUpSize();

            popUp.transform.SetAsLastSibling();

            return popUp;
        }

        public void HidePopUp(PopUp popUp, float duration = 0.2f, float delay = 0f, Action onComplete = default)
        {
            _poolManager.HideObject(popUp, duration, delay, onComplete);
        }
        
        public void HideAllPopUpOfType<T>(float duration = 0.2f, float delay = 0f, Action onComplete = default) where T : PopUp
        {
            _poolManager.HideAllObjectsOfType<T>(duration, delay, onComplete);
        }
        
        public void HideAllPopUp(float duration = 0.2f, float delay = 0f)
        {
            _poolManager.HideAllObjectsOfType<PopUp>(duration, delay);
        }
        
        public void RemovePopUpPool<T>() where T : PopUp => _poolManager.RemovePool<T>();
    }
}