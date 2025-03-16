using DG.Tweening;
using TMPro;

namespace UnityBase.UI.ViewCore
{
    public class CoinView : ICoinView
    {
        private TextMeshProUGUI _coinTxt;
        
        private ICoinModel _coinModel;

        private int _value;
        
        private Tween _valueIncrease;
        
        public ICoinView Initialize(TextMeshProUGUI textMeshProUGUI, ICoinModel coinModel)
        {
            _coinTxt = textMeshProUGUI;
            _coinModel = coinModel;
            _value = _coinModel.Coins.Value;
            _coinTxt.text = _value.ToString();
            return this;
        }

        public ICoinView UpdateView()
        {
            /*_valueIncrease?.Kill();
            var dif = _coinModel.Coins.Value - _value;
            _valueIncrease = DOTween.To(GetValue, SetValue, _coinModel.Coins.Value, dif / 30f);*/
            SetValue(_coinModel.Coins.Value);
            return this;
        }

        private int GetValue() => _value;
        private void SetValue(int value)
        {
            _value = value;
            _coinTxt.text = _value.ToString();
        }
        public void Dispose()
        {
            _valueIncrease?.Kill();
        }
    }
    
    public interface ICoinView : IView
    {
        public ICoinView Initialize(TextMeshProUGUI textMeshProUGUI, ICoinModel coinModel);
        public ICoinView UpdateView();
    }
}