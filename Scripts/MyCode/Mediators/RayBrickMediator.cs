using TMPro;
using UnityEngine;

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
