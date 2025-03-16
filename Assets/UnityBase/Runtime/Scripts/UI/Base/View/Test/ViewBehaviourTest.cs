using NaughtyAttributes;
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
        
        _coinManager.SaveData(1);
        
        coinIcon.MoveTo(_coinManager.CoinIconT,0.5f, ()=>
        {
            _coinManager.UpdateView();
            _coinManager.PlayBounceAnim();
            _poolManager.HideObject(coinIcon, 0f, 0.1f);
        });
    }
}
