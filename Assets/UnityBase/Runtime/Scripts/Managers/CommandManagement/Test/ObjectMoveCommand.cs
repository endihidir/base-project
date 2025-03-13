using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityBase.Extensions;
using UnityEngine;

namespace UnityBase.Command
{
    public class ObjectMoveCommand : MoveCommand
    {
        private Vector3 _newPosition;
        private Vector3 _oldPosition;
        private bool _isInProgress;
        public override bool IsInProgress => _isInProgress;
        public override bool CanPassNextCommandInstantly => _moveEntity.CanPassNextMovementInstantly;

        private CancellationTokenSource _cancellationTokenSource;
        public ObjectMoveCommand(IMoveEntity moveEntity) : base(moveEntity) { }

        public override void Record() => _newPosition = _moveEntity.TargetPosition;

        public override async void Execute(Action onComplete)
        {
            _oldPosition = _moveEntity.Transform.position;
            
            _newPosition = _moveEntity.TargetPosition;

            await MoveObjectAsync(_newPosition, onComplete);
        }
        
        public override async void Undo(bool directly, Action onComplete)
        {
            if (directly)
            {
                _moveEntity.Transform.position = _oldPosition;
                return;    
            }
            
            await MoveObjectAsync(_oldPosition, onComplete);
        }

        public override async void Redo(bool directly, Action onComplete)
        {
            if (directly)
            {
                _moveEntity.Transform.transform.position = _newPosition;
                return;
            }
            
            await MoveObjectAsync(_newPosition, onComplete);
        }

        public override void Cancel() => _cancellationTokenSource?.Cancel();
        public override void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _moveEntity.MeshHandlerTransform.DOKill();
        }

        private async UniTask MoveObjectAsync(Vector3 targetPosition, Action onComplete)
        {
            _isInProgress = true;
            
            CancellationTokenExtentions.Refresh(ref _cancellationTokenSource);

            try
            {
                var dir = (targetPosition - _moveEntity.Transform.position).normalized;
                
                BallBounceAnim(dir);
                
                var transform = _moveEntity.Transform;

                while (transform.position.Distance(targetPosition) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveEntity.Speed * Time.deltaTime);
                    
                    await UniTask.WaitForSeconds(0f,false, PlayerLoopTiming.Update, _cancellationTokenSource.Token);
                }

                transform.position = targetPosition;
                
                onComplete?.Invoke();

                _isInProgress = false;
            }
            catch (Exception e)
            {
                //Debug.Log(e);
                
            }
        }

        private void BallBounceAnim(Vector3 dir)
        {
            _moveEntity.MeshHandlerTransform.DOComplete();
            _moveEntity.MeshHandlerTransform.DOPunchScale(dir, 0.35f, 25, 5f)
                                            .OnComplete(()=> _moveEntity.MeshHandlerTransform.localScale = Vector3.one);
        }
    }
}