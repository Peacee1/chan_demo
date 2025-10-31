using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaAnchor : MonoBehaviour
{
    public enum AnchorEdge { Top, Bottom, Left, Right }
    public AnchorEdge anchorEdge = AnchorEdge.Top;

    public float offset = 50f; // Khoảng cách cách so với safe area (tính bằng pixel)

    void Start()
    {
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        RectTransform rectTransform = GetComponent<RectTransform>();

        Vector2 anchorMin = rectTransform.anchorMin;
        Vector2 anchorMax = rectTransform.anchorMax;
        Vector2 anchoredPosition = rectTransform.anchoredPosition;

        switch (anchorEdge)
        {
            case AnchorEdge.Top:
                float topOffset = (Screen.height - (safeArea.y + safeArea.height));
                anchoredPosition.y = -topOffset - offset;
                break;

            case AnchorEdge.Bottom:
                float bottomOffset = safeArea.y;
                anchoredPosition.y = bottomOffset + offset;
                break;

            case AnchorEdge.Left:
                float leftOffset = safeArea.x;
                anchoredPosition.x = leftOffset + offset;
                break;

            case AnchorEdge.Right:
                float rightOffset = Screen.width - (safeArea.x + safeArea.width);
                anchoredPosition.x = -rightOffset - offset;
                break;
        }

        rectTransform.anchoredPosition = anchoredPosition;
    }
}
