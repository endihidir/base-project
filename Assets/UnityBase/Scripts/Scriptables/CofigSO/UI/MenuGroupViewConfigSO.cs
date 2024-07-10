using DG.Tweening;
using UnityEngine;

namespace UnityBase.UI.Config.SO
{
    [CreateAssetMenu(menuName = "Game/UI/Animation/Configs/MenuGroup")]
    public class MenuGroupViewConfigSO : ScriptableObject
    {
        public float openDuration = 0.5f;
        public float closeDuration = 0.5f;
        public float openDelay;
        public float closeDelay;
        public Ease ease = Ease.InOutQuad;
    }
}