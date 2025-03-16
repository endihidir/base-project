using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UnityBase.UI.ViewCore
{
    public class CoinView : ICoinView
    {
        private TextMeshProUGUI _coinTxt;
        
        private ICoinModel _coinModel;
        
        private Tween _valueIncrease;
        public Transform CoinIconTransform { get; private set; }

        public ICoinView Initialize(Transform coinIconTransform, TextMeshProUGUI textMeshProUGUI, ICoinModel coinModel)
        {
            CoinIconTransform = coinIconTransform;
            _coinTxt = textMeshProUGUI;
            _coinModel = coinModel;
            _coinTxt.text = _coinModel.Coins.Value.ToString();
            return this;
        }

        public ICoinView UpdateView()
        {
            _coinTxt.text = _coinModel.Coins.Value.ToString();
            return this;
        }
        
        public void Dispose()
        {
            _valueIncrease?.Kill();
        }
    }
    
    public interface ICoinView : IView
    {
        public Transform CoinIconTransform { get; }
        public ICoinView Initialize(Transform coinIconTransform, TextMeshProUGUI textMeshProUGUI, ICoinModel coinModel);
        public ICoinView UpdateView();
    }
}