using UnityEngine;

namespace UnityBase.ManagerSO
{
    //[CreateAssetMenu(menuName = "Game/GameData/ManagerDataHolder")]
    public class ManagerDataHolderSO : ScriptableObject
    {
        public GameManagerSO gameManagerSo;

        public SceneManagerSO sceneManagerSo;

        public LevelManagerSO levelManagerSo;

        public CurrencyManagerSO currencyManagerSo;

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
            currencyManagerSo.Initialize();
            cinemachineManagerSo.Initialize();
            poolManagerSo.Initialize();
            popUpManagerSo.Initialize();
            tutorialManagerSo.Initialize();
            tutorialMaskManagerSo.Initialize();
            tutorialStepManagerSo.Initialize();
        }
    }
}