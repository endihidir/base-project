using TMPro;
using UnityBase.UI.ViewCore;
using UnityEngine;
using VContainer;

public class CoinUI : MonoBehaviour, IViewUI
{
    [SerializeField] private bool _useTypeForKey = true;
    
    [SerializeField] private TextMeshProUGUI _coinTxt;

    [SerializeField] private Transform _coinIconT;
    
    [Inject]
    public void Construct(IViewBehaviourFactory viewBehaviourFactory)
    {
        viewBehaviourFactory.CreateViewAnimation<MoveView>(this);
        viewBehaviourFactory.CreateViewModel<CoinViewModel>(this);
    }
    
    private void OnDestroy()
    {
       
    }
}