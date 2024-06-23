using UnityBase.Managers.SO;
using UnityEngine;

namespace UnityBase.GameDataHolder
{
    //[CreateAssetMenu(menuName = "Game/GameData/GameDataHolder")]
    public class GameDataHolderSO : ScriptableObject
    {
        [Header("Manager SO")]
        public GameManagerSO gameManagerSo;
        public SceneManagerSO sceneManagerSo;
        public LevelManagerSO levelManagerSo;
        public CinemachineManagerSO cinemachineManagerSo;
        public PoolManagerSO poolManagerSo;
        public PopUpManagerSO popUpManagerSo;
        public TutorialManagerSO tutorialManagerSo;
        public TutorialMaskManagerSO tutorialMaskManagerSo;
        public TutorialStepManagerSO tutorialStepManagerSo;

        public void Initialize()
        {
            gameManagerSo.Initialize();
            sceneManagerSo.Initialize();
            levelManagerSo.Initialize();
            cinemachineManagerSo.Initialize();
            poolManagerSo.Initialize();
            popUpManagerSo.Initialize();
            tutorialManagerSo.Initialize();
            tutorialMaskManagerSo.Initialize();
            tutorialStepManagerSo.Initialize();
        }
    }
}