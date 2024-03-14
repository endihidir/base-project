using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityBase.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StoreDecoration
{
    [ExecuteInEditMode]
    public class CustomerController : MonoBehaviour
    {
        [ReadOnly] [SerializeField] private Animator _animator;
        [SerializeField] private GameObject[] _customers;
        [SerializeField] private AnimationCurve[] _localMovementCurves;
        [ReadOnly] [SerializeField] private float _xPos;
        [ReadOnly] [SerializeField] private int _animId, _selectedCustomerId;
        [ReadOnly] [SerializeField] private float _randomWaitTime;

        
        private AnimationCurve _movementCurve;
        private float _tVal;
        private bool _switch;

        private static readonly int Walk = Animator.StringToHash("Walk");
        private static readonly int Shop = Animator.StringToHash("Shop");
        private static readonly int Idle = Animator.StringToHash("Idle");

        private float _dirTimer;
        private Vector3 _startPos, _currentPos;

        public bool IsActive => isActiveAndEnabled;
        private void OnValidate()
        {
            _animator = GetComponent<Animator>();
        }

        public void PrepareCustomer()
        {
            _customers.ForEach(x => x.SetActive(false));
            _selectedCustomerId = Random.Range(0, _customers.Length);
            
            var range = Random.Range(0, 2);
            _animId = range == 0 ? Walk : Shop;
            _animator.SetBool(_animId, true);
            
            _randomWaitTime = Random.Range(2f, 4f);
        }

        public float Move()
        {
            _animator.SetBool(_animId, true);
                
            _tVal += Time.deltaTime / _randomWaitTime;

            if (_tVal > 1f)
            {
                _tVal = 0f;
                _switch = !_switch;
                _animator.SetBool(_animId, false);
                _randomWaitTime = Random.Range(2f, 4f);
                return _randomWaitTime;
            }

            _movementCurve = _switch ? _localMovementCurves[0] : _localMovementCurves[1];

            transform.localPosition = transform.localPosition.With(x:_xPos * _movementCurve.Evaluate(_tVal), z: _xPos * _movementCurve.Evaluate(_tVal));

            _dirTimer -= Time.deltaTime;

            if (_dirTimer < 0f)
            {
                _dirTimer = 0.1f;
                _startPos = transform.position;
            }

            _currentPos = transform.position;

            var dist = _currentPos - _startPos;
            dist.y = 0f;
            if(dist.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dist);

            return -1f;
        }
        
        public void ChangeDirection(Quaternion rot, float time)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, time * Time.deltaTime);
        }

        public void ChangeDirection(Quaternion rot)
        {
            transform.rotation = rot;
        }

        public void SetLocalXPos(float xPos, bool crossWalkActivated = true)
        {
            _xPos = xPos;
            
            transform.localPosition = transform.localPosition.With(x: xPos);

            if (!crossWalkActivated)
            {
                var pos = transform.localPosition.x > 0 ? -xPos : xPos;

                transform.localPosition = transform.localPosition.With(z: pos);
            }
        }

        public void EnableSelectedCustomer(bool enable)
        {
            _customers[_selectedCustomerId].SetActive(enable);
        }
    }
}