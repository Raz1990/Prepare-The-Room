using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float range = 1f;

    [Header("UI Prompt Colors")]
    [SerializeField] private Color inputColor = ColorsCenter.Gold;
    [SerializeField] private Color actionColor = ColorsCenter.LightGreen;
    [SerializeField] private Color objectColor = ColorsCenter.Teal;

    [Header("Light References")]
    [Tooltip("Drag the 'Light_Group' GameObject here")]
    [SerializeField] private Transform lightGroup;

    [Header("Audio")]
    [SerializeField] private AudioClip switchSFX;

    private bool isLightsOn = true;
    private Light[] roomLights;
    private List<GameObject> lightOnVisualMeshes = new List<GameObject>();

    public TMP_SpriteAsset SpriteAsset => null;
    public float InteractionRange => range;
    public ItemType RequiredItem => ItemType.None;

    private void Awake()
    {
        CacheLightReferences();
    }

    private void CacheLightReferences()
    {
        if (lightGroup == null)
        {
            Debug.LogWarning($"[LightSwitch] Light Group reference is missing on '{gameObject.name}'!");
            return;
        }

        // 1. Automatically fetch all Light components nested in child fixtures
        roomLights = lightGroup.GetComponentsInChildren<Light>(true);

        // 2. Fetch the 'light_ON' visual mesh objects so the glowing strips turn off too
        foreach (Transform child in lightGroup.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Equals("light_ON", System.StringComparison.OrdinalIgnoreCase))
            {
                lightOnVisualMeshes.Add(child.gameObject);
            }
        }
    }

    public string GetPromptText()
    {
        string actionText = isLightsOn ? "Turn off" : "Turn on";
        return PromptFormatter.BuildPrompt("LMB", actionText, "Lights", inputColor, actionColor, objectColor);
    }

    public bool CanInteract()
    {
        // Light switch can always be toggled back and forth endlessly
        return true;
    }

    public void Interact(ItemType _)
    {
        ToggleLights();
    }

    private void ToggleLights()
    {
        isLightsOn = !isLightsOn;

        // Toggle all 12 Point Light components
        if (roomLights != null)
        {
            foreach (Light lightComp in roomLights)
            {
                if (lightComp != null)
                {
                    lightComp.enabled = isLightsOn;
                }
            }
        }

        // Toggle visual glowing mesh strips (light_ON)
        foreach (GameObject visualMesh in lightOnVisualMeshes)
        {
            if (visualMesh != null)
            {
                visualMesh.SetActive(isLightsOn);
            }
        }

        // Play switch toggle SFX
        if (switchSFX != null)
        {
            AudioManager.TriggerPlaySFX(switchSFX);
        }
    }
}