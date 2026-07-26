using UnityEngine;

public class ColorsCenter
{
    public static Color Gold = new Color(1f, 0.843f, 0f); // Gold (#FFD700)
    public static Color LightGreen = new Color(0.435f, 0.875f, 0.455f); // Brighter Green (#6FDF74)
    public static Color Teal = new Color(0.705f, 1f, 1f); // Bright teal (#B4FFFF)

    public static string ConvertColorToHexString(Color color)
    {
        // Converts Unity Color to 6-digit RGB Hex string for TextMeshPro rich text
        return ColorUtility.ToHtmlStringRGB(color);
    }
}
