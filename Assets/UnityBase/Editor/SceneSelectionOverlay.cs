using System.IO;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

[Overlay(typeof(SceneView), "Scene Selection")]
//[Icon(k_icon)]
public class SceneSelectionOverlay : ToolbarOverlay
{
    //public const string k_icon = "Assets/Editor/Icons/UnityIcon.png";

    SceneSelectionOverlay() : base(SceneDropdownToggle.k_id) { }

    [EditorToolbarElement(k_id, typeof(SceneView))]
    class SceneDropdownToggle : EditorToolbarDropdownToggle, IAccessContainerWindow
    {
        public const string k_id = "SceneSelectionOverlay/SceneDropdownToggle";
        public EditorWindow containerWindow { get; set; }
        SceneDropdownToggle() 
        { 
            text= "Scenes";
            tooltip = "Select a scene to load";
            //icon = AssetDatabase.LoadAssetAtPath<Texture2D>(SceneSelectionOverlay.k_icon);

            dropdownClicked += ShowSceneMenu;
        }

        private void ShowSceneMenu()
        {
            GenericMenu menu = new GenericMenu();

            Scene currentScene = EditorSceneManager.GetActiveScene();

            string[] sceneGuids = AssetDatabase.FindAssets("t:scene", new string[] { "Assets" });

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);

                string name = Path.GetFileNameWithoutExtension(path);

                menu.AddItem(new GUIContent(name), string.Compare(currentScene.name, name) == 0, () => OpenScene(currentScene, path));
            }

            menu.ShowAsContext();
        }

        void OpenScene(Scene currentScene, string path)
        {
            if (currentScene.isDirty)
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.OpenScene(path);
            }
            else
            {
                EditorSceneManager.OpenScene(path);
            }
        }
    }
}
