using Cinemachine;
using UnityEngine;

namespace UnityBase.Managers.SO
{
    [CreateAssetMenu(fileName = "CinemachineManagerData", menuName = "Game/ManagerData/CinemachineManagerData")]
    public class CinemachineManagerSO : ScriptableObject
    {
        public CinemachineBrain.UpdateMethod cinemachineUpdateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;

        public CinemachineStateDrivenCamera stateDrivenCameras;

        public CinemachineBrain cinemachineBrain;

        public Tag_CamTargetsDefaultParent camTargetsDefaultParent;
        
        public Tag_GameplayCamTarget gameplayCamTarget;

        public Tag_TutorialCamTarget tutorialCamTarget;

        public void Initialize()
        {
            stateDrivenCameras = FindObjectOfType<CinemachineStateDrivenCamera>();
            cinemachineBrain = FindObjectOfType<CinemachineBrain>();
            camTargetsDefaultParent = FindObjectOfType<Tag_CamTargetsDefaultParent>();
            gameplayCamTarget = FindObjectOfType<Tag_GameplayCamTarget>();
            tutorialCamTarget = FindObjectOfType<Tag_TutorialCamTarget>();
        }
    }
}