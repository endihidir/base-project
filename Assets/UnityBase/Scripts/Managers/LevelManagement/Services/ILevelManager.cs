using UnityBase.Managers.SO;

namespace UnityBase.Service
{
    public interface ILevelManager
    {
        public int LastSelectedChapterIndex { get; }
        public int LastSelectedLevelIndex { get; }
        public int LastUnlockedChapterIndex { get; }
        public int LastUnlockedLevelIndex { get; }
        public int LevelText { get; }
        public ChapterSO GetLastUnlockedChapterData();
        public LevelSO GetSelectedLevelData();
        public LevelSO GetCurrentLevelData();
        public ChapterSO GetCurrentChapterData();
    }
}