using System;
using Cinemachine;
using UnityBase.Controller;
using UnityBase.ManagerSO;
using UnityBase.Service;
using UnityEngine;

namespace UnityBase.Manager
{
    public class CinemachineManager : ICinemachineDataService, IGameplayPresenterDataService
    {
        public static Action<GameState> OnChangeCamera;

        public static event Action<float, bool> OnCamSwipe;

        private CameraSwipeController _cameraSwipeController;

        private CinemachineStateDrivenCamera _stateDrivenCameras;

        private CinemachineBrain _cinemachineBrain;

        private Animator _animator;

        private CameraState _currentCameraState;

        public CinemachineManager(ManagerDataHolderSO managerDataHolderSo)
        {
            var cinemachineData = managerDataHolderSo.cinemachineManagerSo;
            _stateDrivenCameras = cinemachineData.stateDrivenCameras;
            _cinemachineBrain = cinemachineData.cinemachineBrain;
            _cinemachineBrain.m_UpdateMethod = cinemachineData.cinemachineUpdateMethod;
            _animator = _stateDrivenCameras.GetComponent<Animator>();

            var gameplayVirtualCamera = GetVirtualCam(CameraState.GameplayState);
            _cameraSwipeController = new CameraSwipeController(gameplayVirtualCamera, _stateDrivenCameras.m_CustomBlends.m_CustomBlends[1].m_Blend.BlendTime);
        }

        ~CinemachineManager() => Dispose();

        private void ChangeCamera(GameState gameState)
        {
            _currentCameraState = ConvertGameStateToCameraState(gameState);

            var stateName = _currentCameraState.ToString();

            if (_animator)
                _animator.Play(stateName);
        }

        public void Initialize()
        {
            
        }

        public void Start()
        {
            OnChangeCamera += ChangeCamera;
            
            _cameraSwipeController.Start();
        }

        public void Dispose()
        {
            OnChangeCamera -= ChangeCamera;
            
            _cameraSwipeController.Dispose();
        }

        private CameraState ConvertGameStateToCameraState(GameState to) => to switch
        {
            GameState.GameLoadingState => CameraState.IntroState,
            GameState.GameTutorialState => CameraState.TutorialState,
            GameState.GamePlayState => CameraState.GameplayState,
            GameState.GamePauseState => CameraState.GameplayState,
            GameState.GameSuccessState => CameraState.SuccessState,
            GameState.GameFailState => CameraState.FailState,
            _ => CameraState.None
        };

        public CinemachineVirtualCamera GetVirtualCam(CameraState cameraState)
        {
            var index = (int)cameraState;
            return _stateDrivenCameras.ChildCameras[index] as CinemachineVirtualCamera;
        }

        public void SetCamFollowTarget(CameraState cameraState, Transform target)
        {
            var index = (int)cameraState;
            _stateDrivenCameras.ChildCameras[index].Follow = target;
        }

        public void SetCamLookTarget(CameraState cameraState, Transform target)
        {
            var index = (int)cameraState;
            _stateDrivenCameras.ChildCameras[index].LookAt = target;
        }

        public void SetCamFollowTargets(Transform target)
        {
            var length = _stateDrivenCameras.ChildCameras.Length;

            for (int i = 0; i < length; i++)
            {
                _stateDrivenCameras.ChildCameras[i].Follow = target;
            }
        }

        public void SetCamLookTargets(Transform target)
        {
            var length = _stateDrivenCameras.ChildCameras.Length;

            for (int i = 0; i < length; i++)
            {
                _stateDrivenCameras.ChildCameras[i].LookAt = target;
            }
        }
    }

    public enum CameraState { None = -1, IntroState = 0, TutorialState = 1, GameplayState = 2, SuccessState = 3, FailState = 4 }
}