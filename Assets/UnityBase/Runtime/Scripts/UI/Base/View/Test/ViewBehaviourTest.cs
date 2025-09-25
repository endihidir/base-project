using NaughtyAttributes;
using UnityBase.Service;
using UnityBase.UI.Dynamic;
using UnityEngine;
using VContainer;

public class ViewBehaviourTest : MonoBehaviour
{
    [Inject] private readonly ICurrencyManager _currencyManager;
    [SerializeField] private CoinIconTest _coinIconTestPrefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private CoinViewUI _coinViewUI;
    
    [Button]
    public void TestBehaviour()
    {
        if(!_coinViewUI) return;
        var pos = new Vector3(Screen.width * 0.5f, Screen.height * 0.3f);
        _currencyManager.PlayCollectCoin(_coinViewUI.OwnerContext,500, _coinIconTestPrefab, _parent, pos);
    }
}
