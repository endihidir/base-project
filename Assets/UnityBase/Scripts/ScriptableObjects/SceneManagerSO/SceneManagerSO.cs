using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/ManagerData/SceneManagerData")]
    public class SceneManagerSO : ScriptableObject
    {
        public SingleSceneAssetSO loadingSceneAssetSo;
        
        [SerializeField] private List<SceneAssetSO> _sceneAssets;
        public List<SceneData> GetSceneData(SceneType sceneType)
        {
            var scene = _sceneAssets.FirstOrDefault(x => x.sceneType == sceneType);

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

        public void Initialize()
        {

        }
    }
    
}

public enum SceneType
{
    Loading,
    MainMenu,
    Gameplay
}