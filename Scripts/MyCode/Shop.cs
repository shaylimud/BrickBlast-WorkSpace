using System;
using System.Reflection;
using System.Threading.Tasks;
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


    public ProductMetadata GetProductMetadata(string productId)
    {
        return GetProduct(productId)?.metadata;
    }

    public async void BuildBasicItems()
    {
        Debug.Log("Shay : Bulding UI 1");

        while (Database.GameSettings == null ||
               Database.GameSettings.InAppPurchases == null ||
               Database.GameSettings.InAppPurchases.Consumables == null)
        {
            await Task.Yield();
        }

        foreach (var productId in Database.GameSettings.InAppPurchases.Consumables.Keys)
        {
            Debug.Log("Shay : Bulding UI");
            GameObject basicItem = Instantiate(itemPrefab, itemHolder.transform);
            basicItem.transform.Find("text-offer").GetComponent<TextMeshProUGUI>().text =
                Database.GameSettings.InAppPurchases.ConsumableRewardById(productId).ToString();
        }
    }
    
    
}
