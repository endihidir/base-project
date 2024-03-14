using UnityBase.Extensions;
using UnityBase.UI.ToggleCore;
using VContainer;

public class SfxToggle : ToggleBehaviour
{
    /*[Inject]
    protected AudioManager _audioManager;*/
    
    public override void UpdateToggle()
    {
        base.UpdateToggle();

       // _toggle.isOn = !_audioManager.GetMuteStatus(SoundType.Sfx);

        var handleXPos = _toggle.isOn ? _handleXPos * -1f: _handleXPos;
        
        _handleRectTransform.anchoredPosition = _handleRectTransform.anchoredPosition.With(x: handleXPos);
        
        _backgroundImage.fillAmount = _toggle.isOn ? 1f : 0f;
    }

    protected override void OnValueChanged(bool value)
    {
        base.OnValueChanged(value);

       // _audioManager.Mute(SoundType.Sfx, !value);
    }
}