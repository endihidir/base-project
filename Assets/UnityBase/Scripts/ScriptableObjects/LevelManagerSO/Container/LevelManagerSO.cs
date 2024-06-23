using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/LevelManagerData")]
    public class LevelManagerSO : ScriptableObject
    {
        public ChapterSO[] chapterData;

        public int defaultUnlockedChapterIndex = 0;

        public int defaultUnlockedLevelIndex = 0;

        public void Initialize()
        {

        }
    }
}