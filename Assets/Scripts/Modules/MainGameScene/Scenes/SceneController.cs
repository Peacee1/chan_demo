using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void PlayGame()
    {
        // Chuyển sang scene Gameplay (1)
        SceneManager.LoadScene(1);
    }

    public void BackToMenu()
    {
        // Quay lại scene Menu (0)
        SceneManager.LoadScene(0);
    }

    // Khi scene được load, tự động đổi hướng màn hình phù hợp
    private void OnEnable()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex == 0)
        {
            // Menu nằm ngang
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        else if (sceneIndex == 1)
        {
            // Gameplay nằm dọc
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}
