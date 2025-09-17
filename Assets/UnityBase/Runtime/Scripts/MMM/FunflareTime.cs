using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace __Funflare.Scripts
{
    public static class FunflareTime
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static int CheatTimeAdditionHours
        {
            get => PlayerPrefs.GetInt("cheat_time_addition", 0);
            set => PlayerPrefs.SetInt("cheat_time_addition", value);
        }

        public static void ClearTimeAddition()
        {
            PlayerPrefs.DeleteKey("cheat_time_addition");
        }
        
#endif

        private const string FirstLaunchKey = "first_launch_date_time";
        public static DateTime FirstLaunchDateTime
        {
            get => DateTime.ParseExact(PlayerPrefs.GetString(FirstLaunchKey, DateTime.MinValue.ToString("dd MM yyyy HH:mm:ss", CultureInfo.InvariantCulture)), "dd MM yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            private set => PlayerPrefs.SetString(FirstLaunchKey, value.ToString("dd MM yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        }
        
        public static int DaysSinceFirstOpen { get; private set; }
        public static bool HasInit { get; private set; }
        

        [RuntimeInitializeOnLoadMethod]
        private static async void Init()
        {
            /*FunflareSignals.OnApplicationPause += OnApplicationPause;
#if UNITY_EDITOR
            FunflareSignals.OnApplicationQuit += OnApplicationQuit;
#endif
            FunflareUpdateProvider.RegisterUpdate(OnUpdate);

            //Move ES3 Save To Player Prefs
            {
                if (ES3.KeyExists("ff_time_total_time"))
                {
                    SavedTime = ES3.Load<float>("ff_time_total_time");
                    ES3.DeleteKey("ff_time_total_time");
                }

                if (ES3.KeyExists("ff_time_total_time_each_mart"))
                {
                    SavedMartTime = ES3.Load<float>("ff_time_total_time_each_mart");
                    ES3.DeleteKey("ff_time_total_time_each_mart");
                }
            }*/


            _startTime = await GetUtcTimeAsync();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _startTime += TimeSpan.FromHours(CheatTimeAdditionHours);  
#endif

            _sessionEnterTime = _startTime;
            _martSessionEnterTime = _startTime;


            //Should only be here if somehow on app pause did not work e.g maybe crash ??
            if (SavedSessionTime > 0)
            {
                var copy = SavedSessionTime;
                SavedTime += SavedSessionTime;
                SavedSessionTime = 0;
                Debug.LogError("Last session duration Session " + copy + " missing re adding to game time");
            }
            else if (SavedSessionTime < 0)
            {
                Debug.LogError("Saved Session Time is " + SavedSessionTime + " This should not happen");
                SavedSessionTime = 0;
            }

            if (SavedMartSessionTime > 0)
            {
                var copy = SavedMartSessionTime;
                SavedMartTime += SavedMartSessionTime;
                SavedMartSessionTime = 0;
                Debug.LogError("Last mart session duration " + copy + " missing re adding to game time");
            }
            else if (SavedMartSessionTime < 0)
            {
                Debug.LogError("Saved Mart Session Time is " + SavedMartSessionTime + " This should not happen");
                SavedMartSessionTime = 0;
            }

            if (!PlayerPrefs.HasKey(FirstLaunchKey))
            {
                FirstLaunchDateTime = UTCNow;
            }
            
            DaysSinceFirstOpen = (UTCNow.Date - FirstLaunchDateTime.Date).Days;

            HasInit = true;

            //Debug.LogError("App Start Total Time " + TotalGameTime);
            //Debug.LogError("App Start Total Mart Time " + TotalMartTime);

            /*var timeRequestTask = GetCurrentTimeRequest();
            await timeRequestTask;
            if (timeRequestTask.Exception != null)
            {
                _startTime = timeRequestTask.Result.Now;
            }*/
        }

        private static DateTime _startTime;

        public static DateTime UTCNow => (_startTime + TimeSpan.FromSeconds(Time.realtimeSinceStartup));

        private static DateTime _sessionEnterTime, _martSessionEnterTime;

        private static float _cachedSavedTime;

        private static bool _isCachedSavedTimeValid;

        private static float SavedTime
        {
            get
            {
                if (!_isCachedSavedTimeValid)
                {
                    _isCachedSavedTimeValid = true;
                    //_cachedSavedTime = ES3.Load<float>("ff_time_total_time", 0);
                    _cachedSavedTime = PlayerPrefs.GetFloat("ff_time_total_time", 0);
                }

                return _cachedSavedTime;
            }

            set
            {
                //ES3.Save<float>("ff_time_total_time", value);
                PlayerPrefs.SetFloat("ff_time_total_time", value);
                _cachedSavedTime = value;
            }
        }

        public static float SavedMartTime
        {
            get => PlayerPrefs.GetFloat("ff_time_total_time_each_mart", 0);
            private set => PlayerPrefs.SetFloat("ff_time_total_time_each_mart", value);
        }

        public static float SavedSessionTime
        {
            get => PlayerPrefs.GetFloat("ff_saved_session_time", 0);
            private set => PlayerPrefs.SetFloat("ff_saved_session_time", value);
        }

        public static float SavedMartSessionTime
        {
            get => PlayerPrefs.GetFloat("ff_saved_mart_session_time", 0);
            private set => PlayerPrefs.SetFloat("ff_saved_mart_session_time", value);
        }


        public static float SavedGameTime => SavedTime;
        public static float TotalGameTime => SavedTime + SessionTime;
        public static float TotalMartTime => SavedMartTime + MartSessionTime;

        public static float SessionTime
        {
            get
            {
                var result = (float)(UTCNow - _sessionEnterTime).TotalSeconds;
                return result;
            }
        }

        private static float MartSessionTime => (float)(UTCNow - _martSessionEnterTime).TotalSeconds;

        private static float _nextSessionSaveTime = 0;


#if UNITY_EDITOR
        private static void OnApplicationQuit()
        {
            //Debug.LogError("App Quit Total Time " + TotalGameTime);
            //Debug.LogError("App Quit Total Mart Time " + TotalMartTime);
            SavedTime += SessionTime;
            SavedMartTime += MartSessionTime;
            SavedSessionTime = 0;
            SavedMartSessionTime = 0;
        }
#endif

        private static void OnUpdate()
        {
            if (Time.unscaledTime < _nextSessionSaveTime) return;
            _nextSessionSaveTime = Time.unscaledTime + 5f;
            SavedSessionTime = SessionTime;
            SavedMartSessionTime = MartSessionTime;
        }

        private static void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                SavedTime += SessionTime;
                SavedMartTime += MartSessionTime;
                SavedSessionTime = 0;
                SavedMartSessionTime = 0;
            }
            else
            {
                _sessionEnterTime = UTCNow;
                _martSessionEnterTime = UTCNow;
            }
        }

        public static void ResetMartTime()
        {
            SavedMartTime = 0;
            SavedMartSessionTime = 0;
            _martSessionEnterTime = UTCNow;
        }


        private static async Task<DateTimeData> GetCurrentTimeRequest()
        {
            string timeApiUrl = "https://worldtimeapi.org/api/timezone/Etc/UTC";

            try
            {
                var timeApiResponseBody = await FetchDataFromUrl(timeApiUrl);
                TimeApiResponse timeApiResult = JsonConvert.DeserializeObject<TimeApiResponse>(timeApiResponseBody);
                DateTime localTime = DateTime.Parse(timeApiResult.DateTime);
                return new DateTimeData() { Now = localTime, Exception = null };
            }
            catch (HttpRequestException e)
            {
                Debug.LogError(e);
                return new DateTimeData() { Now = DateTime.Now, Exception = e };
            }
            catch (TaskCanceledException e)
            {
                Debug.LogError(e);
                return new DateTimeData() { Now = DateTime.Now, Exception = e };
            }
        }

        private static async Task<string> FetchDataFromUrl(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 5;

                var asyncOperation = request.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
                {
                    throw new HttpRequestException(request.error);
                }

                return request.downloadHandler.text;
            }
        }

        private struct DateTimeData
        {
            public DateTime Now;
            public Exception Exception;
        }

        private struct IpApiResponse
        {
            [JsonProperty("timezone")] public string Timezone { get; set; }
        }

        private struct TimeApiResponse
        {
            [JsonProperty("datetime")] public string DateTime { get; set; }
        }

        private static async Task<DateTime> GetUtcTimeAsync()
        {
            //#if UNITY_EDITOR

            //#endif
            try
            {
                var client = new TcpClient();
                await client.ConnectAsync("time.nist.gov", 13);
                using var streamReader = new StreamReader(client.GetStream());
                var response = await streamReader.ReadToEndAsync();
                var utcDateTimeString = response.Substring(7, 17);
                return DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                //   Debug.Log("time now in Time Manager inside after is " + UtcNow);
            }
            catch
            {
                return DateTime.Now;
                //     Debug.Log("time now in Time Manager inside after is error " + UtcNow);
                // Handle errors here
            }
            //   Debug.Log("time now in Time Manager after is " + UtcNow);
        }
    }
}