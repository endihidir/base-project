using System;
using UnityEngine;

namespace UnityBase.Service
{
    public interface ITutorialMaskDataService
    {
        public MaskUI GetMask(Vector3 position, MaskUIData maskUIData);
        public bool TryGetMask(Vector3 position, MaskUIData maskUIData, out MaskUI maskUI, bool readLogs = false);
        public MaskUI[] GetMasks(Vector3[] positions, MaskUIData maskUIData);
        public bool TryGetMasks(Vector3[] positions, MaskUIData maskUIData, out MaskUI[] masks, bool readLogs = false);
        public void HideMask(MaskUI maskUI, float killDuration = 0f, float delay = 0f, Action onComplete = default, bool readLogs = false);
        public void HideAllMasks(float killDuration = 0f, float delay = 0f);
        public void RemoveMask(MaskUI maskUI, bool readLogs = false);
        public void RemoveMaskPool<T>(bool readLogs = false) where T : MaskUI;
    }
}