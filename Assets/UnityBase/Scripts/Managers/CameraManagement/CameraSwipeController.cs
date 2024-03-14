using Cinemachine;
using DG.Tweening;
using UnityBase.Extensions;
using UnityBase.Manager;
using UnityEngine;

namespace UnityBase.Controller
{
    public class CameraSwipeController
    {
        private bool _isMapScrollingActivated;

        private float _defaultCamXOffset, _defaultTargetXOffset;

        private CinemachineVirtualCamera _gameplayVirtualCamera;

        private CinemachineTransposer _gameplayCameraTransposer;

        private Tween _gameplayCamTransitionTw;

        private Transform _followTarget;

        private float _blendTime;

        public CameraSwipeController(CinemachineVirtualCamera gameplayVirtualCamera, float blendTime)
        {
            _gameplayVirtualCamera = gameplayVirtualCamera;

            _blendTime = blendTime;

            _followTarget = _gameplayVirtualCamera.m_Follow;

            _gameplayCameraTransposer = _gameplayVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        }

        public void Start()
        {
            _gameplayVirtualCamera.m_Transitions.m_OnCameraLive.AddListener(OnCameraLive);

            CinemachineManager.OnCamSwipe += OnMapScrolling;

            OnStart();
        }

        public void Dispose()
        {
            _gameplayVirtualCamera.m_Transitions.m_OnCameraLive.RemoveListener(OnCameraLive);

            CinemachineManager.OnCamSwipe -= OnMapScrolling;

            OnDispose();
        }

        private void OnCameraLive(ICinemachineCamera currentCam, ICinemachineCamera nextCam)
        {
            if (currentCam.IsValid)
            {
                if (_gameplayCamTransitionTw.IsActive()) return;

                _gameplayCamTransitionTw = DOVirtual.DelayedCall(_blendTime, () => _isMapScrollingActivated = true);
            }
            else
            {
                _isMapScrollingActivated = false;
            }
        }

        private void OnMapScrolling(float xOffset, bool useTargetFollow)
        {
            if (!_isMapScrollingActivated) return;

            if (useTargetFollow)
            {
                if (_followTarget)
                {
                    _followTarget.position = _followTarget.position.With(x: _defaultTargetXOffset + xOffset);
                }
            }
            else
            {
                _gameplayCameraTransposer.m_FollowOffset =
                    _gameplayCameraTransposer.m_FollowOffset.With(x: _defaultCamXOffset + xOffset);
            }
        }

        private void OnStart()
        {
            _defaultCamXOffset = _gameplayCameraTransposer.m_FollowOffset.x;

            if (_followTarget)
            {
                _defaultTargetXOffset = _followTarget.position.x;
            }
            else
            {
                Debug.LogError("Follow Target Is Empty On Gameplay Camera");
            }
        }

        private void OnDispose()
        {
            _gameplayCamTransitionTw.Kill();

            _gameplayCameraTransposer.m_FollowOffset =
                _gameplayCameraTransposer.m_FollowOffset.With(x : _defaultCamXOffset);

            if (_followTarget)
            {
                _followTarget.position = _followTarget.position.With(x : _defaultTargetXOffset);
            }
        }
    }
}