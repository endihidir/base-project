using UnityEngine;

namespace UnityBase.EventBus
{
    public class EventBusMonoTest : MonoBehaviour
    {
        [SerializeField] private int _health = 100, _mana = 100;

        private EventBinding<TestEvent> _testEventBindingHigh, _testEventBindingNormal;
        private EventBinding<PlayerEvent> _playerEventBinding;

        private void OnEnable()
        {
            _testEventBindingNormal = new EventBinding<TestEvent>(HandleTestEvent2, Priority.Critical);
            EventBus<TestEvent>.AddListener(_testEventBindingNormal);

            _testEventBindingHigh = new EventBinding<TestEvent>(HandleTestEvent, Priority.High);
            EventBus<TestEvent>.AddListener(_testEventBindingHigh);

            _playerEventBinding = new EventBinding<PlayerEvent>(HandlePlayerEvent);
            EventBus<PlayerEvent>.AddListener(_playerEventBinding, 0);
        }

        private void OnDisable()
        {
            EventBus<TestEvent>.RemoveListener(_testEventBindingHigh);
            EventBus<PlayerEvent>.RemoveListener(_playerEventBinding);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                EventBus<TestEvent>.Invoke(new TestEvent());
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                EventBus<PlayerEvent>.Invoke(new PlayerEvent { health = _health, mana = _mana });
            }
        }

        void HandleTestEvent()
        {
            Debug.Log("Test Event High Received");
        }

        void HandleTestEvent2()
        {
            Debug.Log("Test Event Normal Received");
        }

        void HandlePlayerEvent(PlayerEvent playerEvent)
        {
            Debug.Log($"Player event received! Healt: {playerEvent.health}, Mana: {playerEvent.mana}");
        }
    }
}