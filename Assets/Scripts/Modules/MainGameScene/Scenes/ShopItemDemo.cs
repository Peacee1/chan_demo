using UnityEngine;
using UnityEngine.UI;

public class ShopItemDemo : MonoBehaviour
{
    public string itemName = "Gói 100 coins";  // tên item hiển thị
    public int coinAmount = 100;               // số coin sẽ cộng khi demo
    public Text messageText;                   // UI Text hiển thị thông báo
    public MoneyUI moneyUI;                    // UI hiển thị tiền, gán trong Inspector

    // Gọi khi người chơi click item
    public void OnClickItem()
    {
        AddCoins();
        ShowPurchaseMessage();
    }

    private void ShowPurchaseMessage()
    {
        string msg = $"Bạn vừa nạp {coinAmount} coins từ {itemName}!";
        Debug.Log(msg);
        if (messageText != null)
            messageText.text = msg;

        // TODO: thêm animation, popup tự ẩn nếu muốn
    }

    private void AddCoins()
    {
        if (PlayerMoneyManager.Instance != null)
        {
            // Cộng coin qua PlayerMoneyManager
            PlayerMoneyManager.Instance.AddMoney(coinAmount);

            // Cập nhật UI MoneyUI
            if (moneyUI != null)
            {
                moneyUI.UpdateUI();
            }

            Debug.Log("Coins hiện tại: " + PlayerMoneyManager.Instance.GetMoney());
        }
        else
        {
            Debug.LogWarning("PlayerMoneyManager.Instance chưa được khởi tạo!");
        }
    }
}
