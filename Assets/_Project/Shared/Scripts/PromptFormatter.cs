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
        // 1. Fetch hex colors for input and action
        string inputHex = ColorsCenter.ConvertColorToHexString(inputColor ?? ColorsCenter.Gold);
        string actionHex = ColorsCenter.ConvertColorToHexString(actionColor ?? ColorsCenter.LightGreen);

        // 2. Base action prompt
        string basePrompt = $"Press <color=#{inputHex}><b>[{inputKey}]</b></color> to <color=#{actionHex}><b>{action}</b></color>";

        // 3. Return base prompt directly if no object name is provided
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return basePrompt;
        }

        // 4. Build object clause if objectName exists
        string itemHex = ColorsCenter.ConvertColorToHexString(itemColor ?? ColorsCenter.Teal);
        string spriteTag = icon != null ? $"<sprite name=\"{icon.name}\"> " : string.Empty;

        return $"{basePrompt} the {spriteTag}<color=#{itemHex}><b>{objectName}</b></color>";
    }
}