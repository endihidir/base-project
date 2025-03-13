using DG.Tweening;
using UnityEngine;

namespace UnityBase.UI.Config.SO
{
    [CreateAssetMenu(menuName = "Game/UI/Animation/Configs/MoveInOutConfig")]
    public class MoveInOutViewConfigSO : ScriptableObject
    {
        public Vector2 inPos, outPos;
        public float openDuration, openDelay;
        public float closeDuration, closeDelay;
        public Ease openEase, closeEase;
        public bool useUnscaledTime;
    }
}