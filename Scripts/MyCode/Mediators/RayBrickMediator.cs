using TMPro;
using UnityEngine;

public class RayBrickMediator : MonoBehaviour
{
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
        public TextMeshProUGUI Currency;
        public BoosterItem ClearRow;
        public BoosterItem ClearColumn;
        public BoosterItem ClearSquare;
    }

    [Header("Booster Shop")]
    public BoosterShopElements Shop = new BoosterShopElements();
}
