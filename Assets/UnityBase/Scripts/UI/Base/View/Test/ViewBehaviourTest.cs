using Sirenix.OdinInspector;
using UnityBase.Service;
using UnityEngine;
using VContainer;

public class ViewBehaviourTest : MonoBehaviour
{
    [Inject] private readonly ICoinManager _coinManager;
    [Inject] private readonly IPoolManager _poolManager;

    [SerializeField] private Transform _parent;

    [Button]
    public void TestBehaviour()
    {
        var coinIcon = _poolManager.GetObject<CoinIconTest>(false);
        
        coinIcon.Show(0f, 0f, null);
        coinIcon.transform.SetParent(_parent);
        coinIcon.transform.localScale = Vector3.one * 0.5f;
        //coinIcon.transform.SetAsFirstSibling();
        coinIcon.transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.3f);
        
        coinIcon.MoveTo(_coinManager.CoinIconT, ()=>
        {
            _coinManager.Collect(1);
            //_coinManager.PlayBounceAnim(null);
            _poolManager.HideObject(coinIcon, 0f, 0.1f);
        });
    }
}
