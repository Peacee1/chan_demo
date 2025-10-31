using UnityEngine;

public class BotNameGenerator : MonoBehaviour
{
    private static string[] namePrefixes = new string[]
    {
        "User", "A", "B", "C", "D"
    };

    /// <summary>
    /// Sinh ngẫu nhiên tên bot
    /// </summary>
    public static string GenerateBotName()
    {
        // Chọn ngẫu nhiên 1 prefix
        string prefix = namePrefixes[Random.Range(0, namePrefixes.Length)];

        // Sinh số ngẫu nhiên kèm theo
        int number = Random.Range(100, 9999);

        return prefix + number;
    }
}
