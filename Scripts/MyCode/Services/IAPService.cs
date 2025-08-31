using Google.MiniJSON;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace Ray.Services
{

    public class IAPService : MonoBehaviour, IDetailedStoreListener
    {
        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Services);

        [Header("Fake Store Settings")]
        [SerializeField] private bool _mockSubsription = false;
        [SerializeField] private bool _useFakeStore = false;
        [SerializeField] private bool _useFakeStoreUIMode = false;

        #if UNITY_IOS
        [SerializeField] private bool _useAskToBuy = false;
        #endif

        private static IStoreController m_StoreController;

        // ADD THIS: Missing validator field
        private static CrossPlatformValidator m_Validator;

#if UNITY_IOS
    private static IAppleExtensions m_AppleExtensions;
        private static IExtensionProvider extensionProvider;
#endif

        private TaskCompletionSource<bool> _iapInitializationTCS;

        public static IAPService Instance;
        private void Awake()
        {
            Instance = this;
        }

        public void MockSubscription()
        {
            _mockSubsription = true;
        }

        private void OnEnable()
        {
            EventService.UI.OnIAPPurchaseBtn += HandleOnPurchaseBtn;
        }

        public async Task Initialize()
        {

            await LoadUGS();

            await InitializePurchasing();

        }

        private async Task LoadUGS()
        {
            try
            {
                var options = new InitializationOptions()
                    .SetEnvironmentName("production");

                await UnityServices.InitializeAsync(options);
            }
            catch (Exception exception)
            {
                Debug.LogError("Unity UGS could'nt initialize : " + exception.ToString());
            }
        }
        private Task InitializePurchasing()
        {

            _iapInitializationTCS = new TaskCompletionSource<bool>();

            ConfigurationBuilder builder;

            if (_useFakeStore)
            {
                var module = StandardPurchasingModule.Instance();
                module.useFakeStoreAlways = true;
                if (_useFakeStoreUIMode) module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

                builder = ConfigurationBuilder.Instance(module);
            }
            else
            {
                builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            }

            foreach (var productId in Database.GameSettings.InAppPurchases.Consumables.Keys)
            {
                builder.AddProduct(productId, ProductType.Consumable);
            }

            builder.AddProduct(Database.GameSettings.InAppPurchases.SubscriptionNoAds, ProductType.Subscription);
            builder.AddProduct(Database.GameSettings.InAppPurchases.Bundle_1.ID , ProductType.Consumable);
            builder.AddProduct(Database.GameSettings.InAppPurchases.Bundle_2.ID , ProductType.Consumable);
            Debug.Log($"Shay : {Database.GameSettings.InAppPurchases.Bundle_1.ID}");
            Debug.Log($"Shay : {Database.GameSettings.InAppPurchases.Bundle_2.ID}");
            UnityPurchasing.Initialize(this, builder);

            
            return _iapInitializationTCS.Task; // Will await until OnInitialized() is called
        }


        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            m_StoreController = controller;

#if UNITY_IOS
            extensionProvider = extensions;
#endif
            if (!_useFakeStore)
            {
#if !UNITY_EDITOR
    InitializeValidator();
#endif

            }

            EventService.IAP.OnIAPInitialized.Invoke(this);

            _iapInitializationTCS?.TrySetResult(true); // Complete the Task
            
            
        }

        void InitializeValidator()
        {
#if UNITY_ANDROID
            m_Validator = new CrossPlatformValidator(GooglePlayTangle.Data(), null, Application.identifier);
            return;
#elif UNITY_IOS
        m_Validator = new CrossPlatformValidator(null, AppleTangle.Data(), Application.identifier);
        return;
#else
        Debug.LogError($"The cross-platform validator is not implemented for the currently selected store: {StandardPurchasingModule.Instance().appStore}. \n" +
                             "Build the project for Android, iOS, macOS, or tvOS and use the Google Play Store or Apple App Store.");   
#endif
        }

        public bool IsSubsribed(string subscriptionId)
        {
            if (_mockSubsription) return true;

            var subProduct = m_StoreController.products.WithID(subscriptionId);
            if (subProduct != null)
            {
                try
                {
                    if (subProduct.hasReceipt)
                    {
                        var subManager = new SubscriptionManager(subProduct, null);
                        var info = subManager.getSubscriptionInfo(); // info holds all info about the subscription including period
                        Debug.Log(info.getExpireDate(), this);

                        if (info.isSubscribed() == Result.True)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else Debug.Log("Subscription Reciept not found", this);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Store not detected, check if store is fake", this);
                }
            }
            else Debug.LogWarning("Missing Subscription product.", this);

            return false;
        }

        private void HandleOnPurchaseBtn(Component c, string productId)
        {
            m_StoreController.InitiatePurchase(productId);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log("Processing Purchase..", this);

            // Parsing receipt to ensure receipt, store and payload data exists
            var productId = args.purchasedProduct.definition.id;
            var currencyCode = args.purchasedProduct.metadata.isoCurrencyCode;
            var price = args.purchasedProduct.metadata.localizedPrice;
            double lPrice = decimal.ToDouble(price);

            var receiptData = Json.Deserialize(args.purchasedProduct.receipt) as Dictionary<string, object>;
            if (receiptData == null)
            {
                Debug.LogWarning("Receipt data is null", this);
                return PurchaseProcessingResult.Complete;
            }

            var store = receiptData["Store"] as string;
            var payload = receiptData["Payload"] as string; // Raw JSON receipt for Android, base64 encoded ASN.1 receipt for Apple.

            if (store == null || payload == null)
            {
                Debug.LogWarning("Store or Payload is null in receipt data", this);
                return PurchaseProcessingResult.Complete;
            }

            // VALIDATE RECEIPT
#if !UNITY_EDITOR
        bool isPurchaseValid = IsPurchaseValid(args.purchasedProduct);

        if (!isPurchaseValid)
        {
            return PurchaseProcessingResult.Complete;
        }
#endif

            // SEND TENJIN TRANSACTION EVENT
#if UNITY_ANDROID
            Debug.Log("Sending ANDROID Tenjin transaction Event", this);

            var googleReceipt = Json.Deserialize(payload) as Dictionary<string, object>;
            if (googleReceipt != null && googleReceipt.TryGetValue("json", out var googleJson) && googleReceipt.TryGetValue("signature", out var googleSignature))
            {
                TenjinService.Instance.SendAndroidTransactionEvent(productId, currencyCode, 1, lPrice, (string)googleJson, (string)googleSignature);
            }

#elif UNITY_IOS
            Debug.Log("Sending IOS Tenjin transaction Event..", this);

            var transactionId = args.purchasedProduct.transactionID;

            TenjinService.Instance.SendIosTransactionEvent(productId, currencyCode, 1, lPrice, transactionId, payload);
#endif

            UnlockContent(args.purchasedProduct);

            //We return Complete, informing Unity IAP that the processing on our side is done and the transaction can be closed.
            return PurchaseProcessingResult.Complete;
        }

        bool IsPurchaseValid(Product product)
        {
            if (m_Validator != null)
            {
                try
                {
                    var validationResults = m_Validator.Validate(product.receipt);
                    foreach (var result in validationResults)
                    {
                        if (result is GooglePlayReceipt googleReceipt)
                        {
                            // You can add additional checks here based on the receipt content
                            LogReceipts(validationResults);
                            return true;
                        }
                    }
                }
                catch (IAPSecurityException)
                {
                    Debug.LogWarning("Invalid receipt, not unlocking content", this);
                    return false;
                }
            }
            return true;
        }

        private async void UnlockContent(Product product)
        {
            Debug.Log($"Unlock Content: {product.definition.id}");

            if(product.definition.id == Database.GameSettings.InAppPurchases.SubscriptionNoAds)
            {
                EventService.IAP.HandlePurchasedSubscriptionNoAds(this);
                TenjinService.Instance.SendCompletedInAppPurchaseEvent(product, 1);
                return;
            }

            int rewardAmount = Database.GameSettings.InAppPurchases.ConsumableRewardById(product.definition.id);

            var saveData = Database.UserData.Copy();
            saveData.Stats.TotalCurrency += rewardAmount;

            await Database.Instance.Save(saveData);

            EventService.IAP.HandlePurchasedConsumable(this);

            TenjinService.Instance.SendCompletedInAppPurchaseEvent(product, rewardAmount);
        }

        public (string productName, string localizedPrice) ProductInfo(string productId)
        {
            Product p = m_StoreController.products.WithID(productId);

            string productName = p.metadata.localizedTitle;
            int parenthesisIndex = productName.IndexOf('(');
            if (parenthesisIndex != -1)
            {
                productName = productName.Substring(0, parenthesisIndex).Trim();
            }
            decimal price = p.metadata.localizedPrice;
            string code = p.metadata.isoCurrencyCode;
            string localizedPrice = price + " " + code;

            return (productName, localizedPrice);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogWarning($"Purchasing failed to initialize. Reason: {error}.");
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            var errorMessage = $"Purchasing failed to initialize. Reason: {error}.";

            if (message != null)
            {
                errorMessage += $" More details: {message}";
            }

            Debug.LogWarning(errorMessage);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogWarning($"Purchase failed - Product: '{product.definition.id}'," +
                $" Purchase failure reason: {failureDescription.reason}," +
                $" Purchase failure details: {failureDescription.message}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"Purchase failed - Product: '{product.definition.id}', PurchaseFailureReason: {failureReason}");
        }

        void LogReceipts(IEnumerable<IPurchaseReceipt> receipts)
        {
            Debug.Log("Receipt is valid. Contents:");

            foreach (var receipt in receipts)
            {
                string logMessage = $"Product ID: {receipt.productID}\n" +
            $"Purchase Date: {receipt.purchaseDate}\n" +
            $"Transaction ID: {receipt.transactionID}\n";

                if (receipt is GooglePlayReceipt googleReceipt)
                {
                    logMessage += $"Purchase State: {googleReceipt.purchaseState}\n" +
                                  $"Purchase Token: {googleReceipt.purchaseToken}\n";
                }

                if (receipt is AppleInAppPurchaseReceipt appleReceipt)
                {
                    logMessage += $"Original Transaction ID: {appleReceipt.originalTransactionIdentifier}\n" +
                                  $"Subscription Expiration Date: {appleReceipt.subscriptionExpirationDate}\n" +
                                  $"Cancellation Date: {appleReceipt.cancellationDate}\n" +
                                  $"Quantity: {appleReceipt.quantity}\n";
                }

                Debug.Log($"{logMessage}\n-----------------\n");
            }
        }

        public void HandleOnRestore(Component c)
        {
#if UNITY_IOS
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                
                var apple = extensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result) =>
                {
                    Debug.Log("RestorePurchases completed: " + result);
                    // Optional: Notify user of result
                });
            }
            else
            {
                Debug.LogWarning("RestorePurchases is not supported on this platform.");
            }
#endif
        }


    }


}