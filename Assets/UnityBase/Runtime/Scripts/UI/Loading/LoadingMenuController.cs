using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityBase.Manager;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class LoadingMenuController : MonoBehaviour, ILoadingMenuController
{
    [Inject] private readonly ISceneLoader _sceneLoader;

    [SerializeField] private CanvasGroup _loadingCanvasGroup;
    
    [SerializeField] private Image _sliderImage;
    
    [SerializeField] private TextMeshProUGUI _sliderTxt;
    
    [SerializeField] private float _fillSpeed = 0.5f;
    
    private float _targetProgress;
    
    protected void OnEnable() => _sceneLoader.LoadingProgress.Progressed += OnProgress;
    protected void OnDisable() => _sceneLoader.LoadingProgress.Progressed -= OnProgress;
    private void OnProgress(float val) => _targetProgress = val;
    private void Update()
    {
        var currentFillAmount = _sliderImage.fillAmount;
        var progressDifference = Mathf.Abs(currentFillAmount - _targetProgress);
        var dynamicFillSpeed = progressDifference * _fillSpeed;
        _sliderImage.fillAmount = Mathf.MoveTowards(currentFillAmount, _targetProgress, Time.deltaTime * dynamicFillSpeed) * 1.1f;
        var percentage = _sliderImage.fillAmount * 100f;
        _sliderTxt.text = "Loading... " + percentage.ToString("0.0") +"%";
    }

    public async UniTask SetActive(bool value)
    {
        await _loadingCanvasGroup.DOFade(value ? 1f : 0f, 0.1f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
    }

    public void Reset()
    {
        _targetProgress = 0f;
        _sliderImage.fillAmount = _targetProgress;
    }
}

public interface ILoadingMenuController
{
    public UniTask SetActive(bool value);
    public void Reset();
}