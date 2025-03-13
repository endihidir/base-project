using UnityBase.Tag;
using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/TutorialManagerData")]
    public class TutorialActionManagerSO : ScriptableObject
    {
        public Transform tutorialsParent;

        public void Initialize()
        {
            tutorialsParent = FindObjectOfType<Tag_TutorialsParent>()?.transform;
        }
    }
}