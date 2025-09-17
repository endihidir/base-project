using UnityBase.SaveSystem;
using UnityEditor;
using UnityEngine;

namespace UnityBase.Scripts.Editor
{
    public class SaveLoadEditor : EditorWindow
    {
        [MenuItem("Edit/Clear Json Data")]
        private static void ClearJsonData()
        {
            SaveManager.ClearJsonData();
        }
        
        
        [MenuItem("Edit/Clear All")]
        private static void ClearAll()
        {
            SaveManager.ClearJsonData();
            PlayerPrefs.DeleteAll();
        }
    }
}