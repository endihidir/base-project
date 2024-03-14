using Cysharp.Threading.Tasks;
using Sirenix.Serialization;

namespace UnityBase.Service
{
   public interface IJsonDataService
   {
      public bool Save<T>(string key, T data);
      public T Load<T>(string key, T defaultData = default, bool autoSaveDefaultData = true);
      public UniTask<bool> SaveAsync<T>(string key, T data);
      public UniTask<T> LoadAsync<T>(string key, T defaultData = default, bool autoSaveDefaultData = true);
   }
}
