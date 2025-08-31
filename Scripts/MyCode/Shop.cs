using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ray.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop instance;

    [Header("Shop-Items")] 
    [SerializeField] public GameObject bundle_1;
    [SerializeField] public GameObject bundle_2;
    public GameObject itemPrefab;
    public GameObject Special;

    [Header("Holders")] 
    [SerializeField] private GameObject itemHolder;

    [SerializeField] private RectTransform screensContent;
    private RectTransform[] screenRects;
    private Vector2[] screenPositions;
    
    private int currentScreenIndex = 0; // start on first screen
    
    [Header("Image")] 
    [SerializeField] private Image coinIcon;
    [SerializeField] private Image rowIcon;
    [SerializeField] private Image colIcon; 
    [SerializeField] private Image shapeIcon;
    [SerializeField] private Image squareIcon;
    
    
    [Header("Shop Screens")]
    [SerializeField] private RectTransform shopScreen;
    [SerializeField] private RectTransform shopScreen2;
    [SerializeField] private RectTransform shopScreen3;


    private readonly Vector2 offScreenPosition = new Vector2(1200f, -1200f);


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (screensContent == null)
        {
            Debug.LogWarning("screensContent is not assigned.");
            return;
        }

        screenRects = Enumerable.Range(0, screensContent.childCount)
            .Select(i => screensContent.GetChild(i) as RectTransform)
            .ToArray();

        // Cache the anchored positions of each screen so we can snap the
        // content to the correct spot when navigating between pages.
        screenPositions = screenRects
            .Select(rect => rect.anchoredPosition)
            .ToArray();

        MoveToScreen(currentScreenIndex);
    }

    public void MoveLeft()
    {
        if (currentScreenIndex <= 0)
        {
            return;
        }

        currentScreenIndex--;
        MoveToScreen(currentScreenIndex);
    }

    private void MoveToScreen(int index)
    {

        if (screensContent == null || screenPositions == null || index < 0 || index >= screenPositions.Length)
        {
            return;
        }

        var targetPos = screenPositions[index];
        screensContent.anchoredPosition = targetPos;

    }

    public void ShowShop()
    {
        SetActiveScreen(shopScreen);
    }

    public void ShowShop1()
    {
        SetActiveScreen(shopScreen2);
    }

    public void ShowShop2()
    {
        SetActiveScreen(shopScreen3);
    }

    private void SetActiveScreen(RectTransform active)
    {
        if (active == null)
        {
            return;
        }

        if (shopScreen != null)
        {
            shopScreen.anchoredPosition = shopScreen == active ? Vector2.zero : offScreenPosition;
        }

        if (shopScreen2 != null)
        {
            shopScreen2.anchoredPosition = shopScreen2 == active ? Vector2.zero : offScreenPosition;
        }

        if (shopScreen3 != null)
        {
            shopScreen3.anchoredPosition = shopScreen3 == active ? Vector2.zero : offScreenPosition;
        }
    }

    private IStoreController StoreController
    {
        get
        {
            var field = typeof(IAPService).GetField("m_StoreController", BindingFlags.NonPublic | BindingFlags.Static);
            return field?.GetValue(null) as IStoreController;
        }
    }

    private Product GetProduct(string productId)
    {
        return StoreController?.products.WithID(productId);
    }

    public string GetLocalizedPrice(string productId)
    {
        var info = IAPService.Instance.ProductInfo(productId);
        return info.localizedPrice;
    }

    public string GetProductName(string productId)
    {
        var info = IAPService.Instance.ProductInfo(productId);
        return info.productName;
    }

    public string GetProductDescription(string productId)
    {
        var product = GetProduct(productId);
        return product?.metadata.localizedDescription ?? string.Empty;
    }


    public ProductMetadata GetProductMetadata(string productId)
    {
        return GetProduct(productId)?.metadata;
    }

    private void SetText(GameObject root, string value, params string[] path)
    {
        Transform current = root.transform;
        foreach (var segment in path)
        {
            current = current?.Find(segment);
            if (current == null)
            {
                Debug.LogWarning($"Transform path not found: {string.Join("/", path)}");
                return;
            }
        }

        var textComponent = current.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = value;
        }
        else
        {
            Debug.LogWarning($"TextMeshProUGUI not found at path {string.Join("/", path)}");
        }
    }

    public async void BuildBasicItems()
    {
        var sortedConsumables = Database.GameSettings.InAppPurchases.Consumables
            .OrderBy(consumable => consumable.Value);

        foreach (var consumable in sortedConsumables)
        {
            string productId = consumable.Key;
            int reward = consumable.Value;
            Debug.Log($"Consumable: {productId} -> Reward: {reward}");

            GameObject basicItem = Instantiate(itemPrefab, itemHolder.transform);
            SetText(basicItem, reward.ToString(), "Offer", "text-offer");
            SetText(basicItem, GetLocalizedPrice(productId), "purchase-button", "text-price");
        }

        RefreshBundleItem();
    }

    /// <summary>
    /// Initiates a purchase for the specified product id by invoking the same
    /// event used by <see cref="IAPService"/>. This mirrors the behaviour used
    /// throughout the project for other in-app purchases. The method is public
    /// and accepts a string parameter so it can be hooked up to a button in the
    /// Unity inspector.
    /// </summary>
    /// <param name="productId">The id of the product to purchase.</param>
    public void PurchaseProduct(string productId)
    {
        EventService.UI.OnIAPPurchaseBtn?.Invoke(this, productId);
    }

    /// <summary>
    /// Backwards compatible wrapper for older references. Prefer using
    /// <see cref="PurchaseProduct"/> instead.
    /// </summary>
    /// <param name="productId">The id of the product to purchase.</param>
    [Obsolete("Use PurchaseProduct instead.")]
    public void PurchaseItem(string productId) => PurchaseProduct(productId);

    /// <summary>
    /// Convenience wrapper to purchase the first bundle.
    /// </summary>
    public void PurchaseBundle1() => PurchaseProduct(Database.GameSettings.InAppPurchases.Bundle_1.ID);

    /// <summary>
    /// Convenience wrapper to purchase the second bundle.
    /// </summary>
    public void PurchaseBundle2() => PurchaseProduct(Database.GameSettings.InAppPurchases.Bundle_2.ID);

// While I generally prefer to avoid using Find(), in this case it helps reduce
// excessive inspector references caused by the current prefab design.
// Using Find() within smaller objects should not have a significant performance impact.
// TODO: Redesign the prefab structure to improve maintainability and eliminate the need for Find().

    public void RefreshBundleItem()
    {
        var bundle = Database.GameSettings.InAppPurchases.Bundle_1;
        SetText(bundle_1, GetLocalizedPrice(bundle.ID), "purchase-button", "text-price");
        SetText(bundle_1, bundle.Coins.ToString(), "mainOffer", "text-offer");
        SetText(bundle_1, bundle.Booster_Row.ToString(), "2nd-offer-panel", "2ndOffer", "text-offer");
        SetText(bundle_1, bundle.Booster_Col.ToString(), "2nd-offer-panel", "2ndOffer_1", "text-offer");
        SetText(bundle_1, bundle.Booster_Square.ToString(), "2nd-offer-panel", "2ndOffer_2", "text-offer");
        SetText(bundle_1, bundle.Booster_Shape.ToString(), "2nd-offer-panel", "2ndOffer_3", "text-offer");

        var bundle2 = Database.GameSettings.InAppPurchases.Bundle_2;
        SetText(bundle_2, GetLocalizedPrice(bundle2.ID), "purchase-button", "text-price");
        SetText(bundle_2, bundle2.Coins.ToString(), "mainOffer", "text-offer");
        SetText(bundle_2, bundle2.Booster_Row.ToString(), "2nd-offer-panel", "2ndOffer", "text-offer");
        SetText(bundle_2, bundle2.Booster_Col.ToString(), "2nd-offer-panel", "2ndOffer_1", "text-offer");
        SetText(bundle_2, bundle2.Booster_Square.ToString(), "2nd-offer-panel", "2ndOffer_2", "text-offer");
        SetText(bundle_2, bundle2.Booster_Shape.ToString(), "2nd-offer-panel", "2ndOffer_3", "text-offer");
    }
    
}



