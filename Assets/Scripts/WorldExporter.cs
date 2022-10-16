#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;
using UnityEditor.SceneManagement;

public class WorldExporter : EditorWindow
{
    public string worldName = "WorldExample";
    public string versionMap = "0.0.1";
    private bool exportMap = false;

    [MenuItem("BNA SDK/World Exporter")]
    public static void ShowWindow()
    {
        GetWindow(typeof(WorldExporter));
    }

    void OnGUI()
    {
        GUILayout.Space(5.0f);
        GUILayout.Label ("Menu Exporter", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(5.0f); 
        
        Texture banner = (Texture) AssetDatabase.LoadAssetAtPath("Assets/Editor/img/Banniere01.png", typeof(Texture));
        GUILayout.Box(banner, EditorStyles.centeredGreyMiniLabel);
        
        GUILayout.Space(5.0f);
        
        GUILayout.Label("Exporting a map is not difficult but simple!\n" +
                        "Please follow the steps below to export your map successfully.", EditorStyles.textArea);
        
        GUILayout.Space(10.0f);
        
        GUILayout.Label("Set name of your map", EditorStyles.boldLabel);
        GUILayout.Label(
            "It is important to put the name of your map for the export to work, but especially to recognize your map in games.",
            EditorStyles.helpBox);
        
        GUILayout.Space(5.0f);
        
        worldName = EditorGUILayout.TextField("", worldName);
        
        GUILayout.Space(5.0f);
        
        GUILayout.Label("Version of your map", EditorStyles.boldLabel);
        GUILayout.Label(
            "It is important to version your versions of your maps.",
            EditorStyles.helpBox);
        
        GUILayout.Space(5.0f);
        
        versionMap = EditorGUILayout.TextField("", versionMap);
        
        GUILayout.Space(10.0f);

        if (!exportMap)
        {
            if (GUILayout.Button("Export World"))
            {
                ExportWorld(); 
            }
        }
        
    }
    
    public async void ExportWorld()
    {
        exportMap = true;
        
        // Clear console
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
        
        Debug.Log("Exporting world...");
        await UniTask.Delay(1000);
        
        // Check if name exist
        if(worldName == "")
        {
            ErrorType(0);
            return;
        }
        
        
        // Check if version exist
        if(versionMap == "")
        {
            ErrorType(1);
            return;
        }
        
        Debug.Log($"World name: {worldName} | Version: {versionMap}");
        await UniTask.Delay(1000);
        
        Debug.Log("Your world is being exported...");
        
        await UniTask.Delay(1000);
        // Save scene
        try
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
            ErrorType(2);
            return;
        }
        
        Debug.Log("Your scene has been successfully saved...");

        exportMap = false;
    }

    public void ErrorType(byte _code)
    {
        exportMap = false;
        
        switch (_code)
        {
            case 2:
                Debug.LogError("Error 2: Your scene is not saved!");
                break;
            case 1:
                Debug.LogError("Error 1: You must enter a version of your map");
                break;
            case 0:
                Debug.LogError("Error 0: You must enter a name for your map.");
                break;
            default:
                Debug.LogWarning("Error code not found.");
                break;
        }
    }
}
#endif