using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StoreDecoration
{
    [ExecuteInEditMode]
    public class PathFollower : MonoBehaviour
    {
        [SerializeField] private CustomerController _customerController;
        [SerializeField] private XPosRange _characterLocalXPosRange;
        
        [ReadOnly] [SerializeField] private PathPoint _closestPath;
        [ReadOnly] [SerializeField] private int _indexCounter;
        [ReadOnly] [SerializeField] private bool _isCrowdedCustomerActive;
        [ReadOnly] [SerializeField] private PathPoint[] _path;
        [ReadOnly] [SerializeField] private MiddlePath[] _middlePaths;
        [ReadOnly] [SerializeField] private bool _isMovedClosestTarget, _isGenerated;
        [ReadOnly] [SerializeField] private float _startDelay, _stopDelay, _cornerDelay = 0.25f;

        private Sequence _moveSeq;
        public float StartDelay
        {
            get => _startDelay;
            set => _startDelay = value;
        }

        public float StopDelay
        {
            get => _stopDelay;
            set => _stopDelay = value;
        }

        public bool IsCrowdedCustomerActive
        {
            get => _isCrowdedCustomerActive;
            set => _isCrowdedCustomerActive = value;
        }

        public XPosRange CharacterLocalXPosRange
        {
            get => _characterLocalXPosRange;
            set => _characterLocalXPosRange = value;
        }

        public PathPoint[] Path
        {
            get => _path;
            set => _path = value;
        }

        public MiddlePath[] MiddlePaths
        {
            get => _middlePaths;
            set => _middlePaths = value;
        }

        public bool IsGenerated
        {
            get => _isGenerated;
            set => _isGenerated = value;
        }

        private void OnEnable()
        {
            if (!_isGenerated)
            {
                _isGenerated = true;

                _customerController.PrepareCustomer();
                
                var randomXVal = Random.Range(CharacterLocalXPosRange.min, CharacterLocalXPosRange.max);
                
                while (randomXVal > (CharacterLocalXPosRange.min + 3f) && randomXVal < (CharacterLocalXPosRange.max - 3f))
                {
                    randomXVal = Random.Range(CharacterLocalXPosRange.min, CharacterLocalXPosRange.max);
                }
                
                _customerController.SetLocalXPos(randomXVal);

                int randomPath = Random.Range(0, MiddlePaths.Length);
                var path = MiddlePaths[randomPath];

                int randomPathIndex_1 = Random.Range(0, path.PathPoints.Length - 1);
                int nextIndex = randomPathIndex_1 + 1;
            
                var dist = path.PathPoints[nextIndex].NextPathPoint.position - path.PathPoints[randomPathIndex_1].PreviousPathPoint.position;
                
                if (IsCrowdedCustomerActive)
                {
                    var targetPos = path.PathPoints[randomPathIndex_1].PreviousPathPoint.position + Random.value * dist;
                    transform.position = targetPos;
                    _closestPath = GetClosestPathPoint(Path);
                    if(_closestPath) _indexCounter = _closestPath.Index;
                }
                else
                {
                    transform.position = _path[0].transform.position;
                    _closestPath = _path[1];
                    _indexCounter = _closestPath.Index;
                }

                dist.y = 0f;
                transform.rotation = Quaternion.LookRotation(dist);
                
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;
            
            StartDelay -= Time.deltaTime;

            if (!(StartDelay < 0f)) return;
            
            StopDelay -= Time.deltaTime;

            if (!_closestPath)
            {
                if (IsCrowdedCustomerActive)
                {
                    _closestPath = GetClosestPathPoint(Path);
                    if(_closestPath) _indexCounter = _closestPath.Index;
                }
                else
                {
                    _closestPath = _path[1];
                    _indexCounter = _closestPath.Index;
                }
            }

            if (Path != null)
            {
                var dist = Path[_indexCounter].transform.position - transform.position;

                if (dist.sqrMagnitude < 0.01f)
                {
                    if (!_isMovedClosestTarget)
                    {
                        _isMovedClosestTarget = true;

                        _indexCounter++;

                        if (_indexCounter > _path.Length - 1)
                        {
                            _indexCounter = 1;
                            transform.position = Path[0].transform.position;
                            _customerController.ChangeDirection(Path[0].transform.rotation);
                        }

                        if (Path[_indexCounter - 1].IsCorner)
                        {
                            StopDelay = _cornerDelay;
                            _isMovedClosestTarget = false;
                        }
                    }
                }
                else
                {
                    _isMovedClosestTarget = false;
                }

                if (Path[_indexCounter - 1].IsCorner)
                    _customerController.ChangeDirection(Path[_indexCounter].transform.rotation, 2f / _cornerDelay);

                       
                if (StopDelay <= 0f)
                {
                    _customerController.EnableSelectedCustomer(true);
                    transform.position = Vector3.MoveTowards(transform.position, Path[_indexCounter].transform.position, 8f * Time.deltaTime);
                    StopDelay = _customerController.Move();
                }
            }
        }

        private PathPoint GetClosestPathPoint(PathPoint[] pathPoints)
        {
            PathPoint closestTarget = null;
        
            float closestDistanceSqr = Mathf.Infinity;

            List<PathPoint> frontPathPoints = new List<PathPoint>();

            foreach (PathPoint potentialTarget in pathPoints)
            {
                float distToTarget = GetPointSide(transform, potentialTarget);

                if (distToTarget > 0)
                {
                    frontPathPoints.Add(potentialTarget);
                }
            }

            foreach (PathPoint potentialTarget in frontPathPoints)
            {
                Vector3 directionToTarget = transform.position - potentialTarget.transform.position;
            
                float dSqrToTarget = directionToTarget.sqrMagnitude;
            
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                
                    closestTarget = potentialTarget;
                }
            }
            
            return closestTarget;
        }
        
        private float GetPointSide(Transform startT, PathPoint closestPathPoint)
        {
            if (closestPathPoint)
            {
                Vector3 forward = startT.forward;
        
                Vector3 direction = closestPathPoint.transform.position - startT.position;
        
                direction.y = 0;
        
                direction.Normalize();
        
                float side = Vector3.Dot(forward, direction);
        
                return side;
            }
            else
            {
                return -1f;
            }
        }
    }

    [Serializable]
    public struct XPosRange
    {
        public float min;
        public float max;
    }
}