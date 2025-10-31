using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayTipManager : MonoBehaviour
{
    public static PlayTipManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject tipPopup;
    [SerializeField] private TMP_Text tipMessage;
    [SerializeField] private Toggle dontShowAgainToggle;
    [SerializeField] private Button closeButton;

    private const string PREF_KEY = "SkipPlayTip";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (tipPopup != null) tipPopup.SetActive(false);
    }

    public void Show(string message)
    {
        // Nếu người chơi đã chọn "không hiện lại"
        if (PlayerPrefs.GetInt(PREF_KEY, 0) == 1) return;

        if (tipPopup != null && tipMessage != null)
        {
            tipMessage.text = message;
            tipPopup.SetActive(true);
        }
    }

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(OnClose);
    }

    private void OnClose()
    {
        if (dontShowAgainToggle != null && dontShowAgainToggle.isOn)
        {
            // Lưu lại trạng thái để không hiện lần sau
            PlayerPrefs.SetInt(PREF_KEY, 1);
            PlayerPrefs.Save();
        }

        if (tipPopup != null) tipPopup.SetActive(false);
    }
}
