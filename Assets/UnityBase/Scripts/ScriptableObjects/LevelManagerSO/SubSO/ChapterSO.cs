using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/LevelManagement/ChapterData", order = 0)]
    public class ChapterSO : ScriptableObject
    {
        public int index;

        public LevelSO[] levelData;
    }
}