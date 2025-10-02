using UnityBase.Managers.SO;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityBase.GameDataHolder
{
    //[CreateAssetMenu(menuName = "Game/GameData/GameDataHolder")]
    public class GameDataHolderSO : ScriptableObject
    {
        [Header("Manager SO")]
        public GameManagerSO gameManagerSo;
        [FormerlySerializedAs("sceneGroupManagerSo")] public SceneLoaderSO sceneLoaderSo;
        public LevelManagerSO levelManagerSo;
        public CinemachineManagerSO cinemachineManagerSo;
        public PoolManagerSO poolManagerSo;
        public PopUpManagerSO popUpManagerSo;
        public TutorialActionManagerSO tutorialActionManagerSo;
        public TutorialMaskManagerSO tutorialMaskManagerSo;
        public TutorialProcessManagerSO tutorialProcessManagerSo;

        public void Initialize()
        {
            gameManagerSo.Initialize();
            sceneLoaderSo.Initialize();
            levelManagerSo.Initialize();
            cinemachineManagerSo.Initialize();
            poolManagerSo.Initialize();
            popUpManagerSo.Initialize();
            tutorialActionManagerSo.Initialize();
            tutorialMaskManagerSo.Initialize();
            tutorialProcessManagerSo.Initialize();
        }
    }
}