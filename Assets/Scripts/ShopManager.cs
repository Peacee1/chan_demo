using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

/// <summary>
/// Manager quản lý shop items sử dụng ShopItemSO
/// Có thể tự động load từ Unity IAP ProductCatalog hoặc gán thủ công
/// </summary>
public class ShopManager : MonoBehaviour
{
    public enum LoadMode
    {
        Manual,              // Gán thủ công ShopItemSO assets
        FromProductCatalog,  // Tự động load từ Unity IAP ProductCatalog
        FromJSON             // Tự động load từ JSON file
    }

    [Header("Load Mode")]
    [Tooltip("Cách load shop items: Manual = gán thủ công, FromProductCatalog = tự động từ catalog, FromJSON = từ JSON")]
    [SerializeField] private LoadMode loadMode = LoadMode.Manual;

    [Header("Danh sách Shop Items (Manual Mode)")]
    [Tooltip("Kéo thả các ShopItemSO assets vào đây (chỉ dùng khi Load Mode = Manual)")]
    [SerializeField] private ShopItemSO[] shopItems;

    [Header("Auto Load Settings")]
    [Tooltip("Path đến file JSON catalog (VD: IAPProductCatalog)")]
    [SerializeField] private string jsonCatalogPath = "IAPProductCatalog";

    [Header("Debug")]
    [SerializeField] private bool logItemsOnStart = true;

    // Runtime loaded items (dùng khi load từ catalog)
    private List<ShopItemSO> runtimeLoadedItems = new List<ShopItemSO>();

    private void Awake()
    {
        // Load items theo mode được chọn
        switch (loadMode)
        {
            case LoadMode.Manual:
                // Sử dụng shopItems đã gán trong Inspector
                break;

            case LoadMode.FromProductCatalog:
                LoadFromProductCatalog();
                break;

            case LoadMode.FromJSON:
                LoadFromJSON();
                break;
        }
    }

    private void Start()
    {
        if (logItemsOnStart)
        {
            LogShopItems();
        }
    }

    /// <summary>
    /// Tự động load từ Unity IAP ProductCatalog
    /// </summary>
    private void LoadFromProductCatalog()
    {
#if UNITY_PURCHASING
        try
        {
            var catalog = ProductCatalog.LoadDefaultCatalog();
            
            if (catalog == null || catalog.allProducts == null || catalog.allProducts.Count == 0)
            {
                Debug.LogWarning("[ShopManager] ProductCatalog không có sản phẩm nào!");
                return;
            }

            runtimeLoadedItems.Clear();

            foreach (var product in catalog.allProducts)
            {
                // Tạo ShopItemSO runtime (không phải asset)
                ShopItemSO item = ScriptableObject.CreateInstance<ShopItemSO>();
                
                // Map từ ProductCatalog.Product sang ShopItemSO
                item.productID = product.id;
                item.itemID = product.id;
                item.displayName = product.defaultDescription?.title ?? product.id;
                item.description = product.defaultDescription?.description ?? "";
                item.priceDisplay = GetPriceDisplay(product);
                item.coinAmount = ExtractCoinAmount(product.id);
                item.itemType = ShopItemSO.ItemType.Coin;
                item.isAvailable = true;
                item.isSpecialOffer = DetectSpecialOffer(product.defaultDescription?.description ?? "");

                runtimeLoadedItems.Add(item);
            }

            Debug.Log($"[ShopManager] Đã load {runtimeLoadedItems.Count} items từ ProductCatalog");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ShopManager] Lỗi khi load từ ProductCatalog: {e.Message}");
        }
#else
        Debug.LogError("[ShopManager] Unity Purchasing chưa được import! Vui lòng import Unity Purchasing package.");
#endif
    }

    /// <summary>
    /// Load từ JSON file (IAPProductCatalog.json)
    /// </summary>
    private void LoadFromJSON()
    {
        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(jsonCatalogPath);
            
            if (jsonFile == null)
            {
                Debug.LogError($"[ShopManager] Không tìm thấy file JSON tại Resources/{jsonCatalogPath}");
                return;
            }

            // Parse JSON
            var catalogData = JsonUtility.FromJson<ProductCatalogData>(jsonFile.text);
            
            if (catalogData == null || catalogData.products == null || catalogData.products.Length == 0)
            {
                Debug.LogWarning("[ShopManager] JSON catalog không có sản phẩm nào!");
                return;
            }

            runtimeLoadedItems.Clear();

            foreach (var product in catalogData.products)
            {
                ShopItemSO item = ScriptableObject.CreateInstance<ShopItemSO>();
                
                item.productID = product.id;
                item.itemID = product.id;
                
                // Xử lý title và description (decode Unicode escape sequences)
                string title = product.defaultDescription?.title ?? product.id;
                string desc = product.defaultDescription?.description ?? "";
                item.displayName = System.Text.RegularExpressions.Regex.Unescape(title);
                item.description = System.Text.RegularExpressions.Regex.Unescape(desc);
                
                item.priceDisplay = $"Tier {product.applePriceTier}"; // Có thể cải thiện sau
                item.coinAmount = ExtractCoinAmount(product.id);
                item.itemType = ShopItemSO.ItemType.Coin;
                item.isAvailable = true;
                item.isSpecialOffer = DetectSpecialOffer(desc);

                runtimeLoadedItems.Add(item);
            }

            Debug.Log($"[ShopManager] Đã load {runtimeLoadedItems.Count} items từ JSON catalog");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ShopManager] Lỗi khi load từ JSON: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// Trích xuất số coin từ product ID (VD: "com.chan.coin10k" -> 10000)
    /// </summary>
    private int ExtractCoinAmount(string productID)
    {
        try
        {
            // Xử lý các format khác nhau
            string lowerID = productID.ToLower();
            
            if (lowerID.Contains("coin"))
            {
                // Tìm số trong ID
                string numberStr = "";
                bool foundNumber = false;
                
                foreach (char c in lowerID)
                {
                    if (char.IsDigit(c))
                    {
                        numberStr += c;
                        foundNumber = true;
                    }
                    else if (foundNumber && !char.IsDigit(c))
                    {
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(numberStr))
                {
                    int amount = int.Parse(numberStr);
                    
                    // Xử lý k, m (thousand, million)
                    if (lowerID.Contains("k") && amount < 1000)
                        amount *= 1000;
                    else if (lowerID.Contains("m") && amount < 1000000)
                        amount *= 1000000;
                    
                    return amount;
                }
            }
            
            // Default values dựa trên product ID patterns
            if (lowerID.Contains("10k") || lowerID.Contains("10k")) return 10000;
            if (lowerID.Contains("55k")) return 55000;
            if (lowerID.Contains("120k")) return 120000;
            if (lowerID.Contains("300k")) return 300000;
            if (lowerID.Contains("800k")) return 800000;
            if (lowerID.Contains("2m")) return 2000000;
            if (lowerID.Contains("5m")) return 5000000;
            if (lowerID.Contains("12m")) return 12000000;
            
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Lấy price display từ product
    /// </summary>
#if UNITY_PURCHASING
    private string GetPriceDisplay(ProductCatalog.Product product)
    {
        // Có thể cải thiện bằng cách lấy từ runtime store price
        // Hiện tại trả về tier number
        return $"Tier {product.applePriceTier}";
    }
#endif

    /// <summary>
    /// Phát hiện item có phải special offer không từ description
    /// </summary>
    private bool DetectSpecialOffer(string description)
    {
        string lower = description.ToLower();
        return lower.Contains("bonus") || lower.Contains("khuyến mãi") || 
               lower.Contains("special") || lower.Contains("đặc biệt");
    }

    /// <summary>
    /// Lấy tất cả shop items (tự động chọn nguồn dữ liệu)
    /// </summary>
    public ShopItemSO[] GetAllItems()
    {
        if (loadMode == LoadMode.Manual)
        {
            return shopItems;
        }
        else
        {
            return runtimeLoadedItems.ToArray();
        }
    }

    /// <summary>
    /// Lấy danh sách items dưới dạng List
    /// </summary>
    public List<ShopItemSO> GetAllItemsList()
    {
        if (loadMode == LoadMode.Manual)
        {
            return new List<ShopItemSO>(shopItems);
        }
        else
        {
            return new List<ShopItemSO>(runtimeLoadedItems);
        }
    }

    /// <summary>
    /// Tìm item theo Item ID
    /// </summary>
    public ShopItemSO GetItemByID(string itemID)
    {
        if (loadMode == LoadMode.Manual)
        {
            return Array.Find(shopItems, item => item != null && item.itemID == itemID);
        }
        else
        {
            return runtimeLoadedItems.Find(item => item.itemID == itemID);
        }
    }

    /// <summary>
    /// Tìm item theo Product ID (cho IAP)
    /// </summary>
    public ShopItemSO GetItemByProductID(string productID)
    {
        if (loadMode == LoadMode.Manual)
        {
            return Array.Find(shopItems, item => item != null && item.productID == productID);
        }
        else
        {
            return runtimeLoadedItems.Find(item => item.productID == productID);
        }
    }

    /// <summary>
    /// Lấy danh sách items theo type
    /// </summary>
    public ShopItemSO[] GetItemsByType(ShopItemSO.ItemType type)
    {
        if (loadMode == LoadMode.Manual)
        {
            return Array.FindAll(shopItems, item => item != null && item.itemType == type && item.isAvailable);
        }
        else
        {
            return runtimeLoadedItems.FindAll(item => item.itemType == type && item.isAvailable).ToArray();
        }
    }

    /// <summary>
    /// Lấy danh sách items đang available
    /// </summary>
    public ShopItemSO[] GetAvailableItems()
    {
        if (loadMode == LoadMode.Manual)
        {
            return Array.FindAll(shopItems, item => item != null && item.isAvailable);
        }
        else
        {
            return runtimeLoadedItems.FindAll(item => item.isAvailable).ToArray();
        }
    }

    /// <summary>
    /// Log tất cả shop items (dùng để debug)
    /// </summary>
    private void LogShopItems()
    {
        ShopItemSO[] items = GetAllItems();
        
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning($"[ShopManager] Chưa có shop items nào! (Load Mode: {loadMode})");
            return;
        }

        Debug.Log($"[ShopManager] Đã load {items.Length} shop items (Load Mode: {loadMode}):");
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                Debug.Log($"  [{i}] {items[i].displayName} (ID: {items[i].itemID}, Product: {items[i].productID}, Coins: {items[i].coinAmount})");
            }
            else
            {
                Debug.LogWarning($"  [{i}] NULL - Có slot rỗng trong danh sách!");
            }
        }
    }

    // JSON Data Classes cho parsing
    [System.Serializable]
    private class ProductCatalogData
    {
        public ProductData[] products;
    }

    [System.Serializable]
    private class ProductData
    {
        public string id;
        public int type;
        public ProductDescription defaultDescription;
        public int applePriceTier;
    }

    [System.Serializable]
    private class ProductDescription
    {
        public string title;
        public string description;
    }
}
