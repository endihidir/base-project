using TMPro;
using UnityBase.Service;
using UnityBase.UI.Base;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LoadingBarUI : BaseUIElements
{
    [SerializeField] private Slider _progressUI;
    [SerializeField] private TextMeshProUGUI _sliderTxt;

    [Inject] 
    private readonly ISceneDataService _sceneDataService;

    protected override void OnEnable()
    {
        _sceneDataService.OnLoadUpdate += SetProgressValue;
    }

    protected override void OnDisable()
    {
        _sceneDataService.OnLoadUpdate -= SetProgressValue;
    }

    private void SetProgressValue(float val)
    {
        if(!_progressUI) return;

        _progressUI.value = (val * 90f);
        
        var valTxt = (val * 90f).ToString("0.0");

        _sliderTxt.text = "Loading... " + valTxt +"%";
    }
}