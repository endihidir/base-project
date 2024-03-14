using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StoreDecoration
{
    [ExecuteInEditMode]
    public class MiddlePath : MonoBehaviour
    {
        [ReadOnly] [SerializeField] private PathPoint[] _pathPoints;
        [ReadOnly] [SerializeField] private PathManager _pathManager;

        public PathManager PathManager
        {
            get => _pathManager;
            set => _pathManager = value;
        }

        public PathPoint[] PathPoints => _pathPoints;

        private void Update()
        {
            if (!Application.isPlaying)
                _pathPoints = GetComponentsInChildren<PathPoint>();
            
            var midPathPointsLength = _pathPoints.Length;
            
            for (int i = 0; i < midPathPointsLength; i++)
            {
                var dist = _pathPoints[i].NextPathPoint.position - _pathPoints[i].PreviousPathPoint.position;
                var dir = dist.normalized;
                var endDist = dir * (dist.magnitude / (midPathPointsLength + 1));
                _pathPoints[i].transform.position = Vector3.Lerp(_pathPoints[i].PreviousPathPoint.position, _pathPoints[i].NextPathPoint.position - endDist, ((i + 1) * 1f) / midPathPointsLength);
                _pathPoints[i].transform.rotation = _pathPoints[i].PreviousPathPoint.rotation;
            }
        }

        public void OnDestroy()
        {
            if (PathManager)
            {
                PathManager.RemoveMiddlePath(this);
            
                for (int i = 0; i < _pathPoints.Length; i++)
                {
                    PathManager.RemoveMiddlePathPoint(_pathPoints[i]);
                }
            }
        }
    }
}