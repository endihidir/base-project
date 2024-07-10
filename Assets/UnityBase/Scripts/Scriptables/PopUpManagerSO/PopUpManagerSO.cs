using UnityBase.Tag;
using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/PopUpManagerData")]
    public class PopUpManagerSO : ScriptableObject
    {
        public Transform popUpParent;

        public Transform settingsPopUpParent;

        public void Initialize()
        {
            popUpParent = FindObjectOfType<Tag_PopUpCanvas>()?.transform;
        }
    }
}
