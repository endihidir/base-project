using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StoreDecoration
{
    [ExecuteInEditMode]
    public class FollowerManager : MonoBehaviour
    {
        [SerializeField] private PathManager _pathManager;
        [SerializeField] private PathFollower _pathFollowerPrefab;
        [SerializeField] private Transform _followerPoolT, _activeFollowersT;
        [SerializeField] private int _frequency;
        [SerializeField] private int _poolCount;
        
        [SerializeField] private float _movementBounds;
        [SerializeField] private bool _useCrowdedCutomer;

        [ShowIf("ShowSpawnDelay")] 
        [SerializeField] private float _actvationDelay;

        [ReadOnly] [SerializeField] private List<PathFollower> _activeFollowers = new List<PathFollower>();
        [ReadOnly] [SerializeField] private List<PathFollower> _pooledFollowers = new List<PathFollower>();

        public bool ShowSpawnDelay => !_useCrowdedCutomer;
        private void OnValidate()
        {
            _pathManager = GetComponent<PathManager>();
        }

        private void Update()
        {
            if (_movementBounds < 3f) _movementBounds = 3f;
            
            if (_frequency < 0f) _frequency = 0;
                
            if (_poolCount < 0) _poolCount = 0;
                
            if (_frequency > _poolCount) _frequency = _poolCount;
            
            var instantiateStartCount = _useCrowdedCutomer ? _pooledFollowers.Count + _frequency : _pooledFollowers.Count;
            var instantiateEndCount = _useCrowdedCutomer ? _poolCount + _frequency : _poolCount;
            
            for (int i = instantiateStartCount; i < instantiateEndCount; i++)
            {
                var follower = Instantiate(_pathFollowerPrefab, _followerPoolT);
                
                follower.name = "Follower_" + (_pooledFollowers.Count + 1);

                follower.IsGenerated = false;
                
                follower.CharacterLocalXPosRange = new XPosRange()
                {
                    min = -_movementBounds,
                    max = _movementBounds
                };

                _pooledFollowers.Add(follower);
            }

            var removeStartCount = _useCrowdedCutomer ? _pooledFollowers.Count - _frequency : _pooledFollowers.Count;
            var removeEndCount = _useCrowdedCutomer ? _poolCount - _frequency : _poolCount;
            
            for (int i = removeStartCount; i > removeEndCount; i--)
            {
                if(!Application.isPlaying)
                    DestroyImmediate(_pooledFollowers[^1].gameObject);
                else
                    Destroy(_pooledFollowers[^1].gameObject);
                
                _pooledFollowers.RemoveAt(_pooledFollowers.Count - 1);
            }
            
            for (int i = 0; i < _pooledFollowers.Count; i++)
            {
                _pooledFollowers[i].Path = _pathManager.PathPoints;
                _pooledFollowers[i].MiddlePaths = _pathManager.GeneratedMiddlePaths;
            }
            
            for (int i = _activeFollowers.Count; i < _frequency; i++)
            {
                if (!_useCrowdedCutomer)
                {
                    _pooledFollowers[i].StartDelay = i * _actvationDelay;
                }
                else
                {
                    _pooledFollowers[i].IsGenerated = false;
                    _pooledFollowers[i].StartDelay = 0f;
                }
                
                _pooledFollowers[i].transform.SetParent(_activeFollowersT);

                _activeFollowers.Add(_pooledFollowers[i]);
            }

            for (int i = _activeFollowers.Count; i > _frequency; i--)
            {
                if (_activeFollowers[^1])
                {
                    _activeFollowers[^1].IsGenerated = false;
                    _activeFollowers[^1].StartDelay = 0f;
                    _activeFollowers[^1].transform.SetParent(_followerPoolT);
                    _activeFollowers[^1].transform.SetSiblingIndex(0);
                    _activeFollowers[^1].transform.localPosition = Vector3.zero;
                }
                    
                _activeFollowers.RemoveAt(_activeFollowers.Count - 1);
            }

            for (int i = 0; i < _pooledFollowers.Count; i++)
            {
                _pooledFollowers[i].IsCrowdedCustomerActive = _useCrowdedCutomer;
            }
        }
        
        public void UseCrowdedCustomers(bool useCrowdedCustomers, float delay = 1f)
        {
            _actvationDelay = delay;
            _useCrowdedCutomer = useCrowdedCustomers;
        }

        public void SetFrequency(int count, bool resetFrequency = true)
        {
            if(resetFrequency) _frequency = 0;
            if (count <= _poolCount) _frequency = count;
            
        }
    }
}