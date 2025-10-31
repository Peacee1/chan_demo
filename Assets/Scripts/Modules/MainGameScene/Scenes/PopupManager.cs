using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    [Header("Popups")]
    [SerializeField] private GameObject popupReady;
    [SerializeField] private GameObject popupFindingOpponent;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button readyButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text findingText;  // Text trong popup FindingOpponent

    private Coroutine findingCoroutine;

    private void Start()
    {
        // Ban đầu ẩn hết
        popupReady.SetActive(false);
        popupFindingOpponent.SetActive(false);

        // Lắng nghe nút
        playButton.onClick.AddListener(OnPlayClicked);
        readyButton.onClick.AddListener(OnReadyClicked);
    }

    private void OnPlayClicked()
    {
        // Hiện popup tìm đối thủ
        popupFindingOpponent.SetActive(true);

        // Random delay 2–5 giây
        float delay = Random.Range(2f, 5f);

        // Bắt đầu coroutine hiển thị text + timer
        findingCoroutine = StartCoroutine(FindingOpponentRoutine(delay));
    }

    private IEnumerator FindingOpponentRoutine(float delay)
    {
        float elapsed = 0f;
        int dotCount = 0;

        while (elapsed < delay)
        {
            // Cập nhật text theo giây
            dotCount = (dotCount % 3) + 1; // xoay vòng ., .., ...
            string dots = new string('.', dotCount);

            findingText.text = $"Đang tìm đối thủ{dots} ({elapsed:F1}s)";

            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        // Kết thúc tìm đối thủ
        popupFindingOpponent.SetActive(false);
        popupReady.SetActive(true);
    }

    private void OnReadyClicked()
    {
        popupReady.SetActive(false);

        // Bắt đầu game
        CardDealer.Instance.StartGame();
        GameManager.Instance.BeginFirstTurn();
    }
}
