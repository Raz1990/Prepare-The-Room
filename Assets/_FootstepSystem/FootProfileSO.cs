using System;
using System.Collections.Generic;
using UnityEngine;

// Define our built-in templates
public enum SurfaceTemplate
{
    NewMaterial,
    Grass,
    Stone,
    Wood,
    Sand,
    Gravel,
    Snow,
    Metal,
    WaterMud,
    FabricRug
}

[CreateAssetMenu(fileName = "NewFootProfile", menuName = "Audio/Footstep Profile")]
public class FootProfileSO : ScriptableObject
{
    public string profileName;

    [Header("Material Specifications")]
    public List<SurfaceSpecification> materialSpecifications = new List<SurfaceSpecification>();

    [Serializable]
    public class StepGroup
    {
        public List<AudioClip> softSteps = new List<AudioClip>();
        public List<AudioClip> mediumSteps = new List<AudioClip>();
        public List<AudioClip> hardSteps = new List<AudioClip>();
    }

    [Serializable]
    public class SurfaceSpecification
    {
        // We initialize values here, but Unity's list duplication overrides them.
        // The Editor script below explicitly catches and cleans this up.
        [HideInInspector] public SurfaceTemplate templateSelection = SurfaceTemplate.NewMaterial;
        public string materialName = "";
        public float volumeMultiplier = 1.0f;

        public List<string> similarNames = new List<string>();

        // Dual-Layer support for both bare and shoe.
        [Header("Footwear Variations")]
        public StepGroup shoeSteps = new StepGroup();
        public StepGroup bareFootSteps = new StepGroup();

        [Header("Additional Actions (Optional)")]
        public List<AudioClip> scuffs = new List<AudioClip>();
        public List<AudioClip> jumps = new List<AudioClip>();
        public List<AudioClip> lands = new List<AudioClip>();
    }
}
