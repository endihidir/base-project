using System;
using DG.Tweening;
using UnityBase.PopUpCore;
using UnityBase.UI.ButtonCore;
using UnityBase.UI.ToggleCore;
using UnityEngine;

public class MainMenuSettingsPopUp : PopUp
{
    [Header("TOGGLES")]
    [SerializeField] private ToggleBehaviour[] _toggleBehaviours;

    [Header("BUTTONS")] 
    [SerializeField] private ButtonBehaviour[] _settingsButtons;

    protected Tween _popUpTween;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        _settingsButtons = GetComponentsInChildren<ButtonBehaviour>(true);
        _toggleBehaviours = GetComponentsInChildren<ToggleBehaviour>(true);
    }
#endif
    
    public override void Show(float duration, float delay, Action onComplete)
    {
        UpdateBehaviours();
        
        _popUpTween.Kill();
        
        gameObject.SetActive(true);
        
        SetGroupSettings(0f, true, true);
            
        _popUpHandler.localScale = Vector3.zero;

        _popUpTween = DOTween.Sequence()
                            .Append(DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1, duration).SetEase(Ease.Linear))
                            .Join(_popUpHandler.DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear))
                            .SetDelay(delay)
                            .SetUpdate(true)
                            .OnComplete(()=> onComplete?.Invoke());
    }

    public override void Hide(float duration, float delay, Action onComplete)
    {
        _popUpTween.Kill();
        
        SetGroupSettings(1f, false, false);

        _popUpTween = DOTween.Sequence()
                            .Append(DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 0, duration).SetEase(Ease.Linear))
                            .AppendCallback(()=> gameObject.SetActive(false))
                            .SetDelay(delay)
                            .SetUpdate(true)
                            .OnComplete(()=> onComplete?.Invoke());
    }
    
    private void UpdateBehaviours()
    {
        var buttonsLenght = _settingsButtons.Length;

        for (int i = 0; i < buttonsLenght; i++)
        {
            _settingsButtons[i].UpdateButton();
        }

        var togglesLenght = _toggleBehaviours.Length;

        for (int i = 0; i < togglesLenght; i++)
        {
            _toggleBehaviours[i].UpdateToggle();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        _popUpTween?.Kill();
    }
}