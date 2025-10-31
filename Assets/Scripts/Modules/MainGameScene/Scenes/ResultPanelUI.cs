using UnityEngine;
using TMPro;

public class ResultPanelUI : MonoBehaviour
{
    public static ResultPanelUI Instance;

    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject baoPanel;

    [Header("Texts")]
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text loseBaoText;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowWin(int reward, int streak)
    {
        winPanel.SetActive(true);
        losePanel.SetActive(false);
        baoPanel.SetActive(false);

        winText.text = $"+{reward} điểm\nChuỗi thắng: {streak}";
    }

    public void ShowLose(int penalty)
    {
        losePanel.SetActive(true);
        winPanel.SetActive(false);
        baoPanel.SetActive(false);

        loseText.text = $"- {penalty} điểm\nChuỗi thắng reset!";
    }
        public void ShowBao(int penalty)
    {
        losePanel.SetActive(false);
        winPanel.SetActive(false);
        baoPanel.SetActive(true);

        loseBaoText.text = $"{penalty} điểm\nChuỗi thắng reset!";
    }
}
