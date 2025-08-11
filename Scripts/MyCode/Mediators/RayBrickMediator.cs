using Ray.Controllers;
using Ray.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BlockPuzzleGameToolkit.Scripts.Gameplay;

public class RayBrickMediator : MonoBehaviour
{

        public static RayBrickMediator Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            EventService.Resource.OnMenuResourceChanged -= UpdatePrices;
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

            SetupUseBoosterButton(Level.ClearRow, BoosterType.ClearRow);
            SetupUseBoosterButton(Level.ClearColumn, BoosterType.ClearColumn);
            SetupUseBoosterButton(Level.ClearSquare, BoosterType.ClearSquare);

            EventService.Resource.OnMenuResourceChanged += UpdatePrices;
            UpdatePrices(this);

            if (Shop.Panel != null)
                Shop.Panel.SetActive(false);
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
        }

        [Header("Booster Shop")] public BoosterShopElements Shop = new BoosterShopElements();

        [System.Serializable]
        public class BoosterLevelElements
        {
            public GameObject ClearRow;
            public GameObject ClearColumn;
            public GameObject ClearSquare;
        }

        [Header("Level Boosters")] public BoosterLevelElements Level = new BoosterLevelElements();


        private void OpenShop()
        {
            if (Shop.Panel != null)
                Shop.Panel.SetActive(true);
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
            if (button == null) return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                BoosterManager.Instance?.SelectBooster(type);
            });
        }

        private void UpdatePrices(Component c)
        {
            
            UpdateBoosterItem(Shop.ClearRow, Database.UserData.Stats.Power_1);
            UpdateBoosterItem(Shop.ClearColumn, Database.UserData.Stats.Power_2);
            UpdateBoosterItem(Shop.ClearSquare, Database.UserData.Stats.Power_3);
            Shop.Currency.text = Database.UserData.Stats.TotalCurrency.ToString();
        }

        private void UpdateBoosterItem(BoosterItem item, int amount)
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

            Shop.ClearRow.Price = CalculateBoosterPrice(Database.UserData.Stats.Power_1);
            Shop.ClearColumn.Price = CalculateBoosterPrice(Database.UserData.Stats.Power_2);
            Shop.ClearSquare.Price = CalculateBoosterPrice(Database.UserData.Stats.Power_3);
        }

        private int CalculateBoosterPrice(int amount)
        {
            return Mathf.FloorToInt(100f * Mathf.Pow(1.17f, amount));
        }

    }

