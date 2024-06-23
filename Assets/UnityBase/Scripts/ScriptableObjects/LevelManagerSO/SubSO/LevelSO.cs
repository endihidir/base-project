using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(menuName = "Game/LevelManagement/LevelData", order = 1)]
    public class LevelSO : ScriptableObject
    {
        public int index;
    }
}