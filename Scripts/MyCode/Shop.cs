using System;
using System.Reflection;
using Ray.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

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

    public int GetProductValue(string productId)
    {
        if (Database.GameSettings?.InAppPurchases?.Consumables == null)
            return 0;

        return Database.GameSettings.InAppPurchases.Consumables.TryGetValue(productId, out var value)
            ? value
            : 0;
    }
    public ProductMetadata GetProductMetadata(string productId)
    {
        return GetProduct(productId)?.metadata;
    }

    public void BuildBasicItems()
    {
        Debug.Log("Shay : Bulding UI 1");

        foreach (var productId in Database.GameSettings.InAppPurchases.Consumables.Keys)
        {
            Debug.Log("Shay : Bulding UI");
            GameObject basicItem = Instantiate(itemPrefab, itemHolder.transform);
            basicItem.transform.Find("text-offer").GetComponent<TextMeshProUGUI>().text = GetLocalizedPrice(productId);
        }
    }
    
    
}
