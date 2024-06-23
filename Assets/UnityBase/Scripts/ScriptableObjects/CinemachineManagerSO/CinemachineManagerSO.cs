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

        public void Initialize()
        {
            stateDrivenCameras = FindObjectOfType<CinemachineStateDrivenCamera>();
            cinemachineBrain = FindObjectOfType<CinemachineBrain>();
        }
    }
}