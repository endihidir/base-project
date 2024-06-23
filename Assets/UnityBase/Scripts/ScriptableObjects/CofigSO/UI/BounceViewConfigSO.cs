using DG.Tweening;
using UnityEngine;

namespace UnityBase.UI.Config.SO
{
    [CreateAssetMenu(menuName = "Game/UI/Animation/Configs/BounceConfig")]
    public class BounceViewConfigSO : ScriptableObject
    {
        public float duration;
        public float scaleMultiplier;
        public Ease ease;
        public bool useUnscaledTime;
    }
}