using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    private IStoreController storeController;
    private IExtensionProvider extensionProvider;
    private void Star()
    {
        SetupBuild();
    }
    private bool IsInitialized()
    {
        return storeController != null && extensionProvider != null;
    }
    public void SetupBuild()
    {
        if (IsInitialized())
        {
            Debug.Log("IAP already initialized");
            return;
        }
        var module = StandardPurchasingModule.Instance();
        var builder = ConfigurationBuilder.Instance(module);
        builder.AddProduct("com.chan.coin10k", ProductType.Consumable);
        builder.AddProduct("com.chan.coin55k", ProductType.Consumable);
        builder.AddProduct("com.chan.coin120k", ProductType.Consumable);
        builder.AddProduct("com.chan.coin300k", ProductType.Consumable);
        builder.AddProduct("com.chan.coin800k", ProductType.Consumable);
        builder.AddProduct("com.chan.coin2m", ProductType.Consumable);
        builder.AddProduct("com.chan.coin5m", ProductType.Consumable);
        builder.AddProduct("com.chan.coin12m", ProductType.Consumable);
        UnityPurchasing.Initialize(this, builder);
    }
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        Debug.Log("IAP initialized successfully");
    }
    public void OnInitializeFailed(InitializationFailureReason error)
    {
    }
    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        // Handled by ShopItemDemo
        return PurchaseProcessingResult.Complete;
    }
}
