using TMPro;

namespace UnityBase.UI.ViewCore
{
    public class CoinView : ICoinView
    {
        private TextMeshProUGUI _coinTxt;
        
        private ICoinModel _coinModel;
        
        public ICoinView Initialize(TextMeshProUGUI textMeshProUGUI, ICoinModel coinModel)
        {
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
            
        }
    }
    
    public interface ICoinView : IView
    {
        public ICoinView Initialize(TextMeshProUGUI textMeshProUGUI, ICoinModel coinModel);
        public ICoinView UpdateView();
    }
}