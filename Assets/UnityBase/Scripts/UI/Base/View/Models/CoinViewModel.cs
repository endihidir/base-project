using UnityBase.Observable;
using UnityBase.Service;

namespace UnityBase.UI.ViewCore
{
    public class CoinViewModel : IViewModel<int>
    {
        public Observable<int> Values { get; }

        private readonly IViewUI _viewUI;

        private readonly IJsonDataManager _jsonDataManager;

        private int _value;
        
        public CoinViewModel(IViewUI viewUI, IJsonDataManager jsonDataManager)
        {
            _viewUI = viewUI;
            
            _jsonDataManager = jsonDataManager;
           
            Values = new Observable<int>(_value);
            
            Values.AddListener(OnValueChanged);
        }
        
        public void Configure()
        {
            
        }

        public void ChangeValue(int value)
        {
            
        }

        public void OnValueChanged(int value)
        {
            
        }

        public T Serialize<T>() where T : struct
        {
            return _jsonDataManager.Load<T>("CoinData");
        }

        public void Deserialize<T>(T value) where T : struct
        {
            _jsonDataManager.Save("CoinData", value);
        }
        
        public void Dispose() => Values?.Dispose();
    }
}