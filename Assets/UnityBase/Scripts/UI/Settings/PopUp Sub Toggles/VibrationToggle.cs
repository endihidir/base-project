using UnityBase.Extensions;
using UnityBase.UI.ToggleCore;

public class VibrationToggle : ToggleBehaviour
{
    public override void UpdateToggle()
    {
        base.UpdateToggle();
        
       // _toggle.isOn = HapticManager.isVibrationEnabled;
        
        var handleXPos = _toggle.isOn ? _handleXPos * -1f: _handleXPos;
        
        _handleRectTransform.anchoredPosition = _handleRectTransform.anchoredPosition.With(x : handleXPos);
        
        _backgroundImage.fillAmount = _toggle.isOn ? 1f : 0f;
    }

    protected override void OnValueChanged(bool value)
    {
        base.OnValueChanged(value);

        //HapticManager.SetActive(value);
    }
}