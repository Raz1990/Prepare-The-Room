using TMPro;

public interface IPromptable
{
    string GetPromptText();
    TMP_SpriteAsset SpriteAsset { get; }
}
