using TMPro;
using UnityBase.Manager;
using UnityBase.Service;
using UnityBase.UI.ButtonCore;
using UnityEngine;
using VContainer;

public class CurrencyUI : MonoBehaviour
{
    [Inject] 
    private readonly ICurrencyManagementService _currencyManagementService;
    
    [SerializeField] private TextMeshProUGUI _coinTxt;

    [SerializeField] private Transform _coinIconT;
    
    
    public Transform ViewTransform => transform;
    public Transform CoinIconT => _coinIconT;

    [Inject]
    private void Construct(IUIBehaviourFactory uiBehaviourFactory)
    {
        
    }
    /*
    private void PlayCoinIconAnimation()
    {
        _iconScaleUpAnim?.Kill(true);
        _iconScaleUpAnim = _coinIconT.transform.DOPunchScale(Vector3.one * 0.6f, 0.2f)
                                               .OnComplete(()=> _coinIconT.transform.localScale = Vector3.one);
    }*/
    
    public void UpdateView(int val)
    {
        
    }
}
