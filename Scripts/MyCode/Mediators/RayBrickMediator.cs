using Ray.Controllers;
using TMPro;
using UnityEngine;

using UnityEngine.UI;

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
        }


        private void Start()
        {
            if (Shop.BtnOpen != null)
                Shop.BtnOpen.GetComponent<Button>().onClick.AddListener(OpenShop);

            if (Shop.BtnClose != null)
                Shop.BtnClose.GetComponent<Button>().onClick.AddListener(CloseShop);

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

    }

