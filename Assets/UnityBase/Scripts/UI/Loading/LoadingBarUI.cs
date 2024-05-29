using TMPro;
using UnityBase.Service;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LoadingBarUI : MonoBehaviour
{
    [SerializeField] private Slider _progressUI;
    [SerializeField] private TextMeshProUGUI _sliderTxt;

    [Inject] 
    private readonly ISceneManagementService _sceneManagementService;

    protected void OnEnable() => _sceneManagementService.OnLoadUpdate += SetProgressValue;
    protected void OnDisable() => _sceneManagementService.OnLoadUpdate -= SetProgressValue;

    private void SetProgressValue(float val)
    {
        if(!_progressUI) return;

        _progressUI.value = (val * 90f);
        
        var valTxt = (val * 90f).ToString("0.0");

        _sliderTxt.text = "Loading... " + valTxt +"%";
    }
}