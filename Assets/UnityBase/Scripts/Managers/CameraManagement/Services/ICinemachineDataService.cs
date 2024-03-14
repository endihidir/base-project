using Cinemachine;
using UnityEngine;
using CameraState = UnityBase.Manager.CameraState;

namespace UnityBase.Service
{
    public interface ICinemachineDataService
    {
        public CinemachineVirtualCamera GetVirtualCam(CameraState cameraState);
        public void SetCamFollowTarget(CameraState cameraState, Transform target);
        public void SetCamLookTarget(CameraState cameraState, Transform target);
        public void SetCamFollowTargets(Transform target);
        public void SetCamLookTargets(Transform target);
    }
}