using UnityEngine;

public class OrientationLocker : MonoBehaviour
{
    public bool isPortrait; // tick chọn trong Inspector nếu scene này dọc

    void Start()
    {
        if (isPortrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
        else
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
    }
}
