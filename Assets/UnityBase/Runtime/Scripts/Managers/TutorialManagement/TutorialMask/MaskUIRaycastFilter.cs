using UnityEngine;

[AddComponentMenu("UI/Unmask/UnmaskRaycastFilter", 2)]
public class MaskUIRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [SerializeField] private MaskUI mTargetMaskUI;
    
    public MaskUI TargetMaskUI { get { return mTargetMaskUI; } set { mTargetMaskUI = value; } }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (!isActiveAndEnabled || !mTargetMaskUI || !mTargetMaskUI.isActiveAndEnabled)
        {
            return true;
        }
        
        if (eventCamera)
        {
            return !RectTransformUtility.RectangleContainsScreenPoint((mTargetMaskUI.transform as RectTransform), sp, eventCamera);
        }
        else
        {
            return !RectTransformUtility.RectangleContainsScreenPoint((mTargetMaskUI.transform as RectTransform), sp);
        }
    }
}