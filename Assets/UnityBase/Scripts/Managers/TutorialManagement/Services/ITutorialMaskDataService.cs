using System;
using UnityEngine;

namespace UnityBase.Service
{
    public interface ITutorialMaskDataService
    {
        public MaskUI GetMask(Vector3 position, MaskUIData maskUIData, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default);
        public bool TryGetMask(Vector3 position, MaskUIData maskUIData, out MaskUI maskUI, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false);
        public MaskUI[] GetMasks(Vector3[] positions, MaskUIData maskUIData, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default);
        public bool TryGetMasks(Vector3[] positions, MaskUIData maskUIData, out MaskUI[] masks, bool show = true, float duration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false);
        public void HideMask(MaskUI maskUI, float killDuration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false);
        public void HideAllMasks(float killDuration = 0f, float delay = 0f);
        public void RemoveMaskPool<T>(bool readLogs = false) where T : MaskUI;
    }
}