using UnityBase.Observable;
using UnityBase.Service;

namespace UnityBase.UI.ViewCore
{
    public class CoinViewModel : IViewModel<int>
    {
        public Observable<int> Values { get; }

        private readonly IJsonDataManager _jsonDataManager;
        
        public CoinViewModel(IViewModelUI<CoinViewModel, int> viewModelUI, IJsonDataManager jsonDataManager)
        {
            _jsonDataManager = jsonDataManager;
           
            Values = new Observable<int>(viewModelUI.Value);
            
            Values.AddListener(OnValueChanged);
        }
        
        public void Configure()
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