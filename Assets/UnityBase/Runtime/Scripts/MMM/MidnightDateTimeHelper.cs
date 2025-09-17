using System;
//using System.Threading.Tasks;
using __Funflare.Scripts;
using UnityEngine;

namespace __Funflare.Game.Scripts.UpgradeMenu
{
    public static class MidnightDateTimeHelper
    {
        public static Action OnMidnightPassed { get; internal set; }
        private static float RemainingSecondsToPassDay { get; set; }
        /*private static DateTime SavedDate
        {
            get => ES3.Load<DateTime>(SAVED_DATE);
            set => ES3.Save(SAVED_DATE, value);
        }
        private static bool IsSavedDateExist => ES3.KeyExists(SAVED_DATE);*/
        
        private const string SAVED_DATE = "funflare_saved_date_time_for_midnight";

        /*
        [RuntimeInitializeOnLoadMethod]
        private static async void Init()
        {
            /*while (!FunflareAnalytics.HasInit)
            {
                await Task.Yield();
            }
            #1#
            
           // InvokeMidnightPassing();
            
           // FunflareUpdateProvider.RegisterUpdate(OnUpdate);
        }
        */
        
        private static void InvokeMidnightPassing()
        {
            var utcNow = FunflareTime.UTCNow;
            
            var endOfDay = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 23, 59, 59,999);

            var remainingSeconds = Mathf.Abs((float)(endOfDay - utcNow).TotalSeconds);
            
            RemainingSecondsToPassDay = remainingSeconds < 1f ? (float)TimeSpan.FromHours(24).TotalSeconds : remainingSeconds;
            
            /*if (IsSavedDateExist)
            {
                if (utcNow.Year != SavedDate.Year || utcNow.Month != SavedDate.Month || utcNow.Day != SavedDate.Day || remainingSeconds < 1f)
                {
                    SavedDate = utcNow;
                    OnMidnightPassed?.Invoke();
                }
            }
            else
            {
                SavedDate = utcNow;
            }*/
        }
        
        private static void OnUpdate()
        {
            RemainingSecondsToPassDay = Mathf.Max(RemainingSecondsToPassDay - Time.unscaledDeltaTime, 0f);
            
            if (RemainingSecondsToPassDay <= 0f)
            {
                InvokeMidnightPassing(); 
            }
        }

        public static bool CheckMidnightPassed()
        {
            var utcNow = FunflareTime.UTCNow;

            return false /*utcNow.Year != SavedDate.Year || utcNow.Month != SavedDate.Month || utcNow.Day != SavedDate.Day*/;
        }
    }
}