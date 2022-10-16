#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;
using UnityEditor.SceneManagement;
using System.Security.Cryptography;
using System.Text;

public class WorldExporter : EditorWindow
{
    public string worldName = "WorldExample"; // Name of the world
    public string versionMap = "0.0.1"; // Version of the map
    private bool exportMap = false; // Export the map
    private static bool firstTime = false; // First time

    public static string PathSaveWorld = ""; // Path to save the world
    
    private byte[] bytesEncode; // Bytes encode

    [MenuItem("BNA SDK/World/World Exporter")]
    public static void ShowWindow()
    {
        CheckFolderExist();
        GetWindow(typeof(WorldExporter));
    }

    void OnGUI()
    {
        if (firstTime)
        {
            GUILayout.Space(5.0f);
            GUILayout.Label ("Setup", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5.0f); 
            
            if (GUILayout.Button("Setup project"))
            {
                SetupProject();
            }
        }
        else
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
        
    }
    
    public async void ExportWorld()
    {
        exportMap = true;
        
        // Get PathSaveWorld
        PathSaveWorld = PlayerPrefs.GetString("PathSaveWorld");
        
        // Check if the PathSaveWorld is empty
        if (PathSaveWorld == "")
        {
            ErrorType(3);
            return;
        }

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
        
        await UniTask.Delay(1000);
        
        // Export this scene to a folder
        try
        {
            // check if scene is exist in the folder
            if (File.Exists(PathSaveWorld + "/" + worldName + "_" + versionMap + ".unity"))
            {
                // Delete scene
                File.Delete(PathSaveWorld + "/" + worldName + ".unity");
            }
            
            await UniTask.Delay(500); // Wait 500ms

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), PathSaveWorld + "/" + worldName + "_" + versionMap + ".unity");
            
            Byte[] bytes = File.ReadAllBytes(PathSaveWorld + "/" + worldName + "_" + versionMap + ".unity");
            string file = Convert.ToBase64String(bytes);
            
            // Delete file Unity
            File.Delete(PathSaveWorld + "/" + worldName + "_" + versionMap + ".unity");
            
            // Create file Unity to bnaw
            File.WriteAllText(PathSaveWorld + "/" + worldName + "_" + versionMap + ".bnaw", file);
            
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
            ErrorType(4);
            return;
        }
        
        Debug.Log("Your scene has been successfully exported...");

        exportMap = false;
    }

    public void ErrorType(byte _code)
    {
        exportMap = false;
        
        switch (_code)
        {
            case 4:
                Debug.LogError("Error 4: An error occurred while exporting your map, please try again.");
                break;
            case 3:
                Debug.LogError("Error 3: Please set the path to save your world (Please setup the project again or reopen this window).");
                break;
            case 2:
                Debug.LogError("Error 2: Your scene is not saved!");
                break;
            case 1:
                Debug.LogError("Error 1: You must enter a version of your map.");
                break;
            case 0:
                Debug.LogError("Error 0: You must enter a name for your map.");
                break;
            default:
                Debug.LogWarning("Error code not found.");
                break;
        }
    }

    public static void CheckFolderExist()
    {
        try
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/BNA SDK";
            if (Directory.Exists(path))
            {
                if (PlayerPrefs.HasKey("firstTimeExport"))
                {
                    PlayerPrefs.GetInt("firstTimeExport" , firstTime ? 1 : 0);
                }
                
                string pathExportWorld = path + "/ExportWorld";
                if (!Directory.Exists(pathExportWorld))
                {
                    Directory.CreateDirectory(pathExportWorld);
                }
                
                PathSaveWorld = pathExportWorld;
                
                // save PathSaveWorld
                PlayerPrefs.SetString("PathSaveWorld", PathSaveWorld);
                
                Debug.Log("Folder exist -> " + path);
            }
            else
            {
                PlayerPrefs.SetInt("firstTimeExport", 1);
                firstTime = true;
                Debug.LogWarning("Folder not exist -> " + path + " <- Please setup the project.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void SetupProject()
    {
        // Create folder in documents
        try
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/BNA SDK";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            string pathExportWorld = path + "/ExportWorld";
            if (!Directory.Exists(pathExportWorld))
            {
                Directory.CreateDirectory(pathExportWorld);
            }
            
            firstTime = false;
            PlayerPrefs.SetInt("firstTimeExport" , firstTime ? 1 : 0);
            
            PathSaveWorld = pathExportWorld;
            
            Debug.Log("Setup successfully -> " + path);
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }
}
#endif