using System;
using UnityBase.ManagerSO;
using UnityBase.PopUpCore;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class PopUpManager : IPopUpDataService, IAppPresenterDataService
    {
        private Transform _popUpParent, _settingsPopUpParent;

        private readonly IPoolDataService _poolDataService;

        public PopUpManager(ManagerDataHolderSO managerDataHolderSo, IPoolDataService poolDataService)
        {
            _poolDataService = poolDataService;
            _popUpParent = managerDataHolderSo.popUpManagerSo.popUpParent;
            _settingsPopUpParent = managerDataHolderSo.popUpManagerSo.settingsPopUpParent;
        }

        public void Initialize() { }
        public void Start() { }
        public void Dispose() { }
        
        public T GetPopUp<T>(bool show = true, float duration = 0.2f, float delay = 0f) where T : PopUp
        {
            var pos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);

            var popUp = _poolDataService.GetObject<T>(show, duration, delay);

            popUp.transform.position = pos;

            popUp.transform.SetParent(popUp.IsSettingsPopUp ? _settingsPopUpParent : _popUpParent);

            popUp.ResetPopUpSize();

            popUp.transform.SetAsLastSibling();

            return popUp;
        }

        public void HidePopUp(PopUp popUp, float duration = 0.2f, float delay = 0f, Action onComplete = default, bool readLogs = false)
        {
            _poolDataService.HideObject(popUp, duration, delay, onComplete, readLogs);
        }
        
        public void HideAllPopUpOfType<T>(float duration = 0.2f, float delay = 0f, Action onComplete = default) where T : PopUp
        {
            _poolDataService.HideAllObjectsOfType<T>(duration, delay, onComplete);
        }
        
        public void HideAllPopUp(float duration = 0.2f, float delay = 0f)
        {
            _poolDataService.HideAllObjectsOfType<PopUp>(duration, delay);
        }
        
        public void RemovePopUpPool<T>(bool readLogs = false) where T : PopUp => _poolDataService.RemovePool<T>(readLogs);
    }
}