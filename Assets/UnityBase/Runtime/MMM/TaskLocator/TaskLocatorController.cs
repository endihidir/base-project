using System.Collections.Generic;
using UnityEngine;

namespace __Funflare.Scripts.TaskLocator
{
    public class TaskLocatorController : MonoBehaviour
    {
        [SerializeField] private RectTransform _locatorBoundsRect;
        
        [SerializeField] private RectTransform _locatorShowBoundsRect;
     
        [SerializeField] private ArrowHandler _arrowHandler;

        private Camera _cam;

        private List<Component> _pooledComponents = new List<Component>();
        
        private void Awake()
        {
            _cam = Camera.main;
            
            _arrowHandler.gameObject.SetActive(false);
        }

        void Update()
        {
            if (TryGetClosestObject<Component>(out var gameObjectComponent)) // Component need to update
            {
                var targetScreenPos = _cam.WorldToScreenPoint(gameObjectComponent.transform.position);
                
                targetScreenPos.z = 0f;

                var localPoint = _locatorShowBoundsRect.InverseTransformPoint(targetScreenPos);
                
                if (_locatorShowBoundsRect.rect.Contains(localPoint))
                {
                    _arrowHandler.gameObject.SetActive(false);
                }
                else
                {
                    //_myObjectManager.SetObjectIcon(shelfController.shelfObjectType, _arrowHandler.Image);
                    
                    _arrowHandler.gameObject.SetActive(_cam.isActiveAndEnabled); // NEED TO CHECK CAMERA
                    
                    var intersectPoint = GetClosestPointOnRect(_locatorBoundsRect, new Vector2(targetScreenPos.x, targetScreenPos.y));
                    
                    _arrowHandler.transform.position = Vector3.Lerp(_arrowHandler.transform.position, intersectPoint, 8f * Time.deltaTime);

                    var lookRotation = GetLookRotation(_arrowHandler.transform.position, targetScreenPos);
                    
                    _arrowHandler.RotationHandler.rotation = Quaternion.Lerp(_arrowHandler.RotationHandler.rotation, lookRotation, 8f * Time.deltaTime);
                }
            }
            else
            {
                _arrowHandler.gameObject.SetActive(false);
            }
        }

        
        private bool TryGetClosestObject<T>(out T gameObjectComponent) where T : Component
        {
            gameObjectComponent = default;
            
            if (_pooledComponents.Count == 1)
            {
                return TryGetSingleShelfPosition(out gameObjectComponent);
            }

            return TryGetClosest(out gameObjectComponent);
        }

        private bool TryGetSingleShelfPosition<T>(out T component) where T : Component
        {
            component = _pooledComponents[0] as T;
            
            /*var unlockedComponent = _pooledComponents[0];

            if (unlockedComponent)
            {
                component = unlockedComponent;
                return true;
            }*/

            return true;
        }

        private bool TryGetClosest<T>(out T component) where T : Component
        {
            component = default;
            
            var closestDist = float.MaxValue;
            
            var playerPos = Vector3.zero; // Need To Edit

            foreach (var gameObjectComponent in _pooledComponents)
            {
                var distance = Vector3.Distance(gameObjectComponent.transform.position, playerPos);

                if (distance < closestDist)
                {
                    closestDist = distance;
                    component = gameObjectComponent as T;
                }
            }

            return closestDist < float.MaxValue;
        }
        
        private Vector2 GetClosestPointOnRect(RectTransform rectTransform, Vector2 point)
        {
            var worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            
            var bottomLeft = worldCorners[0];
            var topRight = worldCorners[2];
            
            var closestX = Mathf.Clamp(point.x, bottomLeft.x, topRight.x);
            var closestY = Mathf.Clamp(point.y, bottomLeft.y, topRight.y);

            return new Vector2(closestX, closestY);
        }
        
        private Quaternion GetLookRotation(Vector3 mainPos, Vector3 targetPosition)
        {
            var dir = targetPosition - mainPos;
            dir.z = 0f;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, angle);
        }
    }
}
