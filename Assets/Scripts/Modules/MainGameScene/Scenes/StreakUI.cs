using UnityEngine;
using TMPro;

public class StreakUI : MonoBehaviour
{
    public TMP_Text streakText; // ✅ thêm text hiển thị chuỗi thắng

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (PlayerMoneyManager.Instance != null)
        {
            streakText.text = "Chuỗi thắng: " + PlayerMoneyManager.Instance.GetWinStreak();
        }
    }
}
