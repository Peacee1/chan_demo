using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject tipPopup;
    [SerializeField] private Toggle dontShowAgainToggle; // ✅ Toggle trong tipPopup
    [SerializeField] private GameObject readyPopup;
    [SerializeField] private GameObject popupFindingOpponent;
    [SerializeField] private GameObject DimBackGround;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI findingText;

    [Header("Menu Buttons")]
    [SerializeField] private Button[] menuButtons;

    private Coroutine countdownCoroutine;
    private Coroutine findingCoroutine;
    private const int READY_TIME = 10;
    private const string PREF_KEY = "SkipPlayTip";

    private void Start()
    {
        if (readyPopup != null) readyPopup.SetActive(false);
        if (popupFindingOpponent != null) popupFindingOpponent.SetActive(false);
        if (DimBackGround != null) DimBackGround.SetActive(false);
        if (tipPopup != null) tipPopup.SetActive(false);

        if (countdownText != null) countdownText.text = "";
        if (findingText != null) findingText.text = "";
    }

    // 👉 Nút Play
    public void PlayGame()
    {
        if (popupFindingOpponent != null && popupFindingOpponent.activeSelf)
        {
            Debug.Log("Đang tìm đối thủ, không thể PlayGame lại!");
            return;
        }

        // Nếu user đã chọn "Không hiện lại"
        if (PlayerPrefs.GetInt(PREF_KEY, 0) == 1)
        {
            ShowFindingOpponent();
        }
        else
        {
            // Mở tipPopup
            tipPopup.SetActive(true);
            DimBackGround.SetActive(true);
        }
    }

    // 👉 Nút Đóng trong tipPopup
    public void OnCloseTipPopup()
    {
        // Nếu user chọn "Không hiện lại" thì lưu lại
        if (dontShowAgainToggle != null && dontShowAgainToggle.isOn)
        {
            PlayerPrefs.SetInt(PREF_KEY, 1);
            PlayerPrefs.Save();
        }

        // Đóng tipPopup
        tipPopup.SetActive(false);

        // Vào luôn Finding Opponent
        ShowFindingOpponent();
    }

    // 👉 Hàm hiển thị popup tìm đối thủ
    private void ShowFindingOpponent()
    {
        if (popupFindingOpponent != null)
        {
            popupFindingOpponent.SetActive(true);
            DimBackGround.SetActive(true);
            SetMenuButtonsInteractable(false);

            float delay = Random.Range(2f, 5f);
            findingCoroutine = StartCoroutine(FindingOpponentRoutine(delay));
        }
    }

    private IEnumerator FindingOpponentRoutine(float delay)
    {
        float elapsed = 0f;

        while (elapsed < delay)
        {
            if (findingText != null)
                findingText.text = $"Đang tìm đối thủ... {Mathf.CeilToInt(elapsed)}s";

            yield return new WaitForSecondsRealtime(1f);
            elapsed += 1f;
        }

        popupFindingOpponent.SetActive(false);
        if (readyPopup != null)
        {
            readyPopup.SetActive(true);

            if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
            countdownCoroutine = StartCoroutine(AutoStartCountdown());
        }

        SetMenuButtonsInteractable(true);
    }

    // Ready Popup confirm
    public void OnReadyConfirmed()
    {
        StopCountdown();
        Time.timeScale = 1f;
        LoadGame();
    }

    // Ready Popup cancel
    public void OnCancel()
    {
        StopCountdown();
        if (readyPopup != null) readyPopup.SetActive(false);
        if (popupFindingOpponent != null) popupFindingOpponent.SetActive(false);
        if (DimBackGround != null) DimBackGround.SetActive(false);
        SetMenuButtonsInteractable(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }

    private IEnumerator AutoStartCountdown()
    {
        int time = READY_TIME;

        while (time > 0)
        {
            if (countdownText != null)
                countdownText.text = time.ToString();
            yield return new WaitForSecondsRealtime(1f);
            time--;
        }

        OnReadyConfirmed();
    }

    private void StopCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        if (countdownText != null)
            countdownText.text = "";
    }

    private void LoadGame()
    {
        Debug.Log("Loading MainGameScene...");
        SceneManager.LoadScene(1);
    }

    private void SetMenuButtonsInteractable(bool enable)
    {
        if (menuButtons == null) return;
        foreach (Button btn in menuButtons)
        {
            if (btn != null) btn.interactable = enable;
        }
    }

    // ✅ Hàm reset để test
    public void ResetTipPopup()
    {
        PlayerPrefs.DeleteKey(PREF_KEY);
        PlayerPrefs.Save();
        Debug.Log("Reset SkipPlayTip -> lần sau sẽ hiện lại popup tip");
    }
}
