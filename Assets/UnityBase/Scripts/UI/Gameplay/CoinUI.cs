using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityBase.UI.ViewCore;
using UnityEngine;
using VContainer;

public class CoinUI : MonoBehaviour, IViewAnimUI<InOutView>, IViewModelUI<CoinViewModel, int>
{
    [SerializeField] private bool _useTypeForKey = true;
    
    [SerializeField] private TextMeshProUGUI _coinTxt;

    [SerializeField] private Transform _coinIconT;
 
    public Type Key => GetType();

    [ShowInInspector, ReadOnly] public int Value { get; set; }
    public IViewAnimBehaviour<InOutView> AnimBehaviour { get; private set; }
    public IViewModelBehaviour<CoinViewModel, int> ModelBehaviour { get; private set; }
    
    [Inject]
    private void Construct(IViewBehaviourFactory viewBehaviourFactory)
    {
        AnimBehaviour = viewBehaviourFactory.CreateViewAnim(this);
        ModelBehaviour = viewBehaviourFactory.CreateViewModel(this);
    }

    private void OnDestroy()
    {
        AnimBehaviour?.Dispose();
        ModelBehaviour?.Dispose();
    }
}