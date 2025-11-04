using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

/// <summary>
/// Shop Demo Item - Quản lý các event handlers cho Unity Purchasing
/// </summary>
public class ShopItemDemo : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Hiển thị log chi tiết")]
    public bool enableDebugLog = true;

    [Header("UI References (Optional)")]
    [Tooltip("Text hiển thị thông báo (nếu có)")]
    public Text statusText;

    [Header("Test Product IDs")]
    public string[] productIds = new string[]
    {
        "com.chan.coin10k",
        "com.chan.coin55k",
        "com.chan.coin120k",
        "com.chan.coin300k",
        "com.chan.coin800k",
        "com.chan.coin2m",
        "com.chan.coin5m",
        "com.chan.coin12m"
    };

    private void Start()
    {
        Debug.Log("ShopItemDemo initialized.");
    }
    public void OnProductFetched(Product product)
    {
        Debug.Log($"Fetched product: {product.definition.id} - Price: {product.metadata.localizedPriceString}");
    }

    public void OnProductFetchFailed(ProductDefinition productDefinition, string reason)
    {
        Debug.LogError($"Fetch failed for {productDefinition.id} - Reason: {reason}");
    }

    public void OnOrderPending(PendingOrder pendingOrder)
    {
        Debug.Log($"Order pending: {pendingOrder}");
    }

    public void OnOrderConfirmed(ConfirmedOrder confirmedOrder)
    {
        Debug.Log($"Order confirmed: {confirmedOrder}");
    }

    public void OnPurchaseFailed(FailedOrder failedOrder)
    {
        Debug.LogError($"Order failed: {failedOrder} - {failedOrder}");
    }

    public void OnOrderDeferred(DeferredOrder deferredOrder)
    {
        Debug.Log($"Order deferred: {deferredOrder}");
    }

    // === Legacy API (cũ hơn) ===
    public void OnPurchaseCompleteLegacy(Product product)
    {
        Debug.Log($"Purchase complete (Legacy): {product.definition.id}");
        ProcessProductReward(product);
    }

    public void OnPurchaseFailedLegacy(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"Legacy purchase failed: {product.definition.id} - {failureDescription.reason}");
    }
    private void ProcessProductReward(Product product)
    {
        switch (product.definition.id)
        {
            case "com.chan.coin10k":
                PlayerMoneyManager.Instance.AddMoney(10000);
                break;
            case "com.chan.coin55k":
                PlayerMoneyManager.Instance.AddMoney(55000);
                break;
            case "com.chan.coin120k":
                PlayerMoneyManager.Instance.AddMoney(120000);
                break;
            case "com.chan.coin300k":
                PlayerMoneyManager.Instance.AddMoney(300000);
                break;
            case "com.chan.coin800k":
                PlayerMoneyManager.Instance.AddMoney(800000);
                break;
            case "com.chan.coin2m":
                PlayerMoneyManager.Instance.AddMoney(2000000);
                break;
            case "com.chan.coin5m":
                PlayerMoneyManager.Instance.AddMoney(5000000);
                break;
            case "com.chan.coin12m":
                PlayerMoneyManager.Instance.AddMoney(12000000);
                break;
            default:
                break;
        }
    }
}