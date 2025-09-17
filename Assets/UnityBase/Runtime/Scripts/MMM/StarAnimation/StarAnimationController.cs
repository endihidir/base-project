using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace __Funflare.Scripts.Missions
{
    public class StarAnimationController : MonoBehaviour
    {
        public const float STAR_MOVEMENT_DELAY = 0.15f;

        [SerializeField] private GameObject _handler;
        
        //[SerializeField, ReadOnly] private StarProgressBarDisplayController _barDisplayController;

       // [SerializeField, ReadOnly] private StarUIPool _starUIPool;

        private List<StarAnimationSequence> _starAnimationSequences = new List<StarAnimationSequence>();

#if UNITY_EDITOR
        private void OnValidate()
        {
           // TryGetComponent(out _starUIPool);
           // TryGetComponent(out _barDisplayController);
        }
#endif
        private void Awake()
        {
            /*if (!MissionDatabase.IsNewProgressSystemActive(FunflareSceneUtil.SceneName()))
            {
                gameObject.SetActive(false);
                return;
            }

            _starUIPool ??= GetComponent<StarUIPool>();
            
            _starUIPool.CreatePool();
            
            _barDisplayController ??= GetComponent<StarProgressBarDisplayController>();

            FunflareGameSignals.OnSagaMapActive += OnSagaMapActive;*/
        }

        private void OnSagaMapActive(bool isActive)
        {
            _handler.SetActive(!isActive);
        }

        private void OnEnable()
        {
            /*FunflareMissionSignals.OnStarAnimationBegin += PlayStarAnimation;
            
            _barDisplayController.InitStarProgress();*/
        }

        private void PlayStarAnimation(int missionId, Vector2 initialPos, int starCount)
        {
            var sequence = GetAnimationSequence();
            
            for (int i = 0; i < starCount; i++)
            {
               // var starUI = _starUIPool.GetStarIcon(initialPos);

              //  starUI.Initialize(initialPos, _barDisplayController.IconTarget.position, Vector2.one, Vector2.one * 1.5f, true, i * STAR_MOVEMENT_DELAY);
                
               // sequence.Add(starUI);
            }
            
            // sequence.StartMovements(0.75f, 300f, CurveSide.Down, OnEachStarMovementComplete);

            // sequence.OnSequenceComplete += ()=> OnSequenceComplete(missionId);
        }

        /*private void OnEachStarMovementComplete(IStarUI starUI)
        {
            _barDisplayController.UpdateStarProgress();
            
            _starUIPool.ReturnToPool(starUI);
        }*/

        private void OnSequenceComplete(int missionId)
        {
           // FunflareMissionSignals.OnStarAnimationComplete?.Invoke(missionId);
        }

        private StarAnimationSequence GetAnimationSequence()
        {
            var sequence = _starAnimationSequences.FirstOrDefault(x => !x.IsInProgress);

            if (sequence is not null) return sequence;
            
            sequence = new StarAnimationSequence();
            
            _starAnimationSequences.Add(sequence);

            return sequence;
        }
        
        private void OnDisable()
        {
            /*if (!MissionDatabase.IsNewProgressSystemActive(FunflareSceneUtil.SceneName())) return;
            
            FunflareMissionSignals.OnStarAnimationBegin -= PlayStarAnimation;
            
            FunflareGameSignals.OnSagaMapActive -= OnSagaMapActive;*/
        }
    }
}