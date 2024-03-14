using UnityEngine;

namespace UnityBase.ManagerSO
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