using Cinemachine;
using UnityBase.BootService;
using UnityBase.GameDataHolder;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CinemachineManager : ICinemachineManager, IGameplayBootService
    {
        private CinemachineStateDrivenCamera _stateDrivenCameras;

        private CinemachineBrain _cinemachineBrain;

        private Animator _animator;

        private Transform _camTargetsDefaultParent, _gameplayCamTarget, _tutorialCamTarget;

        private CameraState _currentCameraState;

        public CinemachineManager(GameDataHolderSO gameDataHolderSo)
        {
            var cinemachineData = gameDataHolderSo.cinemachineManagerSo;
            _stateDrivenCameras = cinemachineData.stateDrivenCameras;
            _cinemachineBrain = cinemachineData.cinemachineBrain;
            _cinemachineBrain.m_UpdateMethod = cinemachineData.cinemachineUpdateMethod;
            _animator = _stateDrivenCameras.GetComponent<Animator>();
            
            _camTargetsDefaultParent = cinemachineData.camTargetsDefaultParent.transform;
            _gameplayCamTarget = cinemachineData.gameplayCamTarget.transform;
            _tutorialCamTarget = cinemachineData.tutorialCamTarget.transform;
        }

        ~CinemachineManager() => Dispose();

        public void Initialize() { }
        public void Dispose() { }

        public void ChangeCamera(CameraState cameraState)
        {
            var stateName = cameraState.ToString();
            
            _animator?.Play(stateName);
        }

        public CinemachineVirtualCamera GetVirtualCam(CameraState cameraState)
        {
            var index = (int)cameraState;
            return _stateDrivenCameras.ChildCameras[index] as CinemachineVirtualCamera;
        }

        public void SetGameplayTargetParent(Transform parent)
        {
            _gameplayCamTarget.SetParent(parent);
            _gameplayCamTarget.localPosition = Vector3.zero;
        }
        public void SetGameplayTargetPosition(Vector3 position) => _gameplayCamTarget.position = position;
        public void SetGameplayTargetLocalPosition(Vector3 position) => _gameplayCamTarget.localPosition = position;
        public void SetGameplayTargetRotation(Quaternion rotation) => _gameplayCamTarget.rotation = rotation;
        public void SetGameplayTargetLocalRotation(Quaternion rotation) => _gameplayCamTarget.localRotation = rotation;
        public void RotateGameplayTarget(float speed, float deltaTime) => _gameplayCamTarget.Rotate(Vector3.up * speed * deltaTime);

        public void ResetGameplayTarget(bool resetInLocal)
        {
            _gameplayCamTarget.SetParent(_camTargetsDefaultParent);
            
            if (resetInLocal)
            {
                _gameplayCamTarget.localPosition = Vector3.zero;
                _gameplayCamTarget.localRotation = Quaternion.identity;
            }
        }
    }
}

public enum CameraState { None = -1, IntroState = 0, TutorialState = 1, GameplayState = 2, SuccessState = 3, FailState = 4 }
