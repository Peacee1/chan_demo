using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    public TMP_Text moneyText;
    public TMP_Text streakText; // ✅ thêm text hiển thị chuỗi thắng

    void Start()
    {
        if (streakText != null)
            streakText.gameObject.SetActive(false);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (PlayerMoneyManager.Instance != null)
        {
            moneyText.text = PlayerMoneyManager.Instance.GetMoney().ToString();
            // moneyText.text = "Gold: " + PlayerMoneyManager.Instance.GetMoney();
            // streakText.text = "Chuỗi thắng: " + PlayerMoneyManager.Instance.GetWinStreak();
        }
    }
}
