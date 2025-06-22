using Cinemachine;
using UnityEngine;

namespace UnityBase.Service
{
    public interface ICinemachineManager
    {
        public void ChangeCamera(CameraState cameraState);
        public CinemachineVirtualCamera GetVirtualCam(CameraState cameraState);
        public void SetGameplayTargetParent(Transform parent);
        public void SetGameplayTargetPosition(Vector3 position);
        public void SetGameplayTargetLocalPosition(Vector3 position);
        public void SetGameplayTargetRotation(Quaternion rotation);
        public void SetGameplayTargetLocalRotation(Quaternion rotation);
        public void RotateGameplayTarget(float speed, float deltaTime);
        public void ResetGameplayTarget(bool resetInLocal);
    }
}