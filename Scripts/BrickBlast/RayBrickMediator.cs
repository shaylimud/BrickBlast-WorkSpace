using Ray.Controllers;
using Ray.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Enums;
using System.Collections.Generic;

    public class RayBrickMediator : MonoBehaviour
    {

        public static RayBrickMediator Instance { get; private set; }
        Button reviveButton;

        Button winCollectButton;
        Button winTripleButton;
        TextMeshProUGUI winCurrencyText;
        private bool winRewardGranted;

        public FailedTimed failedTimedPopup;
        public FailedTimed FailedTimedPopup => failedTimedPopup;

        [SerializeField] private Win winPopup;
        public Win WinPopup => winPopup;

        [SerializeField] private Sprite boosterAdSprite;

        // Reference to an external mediation component assigned in the inspector


        private readonly Dictionary<GameObject, Sprite> boosterOriginalSprites = new();
        private float boosterRefreshTimer;

        
        [Header("Canvas")]
        [SerializeField] private GameObject LevelProgressCanvas;

        [SerializeField] private GameObject BoosterCanvas;

        [Header("LevelProgress")]
        [SerializeField] private Image fill;

        [SerializeField] private GameObject progStar_full;
        [SerializeField] private GameObject progStar_empty;

        [SerializeField] private RectTransform star1;
        [SerializeField] private RectTransform star2;


        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            EventManager.OnGameStateChanged += HandleGameStateChange;
        }

        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChange;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            EventService.Resource.OnMenuResourceChanged -= RefreshShop;
        }

        private void Start()
        {
            if (Shop.BtnOpen != null)
                Shop.BtnOpen.GetComponent<Button>().onClick.AddListener(OpenShop);

            if (Shop.BtnClose != null)
                Shop.BtnClose.GetComponent<Button>().onClick.AddListener(CloseShop);

            SetupBoosterButton(Shop.ClearRow, BoosterType.ClearRow);
            SetupBoosterButton(Shop.ClearColumn, BoosterType.ClearColumn);
            SetupBoosterButton(Shop.ClearSquare, BoosterType.ClearSquare);
            SetupBoosterButton(Shop.ChangeShape, BoosterType.ChangeShape);

            SetupUseBoosterButton(Level.ClearRow, BoosterType.ClearRow);
            SetupUseBoosterButton(Level.ClearColumn, BoosterType.ClearColumn);
            SetupUseBoosterButton(Level.ClearSquare, BoosterType.ClearSquare);
            SetupUseBoosterButton(Level.ChangeShape, BoosterType.ChangeShape);

            EventService.Resource.OnMenuResourceChanged += RefreshShop;
            RefreshShop(this);

            if (Shop.Panel != null)
                Shop.Panel.SetActive(false);

            HandleGameStateChange(EventManager.GameStatus);
        }

        private void Update()
        {
            boosterRefreshTimer -= Time.deltaTime;
            if (boosterRefreshTimer <= 0f)
            {
                boosterRefreshTimer = 1f;
                RefreshShop(this);
            }
        }

        private void HandleGameStateChange(EGameState state)
        {

            bool playing = state == EGameState.Playing;
            bool inAdventure = GameDataManager.GetGameMode() == EGameMode.Adventure;

            bool show = playing && inAdventure;

            BoosterCanvas?.SetActive(show);
            LevelProgressCanvas?.SetActive(show);

            if (show)
            {
                UpdateLevelProgress();
            }
        }

        private void UpdateLevelProgress()
        {
            int subLevel = GameDataManager.GetSubLevelIndex();

            if (fill != null)
            {
                float fillAmount = subLevel switch
                {
                    2 => 0.5f,
                    >= 3 => 1f,
                    _ => 0f
                };
                fill.fillAmount = fillAmount;
            }

            UpdateStar(star1, subLevel >= 2);
            UpdateStar(star2, subLevel >= 3);
        }


        private void UpdateStar(Transform parent, bool isFull)

        {
            if (parent == null)
                return;

            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }

            GameObject prefab = isFull ? progStar_full : progStar_empty;
            if (prefab != null)
            {

                Instantiate(prefab, parent, false);

            }
        }

        public void SetReviveButton(Button button)
        {
            if (reviveButton != null)
            {
                reviveButton.onClick.RemoveListener(OnReviveButtonClicked);
            }

            reviveButton = button;

            if (reviveButton != null)
            {
                reviveButton.onClick.RemoveListener(OnReviveButtonClicked);
                reviveButton.onClick.AddListener(OnReviveButtonClicked);
            }
        }

        private void OnReviveButtonClicked()
        {
            RewardedService.Instance.ShowRewarded(RewardedType.Revive);
        }

        public void SetWinButtons(Button collectButton, Button tripleButton, TextMeshProUGUI currencyText)
        {
            // Remove previous listeners if any
            if (winCollectButton != null)
                winCollectButton.onClick.RemoveListener(OnWinCollectClicked);
            if (winTripleButton != null)
                winTripleButton.onClick.RemoveListener(OnWinTripleClicked);

            winCollectButton = collectButton;
            winTripleButton = tripleButton;
            winCurrencyText = currencyText;
            winRewardGranted = false;

            if (winCurrencyText != null)
                winCurrencyText.text = GameManager.instance.Score.ToString();

            if (winCollectButton != null)
                winCollectButton.onClick.AddListener(OnWinCollectClicked);

            if (winTripleButton != null)
            {
                winTripleButton.onClick.AddListener(OnWinTripleClicked);
                winTripleButton.interactable = RewardedService.Instance.IsRewardedReady(RewardedType.Triple);
            }
        }

        private async void OnWinCollectClicked()
        {
            if (winRewardGranted) return;
            winRewardGranted = true;

            var popup = MenuManager.instance.GetLastPopup() as Popup;
            popup?.StopInteration();

            await Database.UserData.AddScoreAsCurrency(GameManager.instance.Score);
            GameManager.instance.NextLevel();
            popup?.Close();
        }

        private void OnWinTripleClicked()
        {
            if (winRewardGranted) return;
            if (!RewardedService.Instance.IsRewardedReady(RewardedType.Triple)) return;

            winTripleButton.interactable = false;
            RewardedService.Instance.ShowRewarded(RewardedType.Triple, OnWinTripleRewarded);
        }

        private async void OnWinTripleRewarded()
        {
            if (winRewardGranted) return;
            winRewardGranted = true;

            var popup = MenuManager.instance.GetLastPopup() as Popup;
            popup?.StopInteration();

            await Database.UserData.AddScoreAsCurrency(GameManager.instance.Score * 3);
            GameManager.instance.NextLevel();
            popup?.Close();
        }


        [System.Serializable]
        public class BoosterItem
        {
            public TextMeshProUGUI Amount;
            public TextMeshProUGUI Cost;
            public GameObject BtnPurchase;
            public int Price;
        }

        [System.Serializable]
        public class BoosterShopElements
        {

            public GameObject Panel;
            public GameObject BtnOpen;
            public GameObject BtnClose;

            public TextMeshProUGUI Currency;
            public BoosterItem ClearRow;
            public BoosterItem ClearColumn;
            public BoosterItem ClearSquare;
            public BoosterItem ChangeShape;
        }

        [Header("Booster Shop")] public BoosterShopElements Shop = new BoosterShopElements();

        [System.Serializable]
        public class BoosterLevelElements
        {
            public GameObject ClearRow;
            public GameObject ClearColumn;
            public GameObject ClearSquare;
            public GameObject ChangeShape;
        }

        [Header("Level Boosters")] public BoosterLevelElements Level = new BoosterLevelElements();


        private void OpenShop()
        {
            if (Shop.Panel != null)
                Shop.Panel.SetActive(true);
            RefreshShop(this);
            UIController.Instance.RefreshShop(this);
        }

        private void CloseShop()
        {
            if (Shop.Panel != null)
                Shop.Panel.SetActive(false);
        }

        private void SetupBoosterButton(BoosterItem item, BoosterType type)
        {
            if (item?.BtnPurchase == null) return;

            var button = item.BtnPurchase.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                EventService.UI.OnBoosterPurchaseBtn?.Invoke(this, type, item.Price);
            });
        }

        private void SetupUseBoosterButton(GameObject obj, BoosterType type)
        {
            if (obj == null) return;

            var button = obj.GetComponent<Button>();
            var image = obj.GetComponent<Image>();
            if (button == null || image == null) return;

            boosterOriginalSprites[obj] = image.sprite;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                int count = 0;
                switch (type)
                {
                    case BoosterType.ClearRow:
                        count = Database.UserData.Stats.Power_1;
                        break;
                    case BoosterType.ClearColumn:
                        count = Database.UserData.Stats.Power_2;
                        break;
                    case BoosterType.ClearSquare:
                        count = Database.UserData.Stats.Power_3;
                        break;
                    case BoosterType.ChangeShape:
                        count = Database.UserData.Stats.Power_4;
                        break;
                }

                if (count <= 0)
                {
                    if (RewardedService.Instance.IsRewardedReady(RewardedType.ExtraSpace))
                    {
                        image.sprite = boosterAdSprite;
                        RewardedService.Instance.ShowRewarded(RewardedType.ExtraSpace, () =>
                        {
                            image.sprite = boosterOriginalSprites[obj];
                            ResourceService.Instance?.RewardBooster(type);
                            RefreshShop(this);
                        });
                    }
                    else
                    {
                        EventService.UI.OnToggleInsufficient?.Invoke(this);
                    }
                    return;
                }

                BlockPuzzleGameToolkit.Scripts.Gameplay.BoosterManager.Instance?.SelectBooster(type);

                BoosterManager.Instance?.SelectBooster(type);

            });
        }

        public void RefreshShop(Component c)
        {
            RefreshBoosterItem(Shop.ClearRow, Database.UserData.Stats.Power_1, Level.ClearRow);
            RefreshBoosterItem(Shop.ClearColumn, Database.UserData.Stats.Power_2, Level.ClearColumn);
            RefreshBoosterItem(Shop.ClearSquare, Database.UserData.Stats.Power_3, Level.ClearSquare);
            RefreshBoosterItem(Shop.ChangeShape, Database.UserData.Stats.Power_4, Level.ChangeShape);
            Shop.Currency.text = Database.UserData.Stats.TotalCurrency.ToString();
        }

        private void RefreshBoosterItem(BoosterItem item, int amount, GameObject levelButton)
        {
            if (item == null) return;

            item.Price = CalculateBoosterPrice(amount);

            if (item.Amount != null)
                item.Amount.text = amount.ToString();

            if (item.Cost != null)
                item.Cost.text = item.Price.ToString();

            if (item.BtnPurchase != null)
            {
                var button = item.BtnPurchase.GetComponent<Button>();
                button.interactable = Database.UserData.Stats.TotalCurrency >= item.Price && amount < 99;
            }

            if (levelButton != null)
            {
                var button = levelButton.GetComponent<Button>();
                var image = levelButton.GetComponent<Image>();
                bool adReady = RewardedService.Instance.IsRewardedReady(RewardedType.ExtraSpace);

                if (button != null)
                    button.interactable = amount > 0 || adReady;

                if (image != null && boosterOriginalSprites.TryGetValue(levelButton, out var originalSprite))
                    image.sprite = amount <= 0 && adReady ? boosterAdSprite : originalSprite;
            }

            Shop.ClearRow.Price = CalculateBoosterPrice(Database.UserData.Stats.Power_1);
            Shop.ClearColumn.Price = CalculateBoosterPrice(Database.UserData.Stats.Power_2);
            Shop.ClearSquare.Price = CalculateBoosterPrice(Database.UserData.Stats.Power_3);
            Shop.ChangeShape.Price = CalculateBoosterPrice(Database.UserData.Stats.Power_4);
        }

        private int CalculateBoosterPrice(int amount)
        {
            return Mathf.FloorToInt(100f * Mathf.Pow(1.17f, amount));
        }

    }

