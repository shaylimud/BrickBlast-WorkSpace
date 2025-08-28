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
            basicItem.transform.Find("Offer").transform.Find("text-offer").GetComponent<TextMeshProUGUI>().text = reward.ToString();
        }
    }

    public void BuildBundleItem()
    {
        
    }
    
    
}
