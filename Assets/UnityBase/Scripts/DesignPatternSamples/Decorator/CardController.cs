using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UnityBase.DesignPatterns.Decorator
{
    public class CardController : MonoBehaviour
    {
        [SerializeField, Required] private CardDefinition _cardDefinition;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Ease _ease = Ease.OutBack;

        private Tween _tween;
        public ICard Card { get; set; }

        private void Awake()
        {
            Card = CardFactory.Crate(_cardDefinition);
        }

        private void OnMouseDown()
        {
            if (CardManager.Instance.selectedCard == null)
            {
                CardManager.Instance.selectedCard = this;
                transform.localScale = new Vector3(2f, 3f, 0.1f) * 1.1f;
            }
            else
            {
                CardManager.Instance.Decorate(this);
                CardManager.Instance.selectedCard = null;
                transform.localScale = new Vector3(2f, 3f, 0.1f);
            }
        }

        public void MoveTo(Vector3 position)
        {
            _tween.Kill();
            _tween = transform.DOMove(position, _duration).SetEase(_ease);
        }
        
        public void MoveToAndDestroy(Vector3 position)
        {
            transform.localScale = new Vector3(2f, 3f, 0.1f);
            _tween.Kill();
            _tween = transform.DOMove(position, _duration).SetEase(_ease).OnComplete(()=> Destroy(gameObject));
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }
    }
}