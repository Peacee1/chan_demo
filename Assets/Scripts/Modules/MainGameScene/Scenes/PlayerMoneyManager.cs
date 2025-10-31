using UnityEngine;

public class PlayerMoneyManager : MonoBehaviour
{
    public static PlayerMoneyManager Instance;

    private int money;
    private int winStreak; // ✅ đếm chuỗi thắng
    public int lastChange { get; private set; } = 0; // ✅ số tiền thay đổi sau trận gần nhất

    private const string MONEY_KEY = "PlayerMoney";
    private const string STREAK_KEY = "PlayerWinStreak";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Lấy dữ liệu cũ
            money = PlayerPrefs.GetInt(MONEY_KEY, 500); // mặc định 500
            winStreak = PlayerPrefs.GetInt(STREAK_KEY, 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetMoney()
    {
        return money;
    }

    public int GetWinStreak()
    {
        return winStreak;
    }

    public int AddWinMoney()
    {
        winStreak++;
        int reward = 150;

        if (winStreak == 3) reward += 200;
        else if (winStreak == 5) reward += 250;
        else if (winStreak == 10) reward += 300;

        money += reward;
        SaveData();
        return reward; // ✅ trả về số điểm vừa cộng
    }

    public int AddLoseMoney()
    {
        winStreak = 0;
        int penalty = 100;
        money -= penalty;
        if (money < 0) money = 0;

        SaveData();
        return penalty; // ✅ trả về số điểm vừa trừ
    }
    private void SaveData()
    {
        PlayerPrefs.SetInt(MONEY_KEY, money);
        PlayerPrefs.SetInt(STREAK_KEY, winStreak);
        PlayerPrefs.Save();
    }
    public int AddMoney(int amount)
    {
        money += amount;
        if (money < 0) money = 0;
        lastChange = amount;
        SaveData();
        return money; // trả về số tiền hiện tại sau khi cộng
    }
}
