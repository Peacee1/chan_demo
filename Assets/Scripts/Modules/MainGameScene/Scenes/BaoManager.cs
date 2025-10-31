using UnityEngine;
using UnityEngine.SceneManagement;

public class BaoManager : MonoBehaviour
{
    [SerializeField] private GameObject panelBao; // gán Panel trong Inspector

    public void ShowPanel()
    {
        if (panelBao != null)
        {
            panelBao.SetActive(true);
            Debug.Log("Panel Báo đã hiện!");
            PlayerMoneyManager.Instance.AddLoseMoney();
        }
    }

    public void HidePanel()
    {
        if (panelBao != null)
        {
            panelBao.SetActive(false);
        }
    }

    // 👉 Gọi khi bấm nút X
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(0); // thay "MainMenu" bằng đúng tên scene của bạn
    }
}
