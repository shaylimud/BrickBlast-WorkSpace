using System.Reflection;
using Ray.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

public class Shop : MonoBehaviour
{
    [Header("Shop-Items")]
    
    [SerializeField] public GameObject bundle_1;
    [SerializeField] public GameObject bundle_2;
    public GameObject itemPrefab;
    public GameObject Special;

    [Header("Holders")] 
    [SerializeField] private GameObject itemHolder;

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

    public void BuildBasicItems()
    {
        
        GameObject basicItem = Instantiate(itemPrefab , itemHolder.transform);
        itemPrefab.transform.Find("text-offer").GetComponent<TextMeshProUGUI>().text = GetLocalizedPrice("ProductID");
    }
    
    
}
