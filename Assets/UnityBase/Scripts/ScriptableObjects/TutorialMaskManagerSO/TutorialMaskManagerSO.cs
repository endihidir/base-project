using UnityBase.Tag;
using UnityEngine;
using UnityEngine.UI;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/TutorialMaskManagerData")]
    public class TutorialMaskManagerSO : ScriptableObject
    {
        public GameObject maskRoot;

        public Transform maskUIPool;

        public Image maskFadePanel;

        public void Initialize()
        {
            var tagMask = FindObjectOfType<Tag_TutorialMask>();

            maskRoot = tagMask?.maskRoot;

            maskUIPool = tagMask?.maskUIPool;

            maskFadePanel = tagMask?.maskFadePanel;
        }
    }
}
