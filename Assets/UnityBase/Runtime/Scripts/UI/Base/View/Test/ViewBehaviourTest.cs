using NaughtyAttributes;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class ViewBehaviourTest : MonoBehaviour
{
    [SerializeField] private CoinIconTest _coinIconTestPrefab;
    
    [Inject] private readonly ICurrencyManager _currencyManager;
    [Inject] private readonly IPoolManager _poolManager;

    [SerializeField] private Transform _parent;

    [Button]
    public void TestBehaviour()
    {
        var coinIcon = _poolManager.GetObject(_coinIconTestPrefab,false);
        
        coinIcon.Show(0f, 0f, null);
        coinIcon.transform.SetParent(_parent);
        coinIcon.transform.localScale = Vector3.one * 0.5f;
        coinIcon.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.3f);
        
        
        coinIcon.MoveTo(_currencyManager.CoinIconT,0.5f, ()=>
        {
            _currencyManager.SaveCoinData(500);
            _currencyManager.UpdateCoinView();
        },
        () =>
        {
            _poolManager.HideObject(coinIcon, 0f, 0.1f);
        });
    }
}
