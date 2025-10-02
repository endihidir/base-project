#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(menuName = "Game/SceneManagement/SingleSceneAsset")]
    public class SingleSceneAssetSO : SceneAssetSO
    {
        public SceneData sceneData;
 
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (sceneData == null) return;
            if (sceneName.Equals(sceneData.Name)) return; 
            sceneName = sceneData.Name;       
            EditorUtility.SetDirty(this);
        }
#endif
    }
}