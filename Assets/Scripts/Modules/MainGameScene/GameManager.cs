using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public enum TurnState { playerTurn, botTurn };
    [SerializeField] public TurnState turnState;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private CardManager cardManager;
    public static GameManager Instance;

    [Header("Panels")]
    public GameObject losePanel;
    public GameObject winPanel;

    private int skipCount = 0;
    private Coroutine turnTimerCoroutine;

    [Header("UI Timer")]
    [SerializeField] private TextMeshProUGUI userTimerText;
    [SerializeField] private TextMeshProUGUI botTimerText;

    private const float TURN_TIME = 15f;
    private int botPlayedCount = 0;

    void Start()
    {
        if (playerUI != null) playerUI.SetActive(false);
        if (userTimerText != null) userTimerText.text = "";
        if (botTimerText != null) botTimerText.text = "";
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGameScene")
        {
            turnState = TurnState.playerTurn;
            if (playerUI != null) playerUI.SetActive(true);

            if (CardDealer.Instance != null)
            {
                CardDealer.Instance.Cleanup();
                CardDealer.Instance.StartGame();
            }
        }
    }

    public void BeginFirstTurn()
    {
        if (turnTimerCoroutine == null)
        {
            Debug.Log("Bắt đầu lượt đầu tiên!");
            StartTurn();
        }
    }

    public void UserSkipTurn()
    {
        skipCount++;
        if (skipCount >= 5)
        {
            Debug.Log("User đã bỏ 5 lần => Thua!");
            HandleLose();
            return;
        }
        NextTurn();
    }

    // ✅ gọi khi user Ù thắng
    public void OnUserWin()
    {
        int reward = PlayerMoneyManager.Instance.AddWinMoney();
        Debug.Log("User Ù! Thắng + " + reward);
        HandleWin(reward, PlayerMoneyManager.Instance.GetWinStreak());
    }

    // ✅ gọi từ bot khi nó đánh xong
    public void OnBotPlayedCard()
    {
        botPlayedCount++;
        if (botPlayedCount >= 6)
        {
            Debug.Log("Bot Ù! User thua!");
            HandleLose();
            PlayerMoneyManager.Instance.AddLoseMoney();
            return;
        }
    }

    public void NextTurn()
    {
        if (turnState == TurnState.playerTurn)
        {
            if (cardManager != null) cardManager.ResetTurnFlags();
            turnState = TurnState.botTurn;
            if (playerUI != null) playerUI.SetActive(false);
        }
        else
        {
            if (cardManager != null) cardManager.ResetTurnFlags();
            turnState = TurnState.playerTurn;
            if (playerUI != null) playerUI.SetActive(true);
        }
        StartTurn();
    }

    private void StartTurn()
    {
        if (turnTimerCoroutine != null)
            StopCoroutine(turnTimerCoroutine);
        turnTimerCoroutine = StartCoroutine(TurnTimer());
    }

    private IEnumerator TurnTimer()
    {
        float remaining = TURN_TIME;
        if (userTimerText != null) userTimerText.text = "";
        if (botTimerText != null) botTimerText.text = "";

        while (remaining > 0f)
        {
            if (turnState == TurnState.playerTurn && userTimerText != null)
            {
                userTimerText.text = Mathf.CeilToInt(remaining).ToString();
                if (botTimerText != null) botTimerText.text = "";
            }
            else if (turnState == TurnState.botTurn && botTimerText != null)
            {
                botTimerText.text = Mathf.CeilToInt(remaining).ToString();
                if (userTimerText != null) userTimerText.text = "";
            }
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        if (turnState == TurnState.playerTurn && userTimerText != null)
            userTimerText.text = "0";
        if (turnState == TurnState.botTurn && botTimerText != null)
            botTimerText.text = "0";

        Debug.Log("Hết 15s, tự động đổi lượt!");
        NextTurn();
    }

    // ==========================
    // Xử lý kết quả thắng thua
    // ==========================
    private void HandleWin(int reward, int streak)
    {
        if (winPanel != null) winPanel.SetActive(true);
        if (ResultPanelUI.Instance != null)
            ResultPanelUI.Instance.ShowWin(reward, streak);

        Time.timeScale = 0f;
    }

    private void HandleLose()
    {
        if (losePanel != null) losePanel.SetActive(true);
        if (ResultPanelUI.Instance != null)
            ResultPanelUI.Instance.ShowLose(100);

        Time.timeScale = 0f;
    }
    private void HandleBao()
    {
        if (losePanel != null) losePanel.SetActive(true);
        if (ResultPanelUI.Instance != null)
            ResultPanelUI.Instance.ShowBao(100);

        Time.timeScale = 0f;
    }
}