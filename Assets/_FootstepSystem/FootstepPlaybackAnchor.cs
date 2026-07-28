using UnityEngine;

[RequireComponent(typeof(AudioSource))]
// This line injects the script right into Unity's "Add Component" switchboard!
[AddComponentMenu("Raz's Footsteps System/Footstep Playback Anchor")]
public class FootstepPlaybackAnchor : MonoBehaviour
{
    [Header("Data Profile Connection")]
    [SerializeField] private FootProfileSO activeFootProfile;

    [Header("Runtime State Configuration")]
    [SerializeField] private bool isWearingShoes = true;
    [SerializeField] private float raycastDistance = 1f;
    [SerializeField] private LayerMask groundLayers = ~0; // Default to hit everything

    [Tooltip("The width footprint of the foot check. Larger spheres catch steep slopes and uneven terrain much easier.")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float footRadius = 0.15f;

    private AudioSource localAudioSource;
    private const float verticalPadding = 0.05f;

    private void Awake()
    {
        localAudioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Unity automatically executes this method when the component is first added 
    /// via the Inspector's "Add Component" menu. 
    /// </summary>
    private void Reset()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // AUTOMATIC UX PRESETS: Force the audio source to act as a 3D sound right away
            audioSource.spatialBlend = 1.0f; // 1.0f is pure 3D spatialized audio
            audioSource.playOnAwake = false;
            audioSource.loop = false;

            // Give it a natural volume rolloff curve so it doesn't blast loudly across the map
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
        }
    }

    /// <summary>
    /// Call this function via your Animation Events or movement script!
    /// Pass 0 for Soft, 1 for Medium, 2 for Hard.
    /// </summary>
    public void TriggerFootstepEvent(int intensityIndex)
    {
        if (activeFootProfile == null) return;

        // 1. Dynamic Origin Lift Calculation:
        // We lift the center of the sphere by its own radius, plus a tiny 0.05m padding buffer.
        // This guarantees the bottom of the sphere always starts exactly 5cm above the capsule's base!
        Vector3 origin = transform.position + Vector3.up * (footRadius + verticalPadding); // Lift slightly higher to account for radius
        Vector3 direction = Vector3.down;

        // 2. Perform a SphereCast instead of a regular Raycast
        if (Physics.SphereCast(origin, footRadius, direction, out RaycastHit hit, raycastDistance, groundLayers))
        {
            // Optional Safety: Ensure we aren't detecting walls if we scrape against a vertical cliff side
            if (hit.normal.y > 0.4f)
            {
                string detectedSurfaceName = GetSurfaceNameFromHit(hit);
                if (!string.IsNullOrEmpty(detectedSurfaceName))
                {
                    PlayMatchingFootstep(detectedSurfaceName, intensityIndex);
                }
            }
        }
    }

    private Vector3 GetSphereStartOrigin()
    {
        return transform.position + Vector3.up * (footRadius + verticalPadding);
    }

    private string GetSurfaceNameFromHit(RaycastHit hit)
    {
        // PRIORITY 1: Physics Material (The deliberate gameplay override)
        // Check if a specific Physics Material is assigned to the collider
        if (hit.collider != null && hit.collider.sharedMaterial != null)
        {
            string physMatName = hit.collider.sharedMaterial.name;
            return physMatName; // Returns the clean name of your Physics Material asset
        }

        // PRIORITY 2: Visual Material Fallback
        // If no physics material exists and it's a standard 3D mesh, fall back to the renderer's material name
        if (hit.collider.TryGetComponent<Renderer>(out var renderer))
        {
            if (renderer.sharedMaterial != null)
            {
                string visualMatName = renderer.sharedMaterial.name;
                return visualMatName;
            }
        }

        // PRIORITY 3: Terrain Texture Indexing
        // If it's a terrain, we want to look at its painted texture weights instead of visual/phys mats
        if (hit.collider.TryGetComponent<Terrain>(out var terrain))
        {
            return GetTerrainTextureName(terrain, hit.point);
        }

        // Ultimate safety baseline if the raycast hits a completely naked/empty collider
        return "Default";
    }

    private string GetTerrainTextureName(Terrain terrain, Vector3 worldPos)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        // Calculate coordinates relative to the terrain grid sizing maps
        int mapX = Mathf.RoundToInt((worldPos.x - terrainPos.x) / terrainData.size.x * terrainData.alphamapWidth);
        int mapZ = Mathf.RoundToInt((worldPos.z - terrainPos.z) / terrainData.size.z * terrainData.alphamapHeight);

        // Bound-check parameters safely
        if (mapX < 0 || mapX >= terrainData.alphamapWidth || mapZ < 0 || mapZ >= terrainData.alphamapHeight)
            return "";

        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float primaryMixValue = 0f;
        int dominantTextureIndex = 0;

        // Loop across terrain layer compositions to uncover the dominant blend layer at this coordinate
        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            if (splatmapData[0, 0, i] > primaryMixValue)
            {
                primaryMixValue = splatmapData[0, 0, i];
                dominantTextureIndex = i;
            }
        }

        if (terrainData.terrainLayers.Length > dominantTextureIndex)
        {
            return terrainData.terrainLayers[dominantTextureIndex].name.ToLower();
        }

        return "";
    }

    private void PlayMatchingFootstep(string surfaceName, int intensityIndex)
    {
        // Search through specifications inside our custom automation profile asset
        foreach (var spec in activeFootProfile.materialSpecifications)
        {
            foreach (string searchKey in spec.similarNames)
            {
                if (
                    !string.IsNullOrEmpty(searchKey) &&
                    // If our detected asset name contains any of the search words (e.g. "grass")
                    // Pure case-insensitive comparison that is highly optimized for performance
                    surfaceName.Contains(searchKey, System.StringComparison.OrdinalIgnoreCase)
                )
                {
                    // Snag the correct Shoe vs. Bare group allocation
                    FootProfileSO.StepGroup selectedGroup = isWearingShoes ? spec.shoeSteps : spec.bareFootSteps;

                    // Pull the correct intensity array list
                    System.Collections.Generic.List<AudioClip> targetClips = intensityIndex switch
                    {
                        0 => selectedGroup.softSteps,
                        2 => selectedGroup.hardSteps,
                        _ => selectedGroup.mediumSteps // 1 or fallback defaults to Medium
                    };

                    if (targetClips != null && targetClips.Count > 0)
                    {
                        AudioClip clipToPlay = targetClips[Random.Range(0, targetClips.Count)];

                        if (localAudioSource == null) localAudioSource = GetComponent<AudioSource>();

                        localAudioSource.PlayOneShot(clipToPlay, spec.volumeMultiplier);
                        return;
                    }
                }
            }




            foreach (string searchKey in spec.similarNames)
            {
                // If our detected asset name contains any of the search words (e.g. "grass")
                if (surfaceName.Contains(searchKey.ToLower()))
                {
                    // Snag the correct Shoe vs. Bare group allocation
                    FootProfileSO.StepGroup selectedGroup = isWearingShoes ? spec.shoeSteps : spec.bareFootSteps;

                    // Pull the correct intensity array list
                    System.Collections.Generic.List<AudioClip> targetClips = intensityIndex switch
                    {
                        0 => selectedGroup.softSteps,
                        2 => selectedGroup.hardSteps,
                        _ => selectedGroup.mediumSteps // 1 or fallback defaults to Medium
                    };

                    if (targetClips != null && targetClips.Count > 0)
                    {
                        // Play a random clip from the matching array pool
                        AudioClip clipToPlay = targetClips[Random.Range(0, targetClips.Count)];

                        // Scale volume dynamic parameters based on specification multipliers
                        localAudioSource.PlayOneShot(clipToPlay, spec.volumeMultiplier);
                        return;
                    }
                }
            }
        }
    }

    // Quick API toggle method for armor/equipment swap adjustments dynamically at runtime
    public void SetFootwearState(bool wearShoes) => isWearingShoes = wearShoes;

    // ==========================================
    // DIAGNOSTIC PLAY-MODE GIZMOS LOGIC
    // ==========================================
    private void OnDrawGizmos()
    {
        // Strictly exit if the engine is not running in Play Mode
        if (!Application.isPlaying) return;

        // Calculate start and end centers using our dynamic math setup
        Vector3 startCenter = GetSphereStartOrigin();
        Vector3 endCenter = startCenter + (Vector3.down * raycastDistance);

        // Check if it's currently hitting anything in real-time to change colors dynamically
        bool isGrounded = Physics.SphereCast(startCenter, footRadius, Vector3.down, out RaycastHit hit, raycastDistance, groundLayers);

        // Visual feedback color styling
        Gizmos.color = isGrounded ? Color.blue : Color.red;

        // Draw the top tracking sphere boundary
        Gizmos.DrawWireSphere(startCenter, footRadius);

        // Draw the terminal tracking depth boundary
        Gizmos.DrawWireSphere(endCenter, footRadius);

        // Draw linking structural lines to generate a clean capsule silhouette path
        Gizmos.DrawLine(startCenter + Vector3.left * footRadius, endCenter + Vector3.left * footRadius);
        Gizmos.DrawLine(startCenter + Vector3.right * footRadius, endCenter + Vector3.right * footRadius);
        Gizmos.DrawLine(startCenter + Vector3.forward * footRadius, endCenter + Vector3.forward * footRadius);
        Gizmos.DrawLine(startCenter + Vector3.back * footRadius, endCenter + Vector3.back * footRadius);

        // If it is hitting ground surfaces, draw an explicit marker point right on the impact pixel!
        if (isGrounded)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(hit.point, 0.04f);
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.4f); // Draws a line indicating the slope angle normal!
        }
    }
}