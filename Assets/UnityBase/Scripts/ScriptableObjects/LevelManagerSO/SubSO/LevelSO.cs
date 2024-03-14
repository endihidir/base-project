using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/LevelManagement/LevelData", order = 1)]
    public class LevelSO : ScriptableObject
    {
        public int index;
    }
}