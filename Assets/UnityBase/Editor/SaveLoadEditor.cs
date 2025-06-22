using UnityBase.Manager;
using UnityEditor;
using UnityEngine;

namespace UnityBase.Scripts.Editor
{
    public class SaveLoadEditor : EditorWindow
    {
        [MenuItem("Edit/Clear All Save Load Data")]
        private static void ClearAllSaveLoadData()
        {
            SaveManager.ClearAllSaveLoadData();
        }
        
        
        [MenuItem("Edit/Clear All")]
        private static void ClearAll()
        {
            SaveManager.ClearAllSaveLoadData();
            PlayerPrefs.DeleteAll();
        }
    }
}