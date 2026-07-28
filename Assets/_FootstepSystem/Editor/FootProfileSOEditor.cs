#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(FootProfileSO))]
public class FootProfileSOEditor : Editor
{
    private SerializedProperty profileNameProp;
    private SerializedProperty listProp;
    private ReorderableList reorderableList;

    private void OnEnable()
    {
        profileNameProp = serializedObject.FindProperty("profileName");
        listProp = serializedObject.FindProperty("materialSpecifications");

        reorderableList = new ReorderableList(serializedObject, listProp, true, true, true, true);

        reorderableList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Material Specifications Template System");
        };

        reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            SerializedProperty element = listProp.GetArrayElementAtIndex(index);

            SerializedProperty templateProp = element.FindPropertyRelative("templateSelection");
            SerializedProperty nameProp = element.FindPropertyRelative("materialName");
            SerializedProperty volumeProp = element.FindPropertyRelative("volumeMultiplier");
            SerializedProperty similarNamesProp = element.FindPropertyRelative("similarNames");

            string headerTitle = string.IsNullOrEmpty(nameProp.stringValue) ? "New Material Specification" : nameProp.stringValue;

            element.isExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.isExpanded, headerTitle, true);

            if (element.isExpanded)
            {
                EditorGUI.indentLevel++;
                float yOffset = rect.y + EditorGUIUtility.singleLineHeight + 2;

                SurfaceTemplate currentTemplate = (SurfaceTemplate)templateProp.enumValueIndex;
                SurfaceTemplate selectedTemplate = (SurfaceTemplate)EditorGUI.EnumPopup(new Rect(rect.x, yOffset, rect.width, EditorGUIUtility.singleLineHeight), "Base Template", currentTemplate);
                yOffset += EditorGUIUtility.singleLineHeight + 2;

                if (selectedTemplate != currentTemplate)
                {
                    templateProp.enumValueIndex = (int)selectedTemplate;
                    ApplyTemplateValues(selectedTemplate, nameProp, similarNamesProp, volumeProp);
                }

                EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, EditorGUIUtility.singleLineHeight), nameProp);
                yOffset += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, EditorGUIUtility.singleLineHeight), volumeProp);
                yOffset += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, EditorGUIUtility.singleLineHeight), similarNamesProp, true);
                yOffset += EditorGUI.GetPropertyHeight(similarNamesProp, true) + 2;

                // Footwear structures
                SerializedProperty shoeProp = element.FindPropertyRelative("shoeSteps");
                EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, EditorGUIUtility.singleLineHeight), shoeProp, true);
                yOffset += EditorGUI.GetPropertyHeight(shoeProp, true) + 2;

                SerializedProperty bareProp = element.FindPropertyRelative("bareFootSteps");
                EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, EditorGUIUtility.singleLineHeight), bareProp, true);
                yOffset += EditorGUI.GetPropertyHeight(bareProp, true) + 2;

                // Auxiliary action fields
                string[] extraActions = { "scuffs", "jumps", "lands" };
                foreach (string type in extraActions)
                {
                    SerializedProperty p = element.FindPropertyRelative(type);
                    EditorGUI.PropertyField(new Rect(rect.x, yOffset, rect.width, EditorGUIUtility.singleLineHeight), p, true);
                    yOffset += EditorGUI.GetPropertyHeight(p, true) + 2;
                }

                EditorGUI.indentLevel--;
            }
        };

        reorderableList.elementHeightCallback = (int index) => {
            SerializedProperty element = listProp.GetArrayElementAtIndex(index);
            if (!element.isExpanded) return EditorGUIUtility.singleLineHeight + 4;

            float totalHeight = EditorGUIUtility.singleLineHeight * 4 + 14;
            totalHeight += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("similarNames"), true) + 2;
            totalHeight += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("shoeSteps"), true) + 2;
            totalHeight += EditorGUI.GetPropertyHeight(element.FindPropertyRelative("bareFootSteps"), true) + 2;

            string[] extraActions = { "scuffs", "jumps", "lands" };
            foreach (string type in extraActions)
            {
                totalHeight += EditorGUI.GetPropertyHeight(element.FindPropertyRelative(type), true) + 2;
            }

            return totalHeight;
        };

        reorderableList.onAddCallback = (ReorderableList list) => {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(index);
            SerializedProperty newElement = list.serializedProperty.GetArrayElementAtIndex(index);

            newElement.FindPropertyRelative("templateSelection").enumValueIndex = (int)SurfaceTemplate.NewMaterial;
            newElement.FindPropertyRelative("materialName").stringValue = "";
            newElement.FindPropertyRelative("volumeMultiplier").floatValue = 1.0f;
            newElement.FindPropertyRelative("similarNames").ClearArray();

            WipeStepGroup(newElement.FindPropertyRelative("shoeSteps"));
            WipeStepGroup(newElement.FindPropertyRelative("bareFootSteps"));

            newElement.FindPropertyRelative("scuffs").ClearArray();
            newElement.FindPropertyRelative("jumps").ClearArray();
            newElement.FindPropertyRelative("lands").ClearArray();

            newElement.isExpanded = true;
        };
    }

    private void WipeStepGroup(SerializedProperty groupProp)
    {
        groupProp.FindPropertyRelative("softSteps").ClearArray();
        groupProp.FindPropertyRelative("mediumSteps").ClearArray();
        groupProp.FindPropertyRelative("hardSteps").ClearArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(profileNameProp);
        EditorGUILayout.Space(10);
        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private static void ApplyTemplateValues(SurfaceTemplate template, SerializedProperty nameProp, SerializedProperty stringsProp, SerializedProperty volumeProp)
    {
        stringsProp.ClearArray();
        volumeProp.floatValue = 1.0f;

        string targetName = "";
        string[] collection = null;

        switch (template)
        {
            case SurfaceTemplate.Grass:
                targetName = "Grass/Dirt";
                collection = new string[] { "Grass", "Dirt", "Lawn", "Field", "Meadow", "Ground", "Soil" };
                break;
            case SurfaceTemplate.Stone:
                targetName = "Stone/Concrete";
                collection = new string[] { "Stone", "Concrete", "Rock", "Pavement", "Brick", "Cliff", "Cement" };
                break;
            case SurfaceTemplate.Wood:
                targetName = "Wood";
                collection = new string[] { "Wood", "Plank", "Floorboard", "Deck", "Log", "Cabin" };
                break;
            case SurfaceTemplate.Sand:
                targetName = "Sand";
                collection = new string[] { "Sand", "Beach", "Desert", "Dune" };
                break;
            case SurfaceTemplate.Gravel:
                targetName = "Gravel";
                collection = new string[] { "Gravel", "Pebbles", "Path", "Shingle" };
                break;
            case SurfaceTemplate.Snow:
                targetName = "Snow";
                collection = new string[] { "Snow", "Ice", "Frost", "Glacier", "Frozen" };
                break;
            case SurfaceTemplate.Metal:
                targetName = "Metal";
                collection = new string[] { "Metal", "Iron", "Steel", "Grate", "Vent", "Pipe", "Platform" };
                break;
            case SurfaceTemplate.WaterMud:
                targetName = "Water/Mud";
                collection = new string[] { "Water", "Pond", "Lake", "River", "Stream", "Ocean", "Sea", "Pool", "Marsh", "Mud", "Swamp", "Bog", "Mire", "Sludge", "Wet" };
                break;
            case SurfaceTemplate.FabricRug:
                targetName = "Fabric/Rug";
                collection = new string[] { "Fabric", "Rug", "Carpet", "Mat", "Cloth" };
                break;
            default:
                targetName = "";
                break;
        }

        nameProp.stringValue = targetName;

        if (collection != null)
        {
            for (int i = 0; i < collection.Length; i++)
            {
                stringsProp.InsertArrayElementAtIndex(i);
                stringsProp.GetArrayElementAtIndex(i).stringValue = collection[i];
            }
        }
    }

    // --- AUTOMATIC PACKAGING FACTORY ENGINE ---
    [MenuItem("Tools/Footstep System/Generate Master Profile")]
    public static void GenerateMasterProfile()
    {
        FootProfileSO masterProfile = ScriptableObject.CreateInstance<FootProfileSO>();
        masterProfile.profileName = "Master Pre-made Profile";

        string audioRootFolder = "Assets/_FootstepSystem/SampleAudio";

        System.Array templates = System.Enum.GetValues(typeof(SurfaceTemplate));
        foreach (SurfaceTemplate template in templates)
        {
            if (template == SurfaceTemplate.NewMaterial) continue;

            FootProfileSO.SurfaceSpecification spec = new FootProfileSO.SurfaceSpecification();

            // Re-apply basic profile strings
            SerializedObject tempObj = new SerializedObject(masterProfile);
            ApplyTemplateValues(template, tempObj.FindProperty("profileName"), tempObj.FindProperty("profileName"), tempObj.FindProperty("profileName"));

            // Re-map localized fields manually based on enum values
            spec.templateSelection = template;
            spec.volumeMultiplier = 1.0f;

            // Map our enums directly to the physical folder name variations shown in your screenshots
            string folderName = template.ToString();
            if (template == SurfaceTemplate.WaterMud) folderName = "Water or Mud";
            if (template == SurfaceTemplate.FabricRug) folderName = "Rug";

            // Fill default string match lists
            InitializeSpecStrings(template, spec);

            string targetFolder = $"{audioRootFolder}/{folderName}";

            if (Directory.Exists(targetFolder))
            {
                // Find all raw audio clip assets residing directly within this surface's specific folder
                string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { targetFolder });

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);

                    if (clip != null)
                    {
                        string nameLower = clip.name.ToLower();

                        // 1. Identify Footwear Style Layer
                        FootProfileSO.StepGroup targetGroup = spec.shoeSteps; // Default fallback
                        if (nameLower.Contains("bare"))
                        {
                            targetGroup = spec.bareFootSteps;
                        }
                        else if (nameLower.Contains("shoe"))
                        {
                            targetGroup = spec.shoeSteps;
                        }

                        // 2. Identify Step Intensity Layer
                        if (nameLower.Contains("soft")) targetGroup.softSteps.Add(clip);
                        else if (nameLower.Contains("medium")) targetGroup.mediumSteps.Add(clip);
                        else if (nameLower.Contains("hard")) targetGroup.hardSteps.Add(clip);
                    }
                }
            }

            masterProfile.materialSpecifications.Add(spec);
        }

        string savePath = "Assets/_FootstepSystem/MasterFootProfile.asset";
        AssetDatabase.CreateAsset(masterProfile, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = masterProfile;

        Debug.Log($"<color=cyan><b>[Footstep Automation]:</b> Generation complete! Built double-layer profiles across all folders inside {savePath}</color>");
    }

    private static void InitializeSpecStrings(SurfaceTemplate template, FootProfileSO.SurfaceSpecification spec)
    {
        switch (template)
        {
            case SurfaceTemplate.Grass:
                spec.materialName = "Grass/Dirt";
                spec.similarNames.AddRange(new string[] { "Grass", "Dirt", "Lawn", "Field", "Meadow", "Ground", "Soil" });
                break;
            case SurfaceTemplate.Stone:
                spec.materialName = "Stone/Concrete";
                spec.similarNames.AddRange(new string[] { "Stone", "Concrete", "Rock", "Pavement", "Brick", "Cliff", "Cement" });
                break;
            case SurfaceTemplate.Wood:
                spec.materialName = "Wood";
                spec.similarNames.AddRange(new string[] { "Wood", "Plank", "Floorboard", "Deck", "Log", "Cabin" });
                break;
            case SurfaceTemplate.Sand:
                spec.materialName = "Sand";
                spec.similarNames.AddRange(new string[] { "Sand", "Beach", "Desert", "Dune" });
                break;
            case SurfaceTemplate.Gravel:
                spec.materialName = "Gravel";
                spec.similarNames.AddRange(new string[] { "Gravel", "Pebbles", "Path", "Shingle" });
                break;
            case SurfaceTemplate.Snow:
                spec.materialName = "Snow";
                spec.similarNames.AddRange(new string[] { "Snow", "Ice", "Frost", "Glacier", "Frozen" });
                break;
            case SurfaceTemplate.Metal:
                spec.materialName = "Metal";
                spec.similarNames.AddRange(new string[] { "Metal", "Iron", "Steel", "Grate", "Vent", "Pipe", "Platform" });
                break;
            case SurfaceTemplate.WaterMud:
                spec.materialName = "Water/Mud";
                spec.similarNames.AddRange(new string[] { "Water", "Pond", "Lake", "River", "Stream", "Ocean", "Sea", "Pool", "Marsh", "Mud", "Swamp", "Bog", "Mire", "Sludge", "Wet" });
                break;
            case SurfaceTemplate.FabricRug:
                spec.materialName = "Fabric/Rug";
                spec.similarNames.AddRange(new string[] { "Fabric", "Rug", "Carpet", "Mat", "Cloth" });
                break;
        }
    }
}
#endif