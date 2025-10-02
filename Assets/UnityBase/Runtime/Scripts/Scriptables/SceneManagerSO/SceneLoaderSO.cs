using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/SceneLoaderConfig")]
    public class SceneLoaderSO : ScriptableObject
    {
        [SerializeField] private List<SceneAssetSO> _sceneAssets;

        [SerializeField] private LoadingMenuController _loadingMenuController;
        
        public LoadingMenuController LoadingMenuController => _loadingMenuController;
        public void Initialize()
        {
            _loadingMenuController = FindObjectOfType<LoadingMenuController>();
        }
        
        public List<SceneData> GetSceneData(string sceneName)
        {
            var scene = _sceneAssets.FirstOrDefault(x => x.sceneName == sceneName);

            var sceneGroup = new List<SceneData>();
            
            if (scene is SingleSceneAssetSO singleSceneAssetSo)
            {
                sceneGroup.Add(singleSceneAssetSo.sceneData);
            }
            else if (scene is GroupSceneAssetSO groupSceneAssetSo)
            {
                var sceneDataList = groupSceneAssetSo.sceneDataList;

                foreach (var sceneData in sceneDataList)
                {
                    sceneGroup.Add(sceneData);
                }
            }
            
            return sceneGroup;
        }

    }
    
}