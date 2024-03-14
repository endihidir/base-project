using UnityEngine;

namespace UnityBase.DesignPatterns.Decorator
{
    public class DropZone : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (CardManager.Instance.selectedCard)
            {
                CardManager.Instance.selectedCard.MoveToAndDestroy(transform.position);

                var total = CardManager.Instance.selectedCard.Card.Play();
                
                Debug.Log("Total: " + total);
            }
        }
    }
}