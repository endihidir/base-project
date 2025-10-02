using NaughtyAttributes;
using UnityBase.Service;
using UnityBase.UI.Dynamic;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

public class ViewBehaviourTest : MonoBehaviour
{
    [Inject] private readonly ICurrencyManager _currencyManager;
    [SerializeField] private CoinIconTest _coinIconTestPrefab;
    [SerializeField] private Transform _parent;
    [FormerlySerializedAs("coinMonoViewMonoView")] [FormerlySerializedAs("coinViewView")] [FormerlySerializedAs("_coinViewUI")] [SerializeField] private CoinViewUI coinViewUI;
    
    [Button]
    public void TestBehaviour()
    {
        if(!coinViewUI) return;
        var pos = new Vector3(Screen.width * 0.5f, Screen.height * 0.3f);
        _currencyManager.PlayCollectCoin(coinViewUI.OwnerContext,500, _coinIconTestPrefab, _parent, pos);
    }
}
