using UnityEngine;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private GameObject tutorialPopup;
    [SerializeField] private TextMeshProUGUI tutorialText;

    private void Start()
    {
        if (tutorialPopup != null)
            tutorialPopup.SetActive(false);
    }

    // Gọi khi bấm nút Tutorial trong menu
    public void ShowTutorial()
    {
        if (tutorialPopup != null)
        {
            tutorialPopup.SetActive(true);
            if (tutorialText != null)
            {
                tutorialText.text =
                    "LUẬT CHƠI:\n\n" +
                    "- Mỗi lượt có 15s để đánh.\n" +
                    "- Bạn có thể ăn hoặc bỏ qua.\n" +
                    "- Ăn đủ 3 lá của đối phương bạn sẽ được phép ù.\n\n" +
                    "- Bỏ quá 5 lần thì bạn sẽ thua.\n\n" +
                    "- Đối phương đánh ra 6 lá thì bạn sẽ thua.\n\n" +
                    "LUẬT ĂN:\n\n" +
                    "- Có thể ăn những lá giống nhau.\n\n" +
                    "- Có thể ăn những lá giống nhau về số và khác chất.\n\n" +
                    "Chúc bạn chơi vui!";
            }
        }
    }

    // Gọi khi bấm nút Cancel (X)
    public void HideTutorial()
    {
        if (tutorialPopup != null)
            tutorialPopup.SetActive(false);
    }
}
