using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
/*using __Funflare.Scripts.FPS;
using __Funflare.Scripts.Helper;
using __Funflare.Scripts.History;
using __Funflare.Scripts.RemoteConfigAdjuster;
using GenericOfferSDK;
using Newtonsoft.Json.Linq;
using SupersonicWisdomSDK;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;*/
using UnityEngine;
/*using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;*/
using Object = UnityEngine.Object;

namespace __Funflare.Scripts.Analytics
{
    public static class FunflareAnalytics
    {
        [Serializable]
        public struct UserAttributes
        {
            public int currentMart;
            public int incomePerCustomer;
            public bool payingUser;
            public int daysSinceLastPurchase;
            public int daysSinceFirstOpen;
            public float firstSeenAppVersion;
        }

        [Serializable]
        public struct AppAttributes
        {
            public string appVersion;
            public bool isTriggered;
        }

        /*private static CustomAnalytics _customAnalytics;
        public static bool HasInit { get; private set; }
        public static IABConfig AbConfig => _customAnalytics;
        public static IRemoteConfig RemoteConfig => _customAnalytics;
        public static IAnalytics Analytics => _customAnalytics;

        private static GenericOfferIntegrationData _genericOfferIntegrationData;*/

        public static bool IsAnyProductOwned { get; private set; }
        public static int DaysSinceLastPurchase { get; private set; } = -1;

        /*public static string EnvironmentID
        {
            get => ES3.Load("environment_id_key",
                defaultValue: UgsEnvironmentProvider.GetEnvironmentId(EnvironmentType.Development));
            private set => ES3.Save("environment_id_key", value);
        }*/

        public static float FirstSeenAppVersion
        {
            get
            {
                if (!PlayerPrefs.HasKey("first_seen_app_version"))
                {
                    PlayerPrefs.SetString("first_seen_app_version", Application.version);
                }

                var version = PlayerPrefs.GetString("first_seen_app_version");

                var lastDotIndex = version.LastIndexOf('.');

                return float.Parse(version.Remove(lastDotIndex, 1), CultureInfo.InvariantCulture);
            }
        }

        /*
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitEventHelper()
        {
            UnityEventHelper.Init();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (TryGetLastPurchasingDate(out DateTime lastPurchaseDate))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Latest purchasing date : " + lastPurchaseDate);
#endif
                var currentDate = FunflareTime.UTCNow;
                var dateDifference = currentDate - lastPurchaseDate;
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogError("Days since last purchase : " + dateDifference.Days);
#endif
                DaysSinceLastPurchase = dateDifference.Days;
            }
            
        }*/


        private static bool TryGetLastPurchasingDate(out DateTime dateTime)
        {
            var minDate = DateTime.MinValue;
            
            /*var allPurchasedProductIds = SupersonicWisdom.Api.GetPurchasedProductIds();

            dateTime = minDate;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError("Purchased product ids count : " + allPurchasedProductIds.Length);

            foreach (var purchasedProductId in allPurchasedProductIds)
            {
                Debug.LogError("Purchased product id : " + purchasedProductId);
            }
#endif

            //IDLogTest();

            if (allPurchasedProductIds is { Length: > 0 })
            {
                IsAnyProductOwned = true;

                var productCollection = SupersonicWisdom.Api.GetProductCollection();

                if (productCollection == null) return false;

                foreach (var product in productCollection.all)
                {
                    if (!allPurchasedProductIds.Contains(product.definition.id)) continue;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError("Purchased product id : " + product.definition.id + " is confirmed!");
#endif
                    if (product is not { hasReceipt: true }) continue;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"{product.definition.id} product has receipt");
#endif
                    var lastPurchasingDate = GetLastPurchaseDateWithValidation(product.receipt);

                    if (!lastPurchasingDate.HasValue) continue;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogError($"{product.definition.id} product purchasing date is {lastPurchasingDate.Value}");
#endif
                    if (lastPurchasingDate.Value > minDate)
                    {
                        minDate = lastPurchasingDate.Value;
                    }
                }
            }

            if (minDate == DateTime.MinValue) return false;*/

            dateTime = minDate;

            return true;
        }

        /*private static DateTime? GetLastPurchaseDateWithValidation(string receipt)
        {
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

            try
            {
                var result = validator.Validate(receipt);
                
                var lastPurchase = result
                    .OrderByDescending(x => x.purchaseDate)
                    .FirstOrDefault();

                if (lastPurchase != null)
                {
                    return lastPurchase.purchaseDate.Add(TimeZoneInfo.Local.BaseUtcOffset);
                }
            }
            catch (IAPSecurityException e)
            {
                Debug.LogError($"Failed to parse receipt: {e.Message}");
            }

            return null;
        }*/

        /*private static DateTime? GetLastPurchaseDate(string receipt)
        {
            try
            {
                JObject receiptJson;

                if (IsBase64String(receipt))
                {
                    string decodedReceipt = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(receipt));
                    receiptJson = JObject.Parse(decodedReceipt);
                }
                else
                {
                    receiptJson = JObject.Parse(receipt);
                }

                if (receiptJson["Store"].ToString() == "GooglePlay")
                {
                    var payloadArray = JArray.Parse(receiptJson["Payload"].ToString());

                    var lastPurchase = payloadArray
                        .OrderByDescending(payload => DateTimeOffset.FromUnixTimeMilliseconds((long)payload["purchaseTime"]).UtcDateTime)
                        .FirstOrDefault();

                    if (lastPurchase != null)
                    {
                        long purchaseTime = lastPurchase["purchaseTime"].ToObject<long>();
                        return DateTimeOffset.FromUnixTimeMilliseconds(purchaseTime).UtcDateTime;
                    }
                }
                else if (receiptJson["Store"].ToString() == "AppleAppStore")
                {
                    var inAppArray = receiptJson["receipt"]?["in_app"];
                    if (inAppArray != null && inAppArray.HasValues)
                    {
                        var lastPurchase = inAppArray
                            .OrderByDescending(purchase => DateTime.Parse(purchase["purchase_date"].ToString()).ToUniversalTime())
                            .FirstOrDefault();

                        if (lastPurchase != null)
                        {
                            string purchaseDateStr = lastPurchase["purchase_date"].ToString();
                            return DateTime.Parse(purchaseDateStr).ToUniversalTime();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse receipt: {e.Message}");
            }

            return null;
        }*/

        private static bool IsBase64String(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length % 4 != 0)
                return false;

            var base64Regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9\+/]*={0,2}$",
                System.Text.RegularExpressions.RegexOptions.None);
            return base64Regex.IsMatch(input);
        }
    }
}