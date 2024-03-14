using UnityBase.Tag;
using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/TutorialManagerData")]
    public class TutorialManagerSO : ScriptableObject
    {
        public Transform tutorialsParent;

        public void Initialize()
        {
            tutorialsParent = FindObjectOfType<Tag_TutorialsParent>()?.transform;
        }
    }
}