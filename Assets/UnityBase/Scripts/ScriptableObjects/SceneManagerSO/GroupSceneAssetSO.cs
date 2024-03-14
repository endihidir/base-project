using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.ManagerSO
{
    [CreateAssetMenu(menuName = "Game/SceneManagement/GroupSceneAsset")]
    public class GroupSceneAssetSO : SceneAssetSO
    {
        public List<SceneData> sceneDataList;
    }
}