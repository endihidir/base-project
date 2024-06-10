using System;
using TMPro;
using UnityBase.UI.ViewCore;
using UnityEngine;
using VContainer;

public class CoinUI : MonoBehaviour, IViewUI
{
    [SerializeField] private bool _useTypeForKey = true;
    
    [SerializeField] private TextMeshProUGUI _coinTxt;

    [SerializeField] private Transform _coinIconT;
    public Type Key => GetType();

    private IViewBehaviour _viewAnimBehaviour, _viewModelBehaviour;
    
    [Inject]
    private void Construct(IViewBehaviourFactory viewBehaviourFactory)
    {
        _viewAnimBehaviour = viewBehaviourFactory.CreateViewAnimationBehaviour<InOutView>(this);
    }
    
    private void OnDestroy()
    {
        _viewAnimBehaviour?.Dispose();
        _viewModelBehaviour?.Dispose();
    }
}