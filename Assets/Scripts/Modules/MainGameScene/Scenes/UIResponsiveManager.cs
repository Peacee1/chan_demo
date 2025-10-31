using UnityEngine;
using UnityEngine.UI;

public class UIResponsiveManager : MonoBehaviour
{
    [Header("UI Elements cần chỉnh kích thước")]
    public RectTransform avatarRect;
    public RectTransform[] uiElements; // các phần tử khác (nút, text...)

    [Header("Tùy chỉnh scale")]
    [Range(0.5f, 2f)] public float phoneScale = 1f;
    [Range(0.5f, 2f)] public float tabletScale = 1.3f;

    void Start()
    {
        bool isTablet = IsTablet();
        float scale = isTablet ? tabletScale : phoneScale;

        // Scale cho avatar
        if (avatarRect != null)
        {
            avatarRect.localScale = Vector3.one * scale;
        }

        // Scale các phần tử UI khác
        foreach (RectTransform rect in uiElements)
        {
            if (rect != null)
                rect.localScale = Vector3.one * scale;
        }

        Debug.Log($"[UIResponsiveManager] Thiết bị: {(isTablet ? "iPad/Tablet" : "iPhone/Phone")} | Scale: {scale}");
    }

    /// <summary>
    /// Kiểm tra xem thiết bị có phải là iPad / tablet hay không.
    /// Dựa trên tỉ lệ màn hình và kích thước cạnh ngắn.
    /// </summary>
    bool IsTablet()
    {
        float aspect = (float)Screen.width / Screen.height;
        int minSide = Mathf.Min(Screen.width, Screen.height);

        // iPad có tỉ lệ gần 4:3 và cạnh ngắn thường >= 1000px
        return (minSide >= 1000 && aspect < 1.5f);
    }
}
