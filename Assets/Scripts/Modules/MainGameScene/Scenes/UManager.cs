using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class UManager : MonoBehaviour
{
    [Header("References")]
    public GameObject panelU;
    public GameObject panelTinhDiem;

    [Header("Buttons")]
    public Button buttonU;
    public Button buttonConfirmU;
    public Button buttonUThong;
    public Button buttonUXuong;
    public Button buttonUChi;
    public Button buttonUChiu;
    public Button buttonClose;

    [Header("Texts")]
    public TMP_Text textDanhSachU;
    public TMP_Text textDiem;

    private List<string> selectedUTypes = new List<string>();
    private Dictionary<Button, string> buttonMap;
    private Dictionary<Button, bool> buttonStates = new Dictionary<Button, bool>();

    private Color normalColor = Color.white;
    private bool hasShownNotification = false;
    public TMP_Text textNotification;

    void Start()
    {
        panelU.SetActive(false);
        panelTinhDiem.SetActive(false);
        buttonU.interactable = false;

        buttonMap = new Dictionary<Button, string>
        {
            { buttonUThong, "Ù Thông" },
            { buttonUXuong, "Ù Xuông" },
            { buttonUChi,   "Ù Chì"   },
            { buttonUChiu,  "Ù Chíu"  }
        };

        foreach (var kvp in buttonMap)
        {
            var btn = kvp.Key;
            buttonStates[btn] = false;
            btn.onClick.AddListener(() => OnUToggle(btn));
        }

        buttonU.onClick.AddListener(OpenPanel);
        buttonConfirmU.onClick.AddListener(ConfirmU);
        buttonClose.onClick.AddListener(ClosePanel);
    }

    void OpenPanel()
    {
        panelU.SetActive(true);
        panelTinhDiem.SetActive(false);
    }

    void ClosePanel()
    {
        panelU.SetActive(false);

        // Reset danh sách Ù đã chọn
        selectedUTypes.Clear();

        // Reset trạng thái các button về mặc định
        foreach (var kvp in buttonMap)
        {
            buttonStates[kvp.Key] = false;
            kvp.Key.image.color = normalColor;
        }
    }

    void OnUToggle(Button btn)
    {
        // Toggle trạng thái nút
        bool isSelected = !buttonStates[btn];
        buttonStates[btn] = isSelected;

        if (isSelected)
        {
            if (!selectedUTypes.Contains(buttonMap[btn]))
                selectedUTypes.Add(buttonMap[btn]);

            btn.image.color = new Color(0.5f, 0.5f, 0.5f, 1f); 
        }
        else
        {
            selectedUTypes.Remove(buttonMap[btn]);
            btn.image.color = Color.white; 
        }
    }

    void ConfirmU()
    {
        panelU.SetActive(false);
        panelTinhDiem.SetActive(true);
        textDanhSachU.text = string.Join(", ", selectedUTypes);
        int reward = PlayerMoneyManager.Instance.AddWinMoney();
        int streak = PlayerMoneyManager.Instance.GetWinStreak();
        textDiem.text = $"Điểm: +{reward}";
        Debug.Log($"Người chơi Ù! Thưởng {reward}, streak {streak}");
    }
    public void EnableUButton(bool enable)
    {
        buttonU.interactable = enable;

        if (enable && textNotification != null && !hasShownNotification)
        {
            hasShownNotification = true;
            StartCoroutine(NotificationCoroutine("Bạn có thể Ù", 1f));
        }
    }

    private IEnumerator NotificationCoroutine(string message, float duration)
    {
        textNotification.text = message;
        textNotification.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        textNotification.gameObject.SetActive(false);
    }
}
