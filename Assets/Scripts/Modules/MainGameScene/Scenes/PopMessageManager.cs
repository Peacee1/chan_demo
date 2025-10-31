using UnityEngine;
using TMPro;
using System.Collections;

public class PopMessageManager : MonoBehaviour
{
    public static PopMessageManager Instance;

    [Header("UI Components")]
    public GameObject messagePanel;   // Panel chứa text (ẩn mặc định)
    public TMP_Text messageText;      // Text hiển thị thông báo

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        messagePanel.SetActive(false); // Ẩn panel khi start
    }

    /// <summary>
    /// Hiện message lên panel trong vài giây
    /// </summary>
    public void Show(string message, float duration = 2f)
    {
        StopAllCoroutines();
        messageText.text = message;
        messagePanel.SetActive(true);
        StartCoroutine(HideAfterDelay(duration));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        messagePanel.SetActive(false);
    }
}
