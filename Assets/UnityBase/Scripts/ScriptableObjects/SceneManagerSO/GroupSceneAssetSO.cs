using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(menuName = "Game/SceneManagement/GroupSceneAsset")]
    public class GroupSceneAssetSO : SceneAssetSO
    {
        public List<SceneData> sceneDataList;
    }
}