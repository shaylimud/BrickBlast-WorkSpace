using UnityEngine;
using UnityEngine.Events;

namespace Ray.Services
{
    public class EventService : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public static readonly ApplicationEvent Application = new ApplicationEvent();
        public static readonly UIEvent UI = new UIEvent();
        public static readonly LevelEvent Level = new LevelEvent();
        public static readonly ItemEvent Item = new ItemEvent();
        public static readonly PlayerEvent Player = new PlayerEvent();
        public static readonly ResourceEvent Resource = new ResourceEvent();
        public static readonly AdEvent Ad = new AdEvent();
        public static readonly DatabaseEvent Database = new DatabaseEvent();
        public static readonly IAPEvent IAP = new IAPEvent();
        public static readonly BrightDataEvent Brd = new BrightDataEvent();

        public class ApplicationEvent
        {
            public UnityAction<Component> OnRequestVersionUpdate;
            public UnityAction<Component> OnGameContentStart;
        }

        public class UIEvent
        {
            public UnityAction<Component> OnUpdateApplicationBtn;

            public UnityAction<Component> OnReachUpgradeBtn;
            public UnityAction<Component> OnSpaceUpgradeBtn;
            public UnityAction<Component> OnStartBtn;

            public UnityAction<Component> OnToggleSound;
            public UnityAction<Component> OnToggleTutorial;
            public UnityAction<Component> OnToggleInsufficient;
            public UnityAction<Component> OnToggleDataMismatch;
            public UnityAction<Component> OnToggleShop;

            public UnityAction<Component, BoosterType, int> OnBoosterPurchaseBtn;

            public UnityAction<Component, string> OnIAPPurchaseBtn;

            public UnityAction<Component, RewardedType> OnRewardedBtn;

            public UnityAction<Component> OnMeterStart;

            public UnityAction<Component> OnBackToMenu;


            public UnityAction<Component> OnSettingBtn;

            public UnityAction<Component> OnTermsOfUse;
            public UnityAction<Component> OnPrivacyPolicy;
            public UnityAction<Component> OnLearnMore;

            //Brd
            public UnityAction<Component , bool> OnBrdChanged;
            public UnityAction<Component> OnShowConsent;

            public UnityAction<Component> OnShowExtraSpace;
        }

        public class LevelEvent
        {
            public UnityAction<Component> OnStart;
            public UnityAction<Component> OnEnd;
        }

        public class SpawnTriggerEvent
        {
            public UnityAction<Component, int> OnSpawnTrigger;
        }

        public class ItemEvent
        {
            public UnityAction<Component, ItemType, Vector2> OnItemCollected;
            public UnityAction<Component> OnObstacleCollected;
        }

        public class PlayerEvent
        {
            public UnityAction<Component, int> OnNewTrackedReach;
            public UnityAction<Component> OnHit;
            public UnityAction<Component> OnRevived;
            public UnityAction<Component> OnOutOfLevel;
            public UnityAction<Component> OnParked;
        }

        public class ResourceEvent
        {
            public UnityAction<Component> OnMenuResourceChanged;

            public UnityAction<Component> OnNoEnemiesReceived;
            public UnityAction<Component> OnExtraSpaceReceived;

            public UnityAction<Component, ItemType, int, Vector2> OnCollectedItemValueProccessed;
            public UnityAction<Component> OnLevelResourceChanged;
            public UnityAction<Component> OnLevelReachChanged;
            public UnityAction<Component> OnSpaceLimitReached;

            public UnityAction<Component> OnEndCurrencyChanged;
        }

        public class AdEvent
        {
            public UnityAction<Component> OnApplovinInitialized;

            public UnityAction<Component> OnWatchAdBtn = delegate { };

            public UnityAction<Component> OnNoEnemiesWatched;
            public UnityAction<Component> OnReviveWatched;
            public UnityAction<Component> OnTripleWatched;
            public UnityAction<Component> OnPenaltyWatched;
            public UnityAction<Component> OnFreeGiftWatched;
            public UnityAction<Component> OnExtraSpaceWatched;

            public UnityAction<Component> OnNoEnemiesDismissed;
            public UnityAction<Component> OnReviveDismissed;
            public UnityAction<Component> OnTripleDismissed;
            public UnityAction<Component> OnExtraSpaceDismissed;
        }

        public class DatabaseEvent
        {
            public UnityAction<Component> OnMismatchDetected;
            public UnityAction<Component, string, int> OnNewHighestEvent;
        }

        public class IAPEvent
        {
            public UnityAction<Component> OnIAPInitialized;
            public UnityAction<Component> OnPurchasedSubscriptionNoAds;
            public UnityAction<Component> OnPurchasedConsumable;
            public UnityAction<Component> OnRestoreBtn;

        }

        public class BrightDataEvent
        {
            public UnityAction<Component> OnFirstTimeConsent;    
        }
    }
}