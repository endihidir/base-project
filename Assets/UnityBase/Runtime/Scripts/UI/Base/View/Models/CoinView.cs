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
        
        private int _currentCoin;
        public Transform CoinIconTransform { get; private set; }

        public ICoinView Initialize(Transform coinIconTransform, TextMeshProUGUI textMeshProUGUI, ICoinModel coinModel)
        {
            CoinIconTransform = coinIconTransform;
            _coinTxt = textMeshProUGUI;
            _coinModel = coinModel;
            _currentCoin = _coinModel.Coins.Value;
            _coinTxt.text = _currentCoin.ToString();
            return this;
        }

        public ICoinView UpdateView()
        {
            _valueIncrease?.Kill();
            
            var duration = ((Mathf.Abs(_currentCoin - _coinModel.Coins.Value) * 0.1f) - 0.1f) * 0.5f;
            
            duration = Mathf.Clamp(duration, 0, 0.35f);
            
            _valueIncrease = DOVirtual.Int(_currentCoin, _coinModel.Coins.Value, duration, x=>
            {
                _currentCoin = x;
                _coinTxt.text = x.ToString();
            });
            
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