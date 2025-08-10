using Google.MiniJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using TenjFix;
using Tenjin;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Ray.Services
{
    public class TenjinService : MonoBehaviour
    {
        private BaseTenjin _baseTenjin;
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        public static TenjinService Instance;
        private void Awake()
        {
            Instance = this;
        }

        void OnApplicationPause(bool isPaused)
        {
            if (!isPaused && _baseTenjin != null)
            {
                _baseTenjin.Connect();
            }
        }

        public void Initialize()
        {
#if UNITY_ANDROID || UNITY_EDITOR
            // Get the Tenjin instance (you'll need to check the correct way to get the instance)
            _baseTenjin = GetComponent<BaseTenjin>(); // or however you get the Tenjin instance
            if (_baseTenjin == null)
            {
                // If no component exists, you might need to add one or get it differently
                // This depends on how your Tenjin SDK is set up
                _baseTenjin = gameObject.AddComponent<AndroidTenjin>(); // Replace with actual Android implementation class
            }
            
            _baseTenjin.Init("CRQI2QZDASGH1YASSFAFXAIXTXDHCV5X");
            _baseTenjin.Connect();
            _baseTenjin.SubscribeAppLovinImpressions();
#elif UNITY_IOS
            InitializeForIos();
#endif
        }

        public void InitializeForIos()
        {
            #if UNITY_IOS
            // Get the Tenjin instance for iOS
            _baseTenjin = GetComponent<BaseTenjin>();
            if (_baseTenjin == null)
            {
                _baseTenjin = gameObject.AddComponent<TenjiniOS>(); // Replace with actual iOS implementation class
            }
            
            _baseTenjin.Init("CRQI2QZDASGH1YASSFAFXAIXTXDHCV5X");
            
            if (new Version(Device.systemVersion).CompareTo(new Version("14.0")) >= 0)
            {
                // Tenjin wrapper for requestTrackingAuthorization
                _baseTenjin.RequestTrackingAuthorizationWithCompletionHandler((status) => {
                       Debug.Log("===> App Tracking Transparency Authorization Status: " + status);
                       _baseTenjin.SetAppStoreType(AppStoreType.other);
                       _baseTenjin.Connect();
                       _baseTenjin.SubscribeAppLovinImpressions();
                });
            }
            else
            {
                _baseTenjin.SetAppStoreType(AppStoreType.other);
                _baseTenjin.Connect();
                _baseTenjin.SubscribeAppLovinImpressions();
            }
#endif
        }

        private async void GetAttribution()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            while (string.IsNullOrEmpty(Database.UserData.Tenjin.AdNetwork))
            {
                SendAttributionRequest();
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        private void SendAttributionRequest()
        {
            _baseTenjin.GetAttributionInfo((Dictionary<string, string> attributionInfoData) =>
            {
                if (attributionInfoData != null)
                {
                    string attributionInfoText = "Attribution Info:\n";
                    foreach (KeyValuePair<string, string> kvp in attributionInfoData)
                    {
                        attributionInfoText += $"Key: {kvp.Key}, Value: {kvp.Value}\n";
                    }

                    _rayDebug.Log(attributionInfoText, this);

                    if (attributionInfoData.ContainsKey("ad_network")) Database.UserData.Tenjin.AdNetwork = attributionInfoData["ad_network"];
                    if (attributionInfoData.ContainsKey("advertising_id")) Database.UserData.Tenjin.AdvertisementId = attributionInfoData["advertising_id"];
                    if (attributionInfoData.ContainsKey("click_id")) Database.UserData.Tenjin.ClickId = attributionInfoData["click_id"];
                }
            });
        }

        public void SendCheatEvent(string fieldKey, string oldValue, string newValue)
        {
            _baseTenjin.SendEvent($"Cheated_{fieldKey}", newValue);
        }

        public void SendReachEvent(bool cheater, int eventValue)
        {
            string tag = cheater ? "Cheater_" : string.Empty;
            //_baseTenjin.SendEvent($"{tag}{ProductInitials().ToLower()}_Player_Reached_{eventValue}_reachlevel", Database.UserData.Stats.TotalSessions.ToString());
        }

        public void SendAndroidTransactionEvent(string ProductId, string CurrencyCode, int Quantity, double UnitPrice, string Receipt, string Signature)
        {
            _baseTenjin.Transaction(ProductId, CurrencyCode, Quantity, UnitPrice, null, Receipt, Signature);
        }

        public void SendIosTransactionEvent(string ProductId, string CurrencyCode, int Quantity, double UnitPrice, string TransactionId, string Receipt)
        {
            _baseTenjin.Transaction(ProductId, CurrencyCode, Quantity, UnitPrice, TransactionId, Receipt, null);
        }

        public void SendCompletedInAppPurchaseEvent(Product product, int rewardAmount)
        {
            var wrapper = Json.Deserialize(product.receipt) as Dictionary<string, object>;
            if (null == wrapper)
            {
                return;
            }

            var productId = product.definition.id;
            _baseTenjin.SendEvent("iap_" + productId.ToString() + "_purchased", rewardAmount.ToString());
            Debug.Log($"Send Consumable Tenjin Event : ID : {productId} + Purchase Amount : {rewardAmount}");
        }

        private string ProductInitials()
        {
            string initials = "";
            foreach (char c in Application.productName)
            {
                initials += c;
            }
            return initials;
        }

        public void SendAllEvents()
        {
            // Cheat Events
            SendCheatEvent("HighestReachEvent", "-1", "-1");
            SendCheatEvent("ReachLevel", "-1", "-1");
            SendCheatEvent("RvCount", "-1", "-1");
            SendCheatEvent("SpaceLevel", "-1", "-1");
            SendCheatEvent("TotalCurrency", "-1", "-1");

            // Send Hightest Reach Events
            List<int> sortedReachEvents = Database.GameSettings.Events.SortedReachEvents();
            foreach (var e in sortedReachEvents)
            {
                SendReachEvent(false, e);
                SendReachEvent(true, e);
            }
        }
    }
}