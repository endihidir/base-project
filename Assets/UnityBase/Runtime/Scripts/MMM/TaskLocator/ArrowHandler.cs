using UnityEngine;
using UnityEngine.UI;

namespace __Funflare.Scripts.TaskLocator
{
    public class ArrowHandler : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        
        [SerializeField] private Transform rotationHandler;

        public Image Image => _iconImage;
        public Transform RotationHandler => rotationHandler;
    }
}