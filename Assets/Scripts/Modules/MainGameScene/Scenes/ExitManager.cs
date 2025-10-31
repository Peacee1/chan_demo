using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitManager : MonoBehaviour
{
    [Header("References")]
    public GameObject panelConfirmExit;
    public Button exitButton;        // Nút exit trong MainGame
    public Button confirmExitButton; // Nút Thoát trong panel
    public Button cancelExitButton;  // Nút Hủy trong panel

    void Start()
    {
        // Ẩn panel khi start game
        if (panelConfirmExit != null)
            panelConfirmExit.SetActive(false);

        // Gán sự kiện
        exitButton.onClick.AddListener(OpenConfirmPanel);
        confirmExitButton.onClick.AddListener(ExitToMainMenu);
        cancelExitButton.onClick.AddListener(CloseConfirmPanel);
    }

    void OpenConfirmPanel()
    {
        if (panelConfirmExit != null)
            panelConfirmExit.SetActive(true);
    }

    void CloseConfirmPanel()
    {
        if (panelConfirmExit != null)
            panelConfirmExit.SetActive(false);
    }

    void ExitToMainMenu()
    {
        SceneManager.LoadScene(0);
        PlayerMoneyManager.Instance.AddLoseMoney(); // đổi đúng tên scene main menu
    }
}
