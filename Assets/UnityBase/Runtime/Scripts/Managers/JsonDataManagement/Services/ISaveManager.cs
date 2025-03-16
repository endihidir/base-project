
namespace UnityBase.Service
{
   public interface ISaveManager
   {
      public void SaveToJson<T>(string key, T data);
      public T LoadFromJson<T>(string key, T defaultData = default, bool autoSaveDefaultData = true);
      public void SaveToPrefs<T>(string key, T data);
      public T LoadFromPrefs<T>(string key, T defaultData = default);
   }
}
