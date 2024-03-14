using Sirenix.OdinInspector;
using UnityEngine;

namespace StoreDecoration
{
    [ExecuteInEditMode]
    public class PathPoint : MonoBehaviour
    {
        [SerializeField] private bool _manuelGeneratedPoint;
        [ReadOnly] [SerializeField] private int _index;
        [ReadOnly] [SerializeField] private bool _isFistPoint;
        [ReadOnly] [SerializeField] private bool _isLastPoint;
        [ReadOnly] [SerializeField] private bool _isWaitPoint;
        [ReadOnly] [SerializeField] private bool _isCorner;
        [ReadOnly] [SerializeField] private PathManager _pathManager;
        
        [ShowIf("Show")]
        [ReadOnly] [SerializeField] private Transform _previousPathPoint, _nextPathPoint;

        private bool Show => !ManuelGeneratedPoint;

        #region PROPERTIES

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public bool IsFistPoint
        {
            get => _isFistPoint;
            set => _isFistPoint = value;
        }

        public bool IsLastPoint
        {
            get => _isLastPoint;
            set => _isLastPoint = value;
        }

        public bool IsWaitPoint
        {
            get => _isWaitPoint;
            set => _isWaitPoint = value;
        }

        public bool IsCorner
        {
            get => _isCorner;
            set => _isCorner = value;
        }

        public PathManager PathManager
        {
            get => _pathManager;
            set => _pathManager = value;
        }

        public bool ManuelGeneratedPoint => _manuelGeneratedPoint;

        public Transform PreviousPathPoint
        {
            get => _previousPathPoint;
            set => _previousPathPoint = value;
        }

        public Transform NextPathPoint
        {
            get => _nextPathPoint;
            set => _nextPathPoint = value;
        }

        #endregion
    }
}