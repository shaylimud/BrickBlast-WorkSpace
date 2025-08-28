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

    [Header("Image")] 
    [SerializeField] private Image coinIcon;
    [SerializeField] private Image rowIcon;
    [SerializeField] private Image colIcon; 
    [SerializeField] private Image shapeIcon;
    [SerializeField] private Image squareIcon;
    

    private void Awake()
    {
        instance = this;
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

    }
    
}



