using NaughtyAttributes;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class ViewBehaviourTest : MonoBehaviour
{
    [Inject] private readonly ICurrencyManager _currencyManager;
    [SerializeField] private CoinIconTest _coinIconTestPrefab;
    [SerializeField] private Transform _parent;

    [Button]
    public void TestBehaviour()
    {
        var pos = new Vector3(Screen.width * 0.5f, Screen.height * 0.3f);
        _currencyManager.PlayCollect(500, _coinIconTestPrefab, _parent, pos);
    }
}
