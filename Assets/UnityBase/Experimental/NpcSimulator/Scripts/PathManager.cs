using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityBase.Runtime.Utility;
using UnityEngine;

namespace StoreDecoration
{
    [ExecuteInEditMode]
    public class PathManager : MonoBehaviour
    {
        [ReadOnly] [SerializeField] private PathPoint[] _pathPoints;
        [ReadOnly] [SerializeField] private PathPoint[] _preparedPathPoints;
        [ReadOnly] [SerializeField] private List<MiddlePath> _generatedMiddlePaths = new List<MiddlePath>();
        [ReadOnly] [SerializeField] private List<PathPoint> _generatedPathPoints = new List<PathPoint>();
        [SerializeField, Range(0,10)] private int[] _middlePointsCount;
        public PathPoint[] PathPoints => _pathPoints;
        public MiddlePath[] GeneratedMiddlePaths => _generatedMiddlePaths.ToArray();

        private void Update()
        {
            if (!Application.isPlaying)
            {
                UpdatePathPointData();
            }
        }

        private void UpdatePathPointData()
        {
            _pathPoints = GetComponentsInChildren<PathPoint>();

            _preparedPathPoints = GetComponentsInChildren<PathPoint>().ToList().FindAll(x=> x.ManuelGeneratedPoint).ToArray();
            
            var preparedPathPointsLenght = _preparedPathPoints.Length;

            Array.Resize(ref _middlePointsCount, preparedPathPointsLenght - 1);

            for (int i = 1; i < preparedPathPointsLenght; i++)
            {
                var lookDir = (_preparedPathPoints[i].transform.position - _preparedPathPoints[i - 1].transform.position).normalized;
                lookDir.y = 0f;
                _preparedPathPoints[i - 1].transform.rotation = Quaternion.LookRotation(lookDir);
            }

            for (int i = 0; i < preparedPathPointsLenght; i++)
            {
                _preparedPathPoints[i].IsCorner = i > 0 && i < (preparedPathPointsLenght - 1);
            }

            var lastPointLookDir = _preparedPathPoints[preparedPathPointsLenght - 1].transform.position -
                                   _preparedPathPoints[preparedPathPointsLenght - 2].transform.position;
            lastPointLookDir.y = 0f;
            _preparedPathPoints[preparedPathPointsLenght - 1].transform.rotation = Quaternion.LookRotation(lastPointLookDir);

            var pathPointsLength = _pathPoints.Length;

            for (int i = 0; i < pathPointsLength; i++)
            {
                _pathPoints[i].Index = i;
                _pathPoints[i].IsFistPoint = i == 0;
                _pathPoints[i].IsLastPoint = i == (pathPointsLength - 1);
                _pathPoints[i].IsWaitPoint = i > 0 && i < (pathPointsLength - 1);
            }
        }

        [Button]
        public void UpdatePointsData()
        {
            ClearPathPoints();

            PreparePathPoints();
        }

        private void ClearPathPoints()
        {
            var middlePathLenght = _generatedMiddlePaths.Count;

            for (int i = 0; i < middlePathLenght; i++)
            {
                DestroyImmediate(_generatedMiddlePaths[0].gameObject);
            }

            _generatedMiddlePaths.Clear();
            _generatedPathPoints.Clear();
        }

        private void PreparePathPoints()
        {
            int counter = 0;

            for (int i = 0; i < _preparedPathPoints.Length - 1; i++)
            {
                var pointLenght = _middlePointsCount[i];
                var middlePoints = new GameObject("MiddlePoints_" + (i + 1));
                var mp = middlePoints.AddComponent<MiddlePath>();
                mp.PathManager = this;
                _generatedMiddlePaths.Add(mp);
                middlePoints.transform.SetParent(transform);
                counter++;
                middlePoints.transform.SetSiblingIndex(i + counter);

                for (int j = 0; j < pointLenght; j++)
                {
                    var go = new GameObject("MiddlePoint_" + (j + 1));
                    go.transform.SetParent(middlePoints.transform);
                    var ds = go.AddComponent<DebugSphere>();
                    ds.onlyWhenSelected = true;
                    var pp = go.AddComponent<PathPoint>();
                    pp.PreviousPathPoint = _preparedPathPoints[i].transform;
                    pp.NextPathPoint = _preparedPathPoints[i + 1].transform;
                    pp.PathManager = this;
                    _generatedPathPoints.Add(pp);
                }
            }
        }

        public void RemoveMiddlePath(MiddlePath middlePath)
        {
            _generatedMiddlePaths.Remove(middlePath);
        }
        public void RemoveMiddlePathPoint(PathPoint pathPoint)
        {
            _generatedPathPoints.Remove(pathPoint);
        }
    }
}