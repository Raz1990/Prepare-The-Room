#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AudioLoadInBackgroundAutomator : EditorWindow
{
    [MenuItem("Tools/Raz's Audio Optimizer/Optimize Selected Audio Folders")]
    public static void OptimizeSelectedAudio()
    {
        string[] selectedGuids = Selection.assetGUIDs;

        if (selectedGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Audio Automator", "Please select an audio folder or audio assets in the Project window first!", "OK");
            return;
        }

        // Use a HashSet to gather unique file paths and easily prevent any duplicate processing
        HashSet<string> candidatePaths = new HashSet<string>();

        foreach (string guid in selectedGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    if (!file.EndsWith(".meta")) candidatePaths.Add(file);
                }
            }
            else if (!path.EndsWith(".meta"))
            {
                candidatePaths.Add(path);
            }
        }

        // THE AUDIT LAYER: Filter down to ONLY files that actually need optimization
        List<string> unoptimizedFilePaths = new List<string>();

        foreach (string filePath in candidatePaths)
        {
            // Only add to our work order if the file is a valid audio clip AND is unoptimized
            if (IsAudioClipValid(filePath) && !IsAudioClipOptimized(filePath))
            {
                unoptimizedFilePaths.Add(filePath);
            }
        }

        int totalFilesToOptimize = unoptimizedFilePaths.Count;

        // SMART CHECK: If everything is already perfect, tell the user and exit!
        if (totalFilesToOptimize == 0)
        {
            EditorUtility.DisplayDialog("Audio Automator", "No audio files to optimize.\n\nAll selected assets are already fully optimized!", "OK");
            return;
        }

        // UX SAFETY LAYER: If the volume of unoptimized files is high, warn the user
        if (totalFilesToOptimize > 50)
        {
            string message = $"You are about to optimize {totalFilesToOptimize} audio files.\n\nThis will re-import the assets and may take a moment. Do you want to proceed?";
            bool proceed = EditorUtility.DisplayDialog("Warning: Large Batch Optimization", message, "Proceed", "Cancel");

            if (!proceed)
            {
                Debug.Log("Audio batch optimization cancelled by user.");
                return;
            }
        }

        // Proceed with the optimization engine loop on the filtered list
        int processedCount = 0;

        foreach (string filePath in unoptimizedFilePaths)
        {
            if (OptimizeAudioClip(filePath))
            {
                processedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success!", $"Successfully optimized {processedCount} footstep audio files!", "Awesome");
    }

    /// <summary>
    /// Simply checks if the asset at the path is actually an audio file Unity can read.
    /// </summary>
    private static bool IsAudioClipValid(string assetPath)
    {
        return AssetImporter.GetAtPath(assetPath) is AudioImporter;
    }

    /// <summary>
    /// Audits the audio file's current settings against our target performance profile.
    /// Returns true ONLY if BOTH settings match our optimization rules.
    /// </summary>
    private static bool IsAudioClipOptimized(string assetPath)
    {
        AudioImporter audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;
        if (audioImporter == null) return true; // Treat invalid files as "optimized" so we skip them

        // Condition 1: Must load in background
        bool hasLoadInBackground = audioImporter.loadInBackground;

        // Condition 2: Must be set to Compressed In Memory
        bool hasCorrectLoadType = audioImporter.defaultSampleSettings.loadType == AudioClipLoadType.CompressedInMemory;

        // It's only optimized if it ticks both boxes perfectly
        return hasLoadInBackground && hasCorrectLoadType;
    }

    private static bool OptimizeAudioClip(string assetPath)
    {
        AudioImporter audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;
        if (audioImporter == null) return false;

        bool modified = false;

        if (!audioImporter.loadInBackground)
        {
            audioImporter.loadInBackground = true;
            modified = true;
        }

        AudioImporterSampleSettings defaultSettings = audioImporter.defaultSampleSettings;

        if (defaultSettings.loadType != AudioClipLoadType.CompressedInMemory)
        {
            defaultSettings.loadType = AudioClipLoadType.CompressedInMemory;
            modified = true;
        }

        if (modified)
        {
            audioImporter.defaultSampleSettings = defaultSettings;
            EditorUtility.SetDirty(audioImporter);
            audioImporter.SaveAndReimport();
            return true;
        }

        return false;
    }
}
#endif