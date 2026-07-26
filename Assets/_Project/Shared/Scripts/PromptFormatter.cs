using TMPro;
using UnityEngine;

public static class PromptFormatter
{
    /// <summary>
    /// Builds a standardized rich-text prompt for interactables and pickupables
    /// </summary>
    public static string BuildPrompt(
        string inputKey,
        string action,
        string objectName,
        Color? inputColor = null,
        Color? actionColor = null,
        Color? itemColor = null,
        Sprite icon = null
    )
    {
        // 1. Fetch hex colors from ColorsCenter defaults if none provided
        string inputHex = ColorsCenter.ConvertColorToHexString(inputColor ?? ColorsCenter.Gold);
        string actionHex = ColorsCenter.ConvertColorToHexString(actionColor ?? ColorsCenter.LightGreen);
        string itemHex = ColorsCenter.ConvertColorToHexString(itemColor ?? ColorsCenter.Teal);

        // 2. Build sprite tag only if an icon is supplied
        string spriteTag = icon != null ? $"<sprite name=\"{icon.name}\"> " : string.Empty;

        // 3. Assemble the single, unified string format
        return $"Press <color=#{inputHex}><b>[{inputKey}]</b></color> to <color=#{actionHex}><b>{action}</b></color> the {spriteTag}<color=#{itemHex}><b>{objectName}</b></color>";
    }
}