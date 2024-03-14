using DG.Tweening;
using TMPro;
using UnityBase.Manager;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class CoinUI : MonoBehaviour, ICoinView
{
    [Inject] 
    private readonly ICurrencyDataService _currencyDataService;
    
    [SerializeField] private TextMeshProUGUI _coinTxt;

    [SerializeField] private Transform _coinIconT;
    public Transform CoinIconT => _coinIconT;

    private int _defaultValue;

    private Tween _iconScaleUpAnim;
    
    private void Awake() => UpdateView(_currencyDataService.SavedCoinAmount);

    public void UpdateView(int val)
    {
        _defaultValue += val;
        _coinTxt.text = _defaultValue.ToString("0");
        PlayCoinIconAnimation();
    }

    private void PlayCoinIconAnimation()
    {
        _iconScaleUpAnim?.Kill(true);
        _iconScaleUpAnim = _coinIconT.transform.DOPunchScale(Vector3.one * 0.6f, 0.2f)
                                               .OnComplete(()=> _coinIconT.transform.localScale = Vector3.one);
    }

    private void OnDestroy() => _iconScaleUpAnim?.Kill();
}