using UnityEngine;
using UnityEngine.SceneManagement;

public class UIControllerMainMenu : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject mid_1;
    [SerializeField] private GameObject mid_2;
    [Header("Canvas")]
    [SerializeField] private Canvas canvas_1;
    [SerializeField] private Canvas canvas_2;
    [SerializeField] private Canvas canvas_3;
    public void ShopPannel()
    {
        shopPanel.SetActive(!shopPanel.activeSelf);
    }
    public void KhuChoiBao()
    {
        canvas_1.gameObject.SetActive(false);
        mid_1.SetActive(true);
        mid_2.SetActive(false);
        canvas_3.gameObject.SetActive(true);
    }
    public void ExitKhuChoiBao()
    {
        canvas_3.gameObject.SetActive(false);
        canvas_1.gameObject.SetActive(true);
    }
    public void Canvas3Mid1()
    {
        mid_1.SetActive(false);
        mid_2.SetActive(true);
    }
    public void LoadGamePlay()
    {
        SceneManager.LoadScene(1);
    }
}
