using Ray.Services;
using Ray.Views;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Ray.Features;

namespace Ray.Controllers
{
    public class UIController : MonoBehaviour
    {
        public static UIController Instance { get; private set; }

        [Header("Configs")]
        [SerializeField, RequireReference] private UIElementMediator _element;

        [Header("References")]
        [SerializeField, RequireReference] UIView _view;

        private RayDebugService _rayDebug => ServiceAllocator.Instance.GetDebugService(ConfigType.Controllers);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keep UIController across scenes
        }

        private void Start()
        {
            var save = SaveSystem.Load();
            if (save.muteSounds) ToggleSoundSprite(this);
        }

        private void OnEnable()
        {
            // Application Start
            EventService.Application.OnRequestVersionUpdate += ShowUpdateApplication;

            // Menu
            EventService.Application.OnGameContentStart += HandleOnGameContentStart;
            EventService.Application.OnGameContentStart += RefreshCurrencies;
            EventService.Application.OnGameContentStart += TryOfferFreeGift;

            EventService.Resource.OnMenuResourceChanged += RefreshCurrencies;
            EventService.Resource.OnMenuResourceChanged += RefreshShop;

            EventService.UI.OnToggleSound += ToggleSoundSprite;
            EventService.UI.OnToggleTutorial += ToggleTutorial;
            EventService.UI.OnToggleInsufficient += ToggleInsufficient;
            EventService.Database.OnMismatchDetected += ToggleDataMismatch;
            EventService.UI.OnToggleDataMismatch += ToggleDataMismatch;
            EventService.UI.OnToggleShop += ToggleShop;
            EventService.UI.OnToggleShop += RefreshShop;

            EventService.UI.OnStartBtn += TryOfferNoEnemies;

            // Shop
            EventService.IAP.OnIAPInitialized += CreateShopUI;
            EventService.IAP.OnPurchasedSubscriptionNoAds += RefreshShop;
            EventService.IAP.OnPurchasedConsumable += RefreshCurrencies;

            // Level
            EventService.Level.OnStart += HandleLevelStart;
            EventService.Player.OnNewTrackedReach += RefreshLevelReach;
            EventService.Resource.OnLevelResourceChanged += RefreshLevel;
            EventService.IAP.OnRestoreBtn += OnRestoreButton;

            // Revive
            EventService.Player.OnHit += TryOfferRevive;
            EventService.Ad.OnReviveWatched += HandleOnReviveWatched;

            // End
            EventService.Player.OnParked += HandleLevelEnd;
            EventService.Ad.OnTripleWatched += HandleOnTripleWatched;
            EventService.Resource.OnEndCurrencyChanged += RefreshEnd;

            // Back to Menu
            EventService.UI.OnBackToMenu += HandleBackToMenu;
            EventService.UI.OnBackToMenu += RefreshCurrencies;

            // ADS
            EventService.Ad.OnWatchAdBtn += StopMeters;

            // Features
            EventService.UI.OnShowExtraSpace += ShowExtraSpace;
            EventService.Resource.OnExtraSpaceReceived += HideExtraSpace;

            //settings
            EventService.UI.OnSettingBtn += ToggleSettings;
            EventService.UI.OnPrivacyPolicy += OpenPrivacyPolicy;
            EventService.UI.OnTermsOfUse += OpenTermsOfUse;
            EventService.UI.OnBrdChanged += ToggleButtonBrd;
            EventService.UI.OnShowConsent += ShowBrdConsent;
            EventService.UI.OnLearnMore += OpenLearnMore;
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks
            EventService.Application.OnRequestVersionUpdate -= ShowUpdateApplication;

            EventService.Application.OnGameContentStart -= HandleOnGameContentStart;
            EventService.Application.OnGameContentStart -= RefreshCurrencies;
            EventService.Application.OnGameContentStart -= TryOfferFreeGift;

            EventService.Resource.OnMenuResourceChanged -= RefreshCurrencies;
            EventService.Resource.OnMenuResourceChanged -= RefreshShop;

            EventService.UI.OnToggleSound -= ToggleSoundSprite;
            EventService.UI.OnToggleTutorial -= ToggleTutorial;
            EventService.UI.OnToggleInsufficient -= ToggleInsufficient;
            EventService.Database.OnMismatchDetected -= ToggleDataMismatch;
            EventService.UI.OnToggleDataMismatch -= ToggleDataMismatch;
            EventService.UI.OnToggleShop -= ToggleShop;
            EventService.UI.OnToggleShop -= RefreshShop;

            EventService.UI.OnStartBtn -= TryOfferNoEnemies;

            EventService.IAP.OnIAPInitialized -= CreateShopUI;
            EventService.IAP.OnPurchasedSubscriptionNoAds -= RefreshShop;
            EventService.IAP.OnPurchasedConsumable -= RefreshCurrencies;

            EventService.Level.OnStart -= HandleLevelStart;
            EventService.Player.OnNewTrackedReach -= RefreshLevelReach;
            EventService.Resource.OnLevelResourceChanged -= RefreshLevel;
            EventService.IAP.OnRestoreBtn -= OnRestoreButton;

            EventService.Player.OnHit -= TryOfferRevive;
            EventService.Ad.OnReviveWatched -= HandleOnReviveWatched;

            EventService.Player.OnParked -= HandleLevelEnd;
            EventService.Ad.OnTripleWatched -= HandleOnTripleWatched;
            EventService.Resource.OnEndCurrencyChanged -= RefreshEnd;

            EventService.UI.OnBackToMenu -= HandleBackToMenu;
            EventService.UI.OnBackToMenu -= RefreshCurrencies;

            EventService.Ad.OnWatchAdBtn -= StopMeters;

            EventService.UI.OnShowExtraSpace -= ShowExtraSpace;
            EventService.Resource.OnExtraSpaceReceived -= HideExtraSpace;

            EventService.UI.OnSettingBtn -= ToggleSettings;
            EventService.UI.OnPrivacyPolicy -= OpenPrivacyPolicy;
            EventService.UI.OnTermsOfUse -= OpenTermsOfUse;
            EventService.UI.OnBrdChanged -= ToggleButtonBrd;
            EventService.UI.OnShowConsent -= ShowBrdConsent;
            EventService.UI.OnLearnMore -= OpenLearnMore;
        }

        // Application Start
        private void ShowUpdateApplication(Component c)
        {
            _rayDebug.Event("ShowUpdateApplication", c, this);

            _view.Show(_element.Canvas.UpdateApplication);
        }
        private void HandleOnGameContentStart(Component c)
        {
            _rayDebug.Event("HandleOnGameContentStart", c, this);

            _view.Hide(_element.Canvas.Loading);
            _view.Show(_element.Canvas.Menu); ;
        }

        // Menu
        private void RefreshMenu(Component c)
        {
            _rayDebug.Event("RefreshMenu", c, this);

            _view.PulseCurrency(_element.Menu.MenuCurrency, Database.UserData.Stats.TotalCurrency);

            _view.SetText(_element.Menu.ReachLevel, Database.UserData.Stats.ReachLevel);
            _view.SetText(_element.Menu.SpaceLevel, Database.UserData.Stats.SpaceLevel);

            string costIcon = ResourceService.Instance.PanalizedUser() ? "<sprite=2>" : "<sprite=0>";

            _view.ShowHideViaStatus(Database.UserData.Security.Cheater, _element.Menu.IconCheater);

            _view.SetText(_element.Menu.ReachCost, $"{costIcon}{ResourceService.Instance.UpgradeCost(UpgradeType.Reach)}");
            _view.SetText(_element.Menu.SpaceCost, $"{costIcon}{ResourceService.Instance.UpgradeCost(UpgradeType.Space)}");
        }
        private void ToggleSoundSprite(Component c)
        {
#if UNITY_IOS
            bool status = _element.Menu.ImageSoundsIos.sprite == _element.Menu.SpriteSoundOn ? false : true;

                        if (status) _element.Menu.ImageSoundsIos.sprite = _element.Menu.SpriteSoundOn;
            else _element.Menu.ImageSoundsIos.sprite = _element.Menu.SpriteSoundOff;
#else
            bool status = _element.Menu.ImageSoundsAndroid.sprite == _element.Menu.SpriteSoundOn ? false : true;

            if (status) _element.Menu.ImageSoundsAndroid.sprite = _element.Menu.SpriteSoundOn;
            else _element.Menu.ImageSoundsAndroid.sprite = _element.Menu.SpriteSoundOff;
#endif
        }
        private void ToggleTutorial(Component c)
        {
            _rayDebug.Event("ToggleTutorial", c, this);

#if UNITY_IOS
            _view.ToggleOnTop(_element.Canvas.Tutorial, _element.Canvas.SettingiOS, true);
#else
            _view.ToggleOnTop(_element.Canvas.Tutorial, _element.Canvas.SettingsAndroid, true);
#endif
        }

        private void ToggleSettings(Component c)
        {
            _rayDebug.Event("ToggleSettings", c, this);

#if UNITY_IOS
            _view.ToggleOnTop(_element.Canvas.SettingiOS , _element.Canvas.Menu, true);
#else
            _view.ToggleOnTop(_element.Canvas.SettingsAndroid, _element.Canvas.Menu, true);
#endif
        }
        private void ToggleInsufficient(Component c)
        {
            _rayDebug.Event("ToggleInsufficient", c, this);

            _view.ToggleOnTop(_element.Canvas.Insufficient, _element.Canvas.Menu, true);
        }
        private void ToggleDataMismatch(Component c)
        {
            _rayDebug.Event("ToggleDataMismatch", c, this);

            _view.ToggleOnTop(_element.Canvas.DataMismatch, _element.Canvas.Menu, true);
        }
        private void ToggleShop(Component c)
        {
            _rayDebug.Event("ToggleShop", c, this);

            _view.ToggleOnTop(_element.Canvas.Shop, _element.Canvas.Menu, true);
        }
        private void RefreshCurrencies(Component c)
        {
            RefreshMenu(c);
            RefreshShop(c);
        }

        // Shop
        private void CreateShopUI(Component c)
        {
            _rayDebug.Event("CreateShopUI", c, this);

            _view.Show(_element.Menu.BtnShop);

            CreateNoAdsSubscriptionUI();
            CreateConsumablesUI();

            RefreshShop(this);
        }
        private void CreateNoAdsSubscriptionUI()
        {
            _element.Shop.CtnrSubscriptionNoAds = Instantiate(_element.Shop.PrefabSubscriptionNoAds, _element.Shop.panelProducts.transform);
            var noAdsInfo = IAPService.Instance.ProductInfo(Database.GameSettings.InAppPurchases.SubscriptionNoAds);

            SetupPurchaseButton(
                container: _element.Shop.CtnrSubscriptionNoAds,
                costTextPath: "Btn - Purchase/Value - Cost",
                purchaseButtonPath: "Btn - Purchase",
                price: noAdsInfo.localizedPrice,
                onClick: () => EventService.UI.OnIAPPurchaseBtn(this, Database.GameSettings.InAppPurchases.SubscriptionNoAds)
            );
        }
        private void CreateConsumablesUI()
        {
            var sortedConsumables = Database.GameSettings.InAppPurchases.Consumables
                .OrderByDescending(consumable => Database.GameSettings.InAppPurchases.ConsumableRewardById(consumable.Key))
                .ToList();

            foreach (var consumable in sortedConsumables)
            {
                var consumableContainer = Instantiate(_element.Shop.PrefabConsumable, _element.Shop.panelProducts.transform);
                var consumableInfo = IAPService.Instance.ProductInfo(consumable.Key);
                var rewardAmount = Database.GameSettings.InAppPurchases.ConsumableRewardById(consumable.Key).ToString("N0");

                SetupPurchaseButton(
                    container: consumableContainer,
                    costTextPath: "Btn - Purchase/Value - Cost",
                    purchaseButtonPath: "Btn - Purchase",
                    price: consumableInfo.localizedPrice,
                    onClick: () => EventService.UI.OnIAPPurchaseBtn(this, consumable.Key)
                );

                SetText(consumableContainer, "Value - Reward", rewardAmount);
            }
        }
        private void SetupPurchaseButton(GameObject container, string costTextPath, string purchaseButtonPath, string price, UnityAction onClick)
        {
            SetText(container, costTextPath, price);

            Button btnPurchase = container.transform.Find(purchaseButtonPath).GetComponent<Button>();
            btnPurchase.onClick.RemoveAllListeners();
            btnPurchase.onClick.AddListener(onClick);
        }
        private void SetText(GameObject container, string textPath, string value)
        {
            var textComponent = container.transform.Find(textPath)?.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                _view.SetText(textComponent, value);
            }
            else
            {
                Debug.LogWarning($"Text path '{textPath}' not found in {container.name}");
            }
        }
        public  void RefreshShop(Component c)
        {
            _rayDebug.Event("RefreshShop", c, this);

            _view.PulseCurrency(_element.Shop.ShopCurrency, Database.UserData.Stats.TotalCurrency);

         /*   if (IAPService.Instance.IsSubsribed(Database.GameSettings.InAppPurchases.SubscriptionNoAds))
            {
                _view.Hide(_element.Shop.CtnrSubscriptionNoAds);
            }
            else _view.Show(_element.Shop.CtnrSubscriptionNoAds);*/
        }

        // Features
        private void TryOfferFreeGift(Component c)
        {
            _rayDebug.Event("OfferDailyGift", c, this);

            FreeGiftAvailabilityLoop();
        }
        private async void FreeGiftAvailabilityLoop()
        {
            while (true)
            {
                bool isAdReady = RewardedService.Instance.IsRewardedReady(RewardedType.FreeGift);
                var (canClaim, cooldown) = await ResourceService.Instance.CanClaimFreeGift();

                if (isAdReady && canClaim)
                {
                    _view.SetText(_element.Menu.FreeGiftCooldown, "READY!");
                    _view.ButtonInteractableState(true, _element.Menu.BtnWatchFreeGift);
                }
                else
                {
                    _view.SetText(_element.Menu.FreeGiftCooldown, cooldown);
                    _view.ButtonInteractableState(false, _element.Menu.BtnWatchFreeGift);
                }

                await Task.Delay(1000);
            }
        }
        private void TryOfferNoEnemies(Component c)
        {
            _rayDebug.Event("TryShowNoEnemies", c, this);

            if (RewardedService.Instance.IsRewardedReady(RewardedType.NoEnemies)
                && ResourceService.Instance.LevelsPlayed.Value % Database.GameSettings.Advertising.Freqs.NoEnemiesFreq == 0
                && ResourceService.Instance.LevelsPlayed.Value > 0
                && Database.GameSettings.Advertising.Freqs.NoEnemiesFreq != 0)
            {
                _view.Show(_element.Canvas.NoEnemies);

                StartCoroutine(_view.DepleteMeter(_element.Feature.NoEnemiesMeter, 3f, () =>
                {
                    EventService.Ad.OnNoEnemiesDismissed?.Invoke(this);
                    _view.FadeOff(_element.Canvas.NoEnemies);
                }));
            }
            else EventService.Ad.OnNoEnemiesDismissed?.Invoke(this);
        }
        private void ShowExtraSpace(Component c)
        {
            _rayDebug.Event("ShowExtraSpace", c, this);

            _view.Show(_element.Canvas.ExtraSpace);

            _view.SetText(_element.Feature.ExtraSpaceReward, $"Get {ExtraSpaceFeature.Instance.ExtraSpaceReward.Value} More!");

            StartCoroutine(_view.DepleteMeter(_element.Feature.ExtraSpaceMeter, 3f, () =>
            {
                EventService.Ad.OnExtraSpaceDismissed?.Invoke(this);
                _view.FadeOff(_element.Canvas.ExtraSpace);
            }));
        }
        private void HideExtraSpace(Component c)
        {
            _rayDebug.Event("HideExtraSpace", c, this);

            _view.Hide(_element.Canvas.ExtraSpace);
        }

        // Level
        private void HandleLevelStart(Component c)
        {
            _rayDebug.Event("HandleOnStartBtn", c, this);

            _view.FadeOff(_element.Canvas.Menu);
            _view.Hide(_element.Canvas.NoEnemies);
            _view.Show(_element.Canvas.Level);
        }
        private void RefreshLevel(Component c)
        {
            _rayDebug.Event("RefreshLevel", c, this);

            _view.SetText(_element.Level.LevelCurrency, ResourceService.Instance.LevelCurrency.Value);
            _view.SetText(_element.Level.LevelSpace, ResourceService.Instance.LevelSpace.Value);
        }
        public void RefreshLevelReach(Component c, int currentReach)
        {
            _view.SetText(_element.Level.LevelReach, -currentReach);
        }
        private void TryOfferRevive(Component c)
        {
            _rayDebug.Event("TryOfferRevive", c, this);

            if (RewardedService.Instance.IsRewardedReady(RewardedType.Revive))
            {
                _view.Show(_element.Canvas.Revive);

                _view.SetText(_element.Revive.ReviveSpace, ResourceService.Instance.LevelSpace.Value);

                StartCoroutine(_view.DepleteMeter(_element.Revive.MeterRevive, 3f, () =>
                {
                    EventService.Ad.OnReviveDismissed?.Invoke(this);
                    _view.FadeOff(_element.Canvas.Revive);
                }));
            }
            else EventService.Ad.OnReviveDismissed?.Invoke(this);
        }
        private void HandleOnReviveWatched(Component c)
        {
            _rayDebug.Event("HandleOnReviveWatched", c, this);

            _view.FadeOff(_element.Canvas.Revive);
        }

        // End
        private void RefreshEnd(Component c)
        {
            _rayDebug.Event("RefreshEnd", c, this);

            _view.PulseCurrency(_element.End.EndCurrency, ResourceService.Instance.LevelCurrency.Value);
        }
        private void HandleLevelEnd(Component c)
        {
            _rayDebug.Event("TryOfferTriple", c, this);

            _view.Show(_element.Canvas.End);
            RefreshEnd(this);

            if (ResourceService.Instance.LevelSpace.Value == 0)
            {
                _view.Show(_element.End.CtnrFullSpace);
                _view.Hide(_element.End.CtnrPartialSpace);
            }
            else
            {
                _view.Show(_element.End.CtnrPartialSpace);
                _view.Hide(_element.End.CtnrFullSpace);
            }

            if (RewardedService.Instance.IsRewardedReady(RewardedType.Triple)
                && ResourceService.Instance.LevelCurrency.Value > 0)
            {
                _view.Show(_element.End.BtnTriple);
                _view.Show(_element.End.InfoOfferTriple);

                EventService.UI.OnMeterStart.Invoke(this);

                StartCoroutine(_view.DepleteMeter(_element.End.MeterTriple, 3f, () =>
                {
                    _view.Hide(_element.End.BtnTriple);
                    EventService.Ad.OnTripleDismissed.Invoke(this);
                    EventService.UI.OnBackToMenu.Invoke(this);
                }));
            }
            else
            {
                _view.Hide(_element.End.BtnTriple);
                _view.Hide(_element.End.InfoOfferTriple);
                EventService.Ad.OnTripleDismissed.Invoke(this);

                StartCoroutine(_view.StandBy(1f, () =>
                {
                    EventService.UI.OnBackToMenu.Invoke(this);
                }));
            }
        }
        private void HandleOnTripleWatched(Component c)
        {
            _rayDebug.Event("HandleOnTripleWatched", c, this);

            _view.Hide(_element.End.BtnTriple);
            _view.Hide(_element.End.InfoOfferTriple);

            EventService.UI.OnMeterStart.Invoke(this);

            StartCoroutine(_view.StandBy(2, () =>
            {
                EventService.UI.OnBackToMenu.Invoke(this);
            }));
        }
        private void HandleBackToMenu(Component c)
        {
            _rayDebug.Event("HandleBackToMenu", c, this);

            _view.Hide(_element.Canvas.Level);
            _view.FadeOff(_element.Canvas.End);
            _view.Show(_element.Canvas.Menu);
        }

        // AD
        private void StopMeters(Component c)
        {
            _rayDebug.Event("StopMeters", c, this);

            StopAllCoroutines();
        }

        private void OpenPrivacyPolicy(Component c)
        {
            _rayDebug.Event("OpenPrivacyPolicy", c, this);
            Application.OpenURL("https://www.raymobile.games/ff-privacy-policy");//Fix This To The currect One!!!!
        }
        private void OpenTermsOfUse(Component c)
        {
            _rayDebug.Event("OpenTermsOfUser", c, this);
            Application.OpenURL("https://www.apple.com/legal/internet-services/itunes/dev/stdeula/");
        }
        private void OpenLearnMore(Component c)
        {
            _rayDebug.Event("OpenLearnMore", c, this);
            Application.OpenURL("https://bright-sdk.com/users#learn-more-about-bright-sdk-web-indexing");
        }
        private void OnRestoreButton(Component c)
        {
            _rayDebug.Event("OpenRestoreButton", c, this);
            IAPService.Instance.HandleOnRestore(this);
        }

        private void ToggleButtonBrd(Component c , bool status)
        {
            if (status)
            {
                _element.Menu.WebIndexOff.SetActive(false);
                _element.Menu.WebIndexOn.SetActive(true);
            }
            else
            {
                _element.Menu.WebIndexOff.SetActive(true);
                _element.Menu.WebIndexOn.SetActive(false);

            }
        }

        private void ShowBrdConsent(Component c)
        {
            if (!Database.GameSettings.BrightData.Enable) return;
        }
    }
}