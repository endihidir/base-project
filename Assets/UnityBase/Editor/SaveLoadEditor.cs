using UnityBase.Manager;
using UnityEditor;

namespace UnityBase.Scripts.Editor
{
    public class SaveLoadEditor : EditorWindow
    {
        [MenuItem("Edit/Clear All Save Load Data")]
        private static void ClearAllSaveLoadData()
        {
            JsonDataManager.ClearAllSaveLoadData();
        }
    }
}