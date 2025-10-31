using UnityEngine;
using System;
using TMPro;

// Local demo stub for IAP to avoid requiring the Unity.Services.IAP package during development.
public static class IAP
{
    public static void Initialize(Action onInitialized, Action<Exception> onInitializeFailed)
    {
        try
        {
            onInitialized?.Invoke();
        }
        catch (Exception e)
        {
            onInitializeFailed?.Invoke(e);
        }
    }
}

public class IAPDemoManager : MonoBehaviour
{
    [Serializable]
    public class DemoProduct
    {
        public string id;          
        public string displayName; 
        public int coinAmount;     
    }

    [Header("Danh sách sản phẩm demo")]
    [SerializeField] private DemoProduct[] products;

    [Header("UI thông báo")]
    [SerializeField] private TMP_Text messageText; 
    [SerializeField] private MoneyUI moneyUI; 

    private void Awake()
    {
        try
        {
            IAP.Initialize(OnInitialized, OnInitializeFailed);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[IAPDemo] Initialize failed: " + e);
        }
    }

    private void OnInitialized()
    {
        Debug.Log("[IAPDemo] IAP Initialized (demo)");
    }

    private void OnInitializeFailed(Exception e)
    {
        Debug.LogWarning("[IAPDemo] IAP Initialize failed: " + e.Message);
    }

    public void Purchase(string productId)
    {
        DemoProduct product = Array.Find(products, p => p.id == productId);
        if (product == null) 
        {
            Debug.LogWarning($"[IAPDemo] Product ID {productId} not found!");
            return;
        }

        Debug.Log($"[IAPDemo] Purchased: {product.displayName} ({product.coinAmount} coins)");

        // Cộng tiền
        if (PlayerMoneyManager.Instance != null)
        {
            PlayerMoneyManager.Instance.AddMoney(product.coinAmount);
        }
        else
        {
            Debug.LogWarning("[IAPDemo] PlayerMoneyManager.Instance chưa khởi tạo!");
        }

        // Cập nhật UI
        moneyUI?.UpdateUI();

        // Hiển thị thông báo
        if (messageText != null)
        {
            messageText.text = $"Bạn vừa nạp {product.coinAmount} coins từ {product.displayName}";
        }
    }
}
