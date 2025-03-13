using UnityBase.Extensions;
using UnityEngine;

public struct MaskUIData
{
    public PositionSpace positionSpace;
    public Vector2 scale;
    public float cornerSharpnessMultiplier;
    public float fadePanelOpacity;

    public MaskUIData(PositionSpace positionSpace, Vector2 scale, float cornerSharpnessMultiplier = 1f, float fadePanelOpacity = 0.9f)
    {
        this.positionSpace = positionSpace;
        this.scale = scale;
        this.cornerSharpnessMultiplier = cornerSharpnessMultiplier;
        this.fadePanelOpacity = fadePanelOpacity;
    }
}