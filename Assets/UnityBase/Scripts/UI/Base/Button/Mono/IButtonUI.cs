using UnityEngine;
using UnityEngine.UI;

namespace UnityBase.UI.ButtonCore
{
    public interface IButtonUI
    {
        public Button Button { get; }
        public Transform Transform { get; }
        public void SetActive(bool value);
        public void SetInteractable(bool value);
        public void SetRaycastTarget(bool value);
    }
}