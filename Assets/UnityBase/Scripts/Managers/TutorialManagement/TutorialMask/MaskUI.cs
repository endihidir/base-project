using System;
using DG.Tweening;
using UnityBase.Extensions;
using UnityBase.Pool;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode]
[AddComponentMenu("UI/Unmask/Unmask", 1)]
public class MaskUI : MonoBehaviour, IMaterialModifier, IPoolable
{
    private static readonly Vector2 s_Center = new Vector2(0.5f, 0.5f);
    
    [SerializeField] private RectTransform m_FitTarget;
    
    [SerializeField] private bool m_FitOnLateUpdate;
    
    [SerializeField] private bool m_OnlyForChildren = false;
    
    [SerializeField] private bool m_ShowUnmaskGraphic = false;

    [Tooltip("Edge smoothing.")]
    [Range(0f, 1f)]
    [SerializeField] private float m_EdgeSmoothing = 0f;

    [SerializeField] private Image _maskFadeImage;

    private Color _fadePanelStartColor;

    public MaskableGraphic graphic { get { return _graphic ?? (_graphic = GetComponent<MaskableGraphic>()); } }
    public Image Image { get; private set; }
    
    public RectTransform fitTarget
    {
        get { return m_FitTarget; }
        set
        {
            m_FitTarget = value;
            FitTo(m_FitTarget);
        }
    }
    
    public bool fitOnLateUpdate { get { return m_FitOnLateUpdate; } set { m_FitOnLateUpdate = value; } }
    public Component PoolableObject => this;
    public bool IsActive => isActiveAndEnabled;
    public bool IsUnique => false;

    public bool showUnmaskGraphic
    {
        get { return m_ShowUnmaskGraphic; }
        set
        {
            m_ShowUnmaskGraphic = value;
            SetDirty();
        }
    }
    
    public bool onlyForChildren
    {
        get { return m_OnlyForChildren; }
        set
        {
            m_OnlyForChildren = value;
            SetDirty();
        }
    }

    public float edgeSmoothing
    {
        get { return m_EdgeSmoothing; }
        set { m_EdgeSmoothing = value; }
    }

    private Tween _maskFadeTween;
    
    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if (!isActiveAndEnabled)
        {
            return baseMaterial;
        }

        Transform stopAfter = MaskUtilities.FindRootSortOverrideCanvas(transform);
        var stencilDepth = MaskUtilities.GetStencilDepth(transform, stopAfter);
        var desiredStencilBit = 1 << stencilDepth;

        StencilMaterial.Remove(_unmaskMaterial);
        _unmaskMaterial = StencilMaterial.Add(baseMaterial, desiredStencilBit - 1, StencilOp.Invert, CompareFunction.Always, m_ShowUnmaskGraphic ? ColorWriteMask.All : (ColorWriteMask)0, desiredStencilBit - 1, (1 << 8) - 1);

        // Unmask affects only for children.
        var canvasRenderer = graphic.canvasRenderer;
        if (m_OnlyForChildren)
        {
            StencilMaterial.Remove(_revertUnmaskMaterial);
            _revertUnmaskMaterial = StencilMaterial.Add(baseMaterial, (1 << 7), StencilOp.Invert, CompareFunction.Equal, (ColorWriteMask)0, (1 << 7), (1 << 8) - 1);
            canvasRenderer.hasPopInstruction = true;
            canvasRenderer.popMaterialCount = 1;
            canvasRenderer.SetPopMaterial(_revertUnmaskMaterial, 0);
        }
        else
        {
            canvasRenderer.hasPopInstruction = false;
            canvasRenderer.popMaterialCount = 0;
        }

        return _unmaskMaterial;
    }
    
    public void FitTo(RectTransform target)
    {
        var rt = transform as RectTransform;

        rt.pivot = target.pivot;
        rt.position = target.position;
        rt.rotation = target.rotation;

        var s1 = target.lossyScale;
        var s2 = rt.parent.lossyScale;
        rt.localScale = new Vector3(s1.x / s2.x, s1.y / s2.y, s1.z / s2.z);
        rt.sizeDelta = target.rect.size;
        rt.anchorMax = rt.anchorMin = s_Center;
    }
    
    private Material _unmaskMaterial;
    private Material _revertUnmaskMaterial;
    private MaskableGraphic _graphic;
    
    private void OnEnable()
    {
        if (m_FitTarget)
        {
            FitTo(m_FitTarget);
        }
        SetDirty();
    }
    
    private void OnDisable()
    {
        StencilMaterial.Remove(_unmaskMaterial);
        StencilMaterial.Remove(_revertUnmaskMaterial);
        _unmaskMaterial = null;
        _revertUnmaskMaterial = null;

        if (graphic)
        {
            var canvasRenderer = graphic.canvasRenderer;
            canvasRenderer.hasPopInstruction = false;
            canvasRenderer.popMaterialCount = 0;
            graphic.SetMaterialDirty();
        }
        SetDirty();
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (m_FitTarget && (m_FitOnLateUpdate || !Application.isPlaying))
#else
		if (m_FitTarget && m_FitOnLateUpdate)
#endif
        {
            FitTo(m_FitTarget);
        }

        Smoothing(graphic, m_EdgeSmoothing);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetDirty();
    }
#endif

    private void Awake()
    {
        Image = GetComponent<Image>();
    }

    public void PrepareMask(Vector3 position, Vector2 scale, float pixelsPerUnitMultiplier, float sharpnessMultiplier = 1f)
    {
        transform.position = position;
        Image.rectTransform.sizeDelta = scale;
        Image.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier * sharpnessMultiplier;
        
        _maskFadeImage.color = _maskFadeImage.color.SetAlpha(0f);
        _maskFadeImage.rectTransform.sizeDelta = scale;
        _maskFadeImage.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier * sharpnessMultiplier;
    }

    public void SetMaskPanelStartColor(Color fadePanelStartColor)
    {
        _fadePanelStartColor = fadePanelStartColor;
    }
    
    public void Show(float duration, float delay, Action onComplete)
    {
        gameObject.SetActive(true);
        onComplete?.Invoke();
    }

    public void Hide(float duration, float delay, Action onComplete)
    {
        _maskFadeTween.Kill();
        
        _maskFadeTween = _maskFadeImage.DOColor(_fadePanelStartColor, duration).SetDelay(delay).OnComplete(()=>
        {
            onComplete?.Invoke();
            Reset();
        });
    }

    private void Reset()
    {
        gameObject.SetActive(false);
        _maskFadeImage.color = _maskFadeImage.color.SetAlpha(0f);
    }

    void SetDirty()
    {
        if (graphic)
        {
            graphic.SetMaterialDirty();
        }
    }

    private static void Smoothing(MaskableGraphic graphic, float smooth)
    {
        if (!graphic) return;

        Profiler.BeginSample("[Unmask] Smoothing");
        var canvasRenderer = graphic.canvasRenderer;
        var currentColor = canvasRenderer.GetColor();
        var targetAlpha = 1f;
        if (graphic.maskable && 0 < smooth)
        {
            var currentAlpha = graphic.color.a * canvasRenderer.GetInheritedAlpha();
            if (0 < currentAlpha)
            {
                targetAlpha = Mathf.Lerp(0.01f, 0.002f, smooth) / currentAlpha;
            }
        }

        if (!Mathf.Approximately(currentColor.a, targetAlpha))
        {
            currentColor.a = Mathf.Clamp01(targetAlpha);
            canvasRenderer.SetColor(currentColor);
        }

        Profiler.EndSample();
    }

    private void OnDestroy()
    {
        _maskFadeTween.Kill();
    }
}