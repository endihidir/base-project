using TMPro;
using UnityBase.Runtime.Factories;
using UnityBase.UI;
using UnityEngine;
using UnityEngine.UI;

public class LoadingMenuController : MonoView
{
    [SerializeField] private CanvasGroup _loadingCanvasGroup;
    [SerializeField] private Image _sliderImage;
    [SerializeField] private TextMeshProUGUI _sliderTxt;
    [SerializeField] private float _fillSpeed = 0.5f;

    protected override void Initialize()
    {
        var loadingModel = OwnerContext.ResolveModel<LoadingModel>().Initialize(_fillSpeed);
        var loadingView = OwnerContext.ResolveView<LoadingView>().Initialize(_loadingCanvasGroup, _sliderImage, _sliderTxt);
        OwnerContext.ResolvePresenter<LoadingPresenter>().Initialize(loadingModel, loadingView);
    }
}