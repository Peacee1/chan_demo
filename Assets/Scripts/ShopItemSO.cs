using UnityEngine;

/// <summary>
/// ScriptableObject định nghĩa các item trong shop
/// </summary>
[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    [Tooltip("ID duy nhất của item (dùng cho IAP)")]
    public string itemID;
    
    [Tooltip("Tên hiển thị của item")]
    public string displayName;
    
    [Tooltip("Mô tả item")]
    [TextArea(2, 4)]
    public string description;

    [Header("Hình ảnh")]
    [Tooltip("Icon của item")]
    public Sprite icon;

    [Header("Giá trị")]
    [Tooltip("Số coin nhận được khi mua item này")]
    public int coinAmount;
    
    [Tooltip("Giá tiền hiển thị (VD: \"$4.99\", \"99.000đ\")")]
    public string priceDisplay;

    [Header("IAP Settings")]
    [Tooltip("Product ID cho Unity IAP (VD: com.chan.coin10k)")]
    public string productID;

    [Header("Thông tin khác")]
    [Tooltip("Loại item (VD: Coin, Gem, Special, etc.)")]
    public ItemType itemType = ItemType.Coin;
    
    [Tooltip("Có được bán không (có thể dùng để ẩn item)")]
    public bool isAvailable = true;
    
    [Tooltip("Có phải là item đặc biệt/khuyến mãi không")]
    public bool isSpecialOffer = false;

    public enum ItemType
    {
        Coin,
        Gem,
        Special,
        Pack
    }

    /// <summary>
    /// Validate dữ liệu khi tạo asset
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = name;
        }
        
        if (string.IsNullOrEmpty(productID))
        {
            productID = itemID;
        }
    }

    /// <summary>
    /// Lấy thông tin item dạng text
    /// </summary>
    public string GetInfo()
    {
        return $"{displayName}\n{description}\nGiá: {priceDisplay}\nNhận được: {coinAmount} coins";
    }
}

