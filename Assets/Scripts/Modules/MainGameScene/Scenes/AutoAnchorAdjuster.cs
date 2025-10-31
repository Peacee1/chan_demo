using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class AutoAnchorAdjuster : MonoBehaviour
{
    [Header("Tùy chỉnh scale UI")]
    [Range(0.5f, 2f)] public float phoneScale = 1f;
    [Range(0.5f, 2f)] public float tabletScale = 0.85f; // giảm nhẹ cho iPad

    private RectTransform rect;
    private Vector2 lastResolution;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        AdjustAnchorAndScale();
    }

    void Update()
    {
        // Kiểm tra nếu đổi độ phân giải (trong editor hoặc xoay màn hình)
        if (lastResolution.x != Screen.width || lastResolution.y != Screen.height)
        {
            AdjustAnchorAndScale();
        }
    }

    private void AdjustAnchorAndScale()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (rect == null) return;

        // Nếu chưa set anchor preset (anchorMin == anchorMax), tự đặt theo vị trí hiện tại
        if (rect.anchorMin == rect.anchorMax)
        {
            Vector2 parentSize = rect.parent.GetComponent<RectTransform>().rect.size;
            Vector2 anchoredPos = rect.anchoredPosition;
            Vector2 size = rect.rect.size;

            Vector2 anchorPos = new Vector2(
                (anchoredPos.x + parentSize.x * 0.5f) / parentSize.x,
                (anchoredPos.y + parentSize.y * 0.5f) / parentSize.y
            );

            rect.anchorMin = anchorPos;
            rect.anchorMax = anchorPos;
            rect.anchoredPosition = Vector2.zero; // reset về 0 vì anchor đã giữ vị trí
        }

        // Scale theo loại thiết bị
        bool isTablet = IsTablet();
        float scale = isTablet ? tabletScale : phoneScale;
        rect.localScale = Vector3.one * scale;

        lastResolution = new Vector2(Screen.width, Screen.height);
    }

    private bool IsTablet()
    {
        float aspect = (float)Screen.width / Screen.height;
        int minSide = Mathf.Min(Screen.width, Screen.height);
        // iPad: 4:3, chiều ngắn > 1000px
        return (minSide >= 1000 && aspect < 1.6f);
    }
}
