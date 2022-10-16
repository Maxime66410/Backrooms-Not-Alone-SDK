#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;
using UnityEditor.SceneManagement;
using System.Security.Cryptography;
using System.Text;
public class WorldImporter : EditorWindow
{
    
    private static bool firstTime = false; // First time
    private bool importMap = false; // Import the map
    
    public string worldName = "-> WorldExample_0.0.1 <-"; // Name of the world

    
    [MenuItem("BNA SDK/World/World Importer")]
    public static void ShowWindow()
    {
        CheckFolderExist();
        GetWindow(typeof(WorldImporter));
    }
    
    public void ErrorType(byte _code)
    {
        importMap = false;
        
        switch (_code)
        {
            case 1:
                EditorUtility.DisplayDialog("Error", "File can't decode.", "Ok");
                Debug.LogError("Error: File can't decode");
                break;
            default:
                EditorUtility.DisplayDialog("Error", "Error code not found.", "Ok");
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
                
                Debug.Log("Folder exist -> " + path);
            }
            else
            {
                PlayerPrefs.SetInt("firstTimeExport", 1);
                firstTime = true;
                Debug.LogWarning("Folder not exist -> " + path + " <- Please setup the project.");
                EditorUtility.DisplayDialog("Warning", "Folder not exist \n" + path + "\nPlease setup the project.", "Ok");
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

            Debug.Log("Setup successfully -> " + path);
            EditorUtility.DisplayDialog("Info", "Setup successfully \n" + path, "Ok");
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
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
            GUILayout.Label ("Menu Importer", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5.0f); 
        
            Texture banner = (Texture) AssetDatabase.LoadAssetAtPath("Assets/Editor/img/Banniere01.png", typeof(Texture));
            GUILayout.Box(banner, EditorStyles.centeredGreyMiniLabel);
            
            GUILayout.Space(5.0f);
            
            GUILayout.Label("Name of your map", EditorStyles.boldLabel);
            GUILayout.Label(
                "It is important to copy paste the file name without the '.bnaw'.",
                EditorStyles.helpBox);
        
            worldName = EditorGUILayout.TextField("", worldName);
        
            GUILayout.Space(10.0f);
            
            if (!importMap)
            {
                if (GUILayout.Button("Import World"))
                {
                    ImportWorld(); 
                }
            }
        }
    }

    public async void ImportWorld()
    {
        importMap = true;
        
        // check file is exist
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/BNA SDK/ExportWorld/" + worldName + ".bnaw";
        if (File.Exists(path))
        {
            Debug.Log("File exist -> " + path);
        }
        else
        {
            Debug.LogWarning("File not exist -> " + path);
            importMap = false;
            return;
        }
        
        
        // import file into unity
        string pathUnity = "Assets/" + worldName + ".bnaw";
        if (File.Exists(pathUnity))
        {
            Debug.LogWarning("File already exist in unity -> " + pathUnity);
            importMap = false;
            return;
        }
        else
        {
            File.Copy(path, pathUnity);
            Debug.Log("File import in unity -> " + pathUnity);
        }

        try
        {
            StreamReader reader = new StreamReader(pathUnity);
            string line = reader.ReadLine();

            // decode base64 to file unity
            byte[] data = Convert.FromBase64String(line);
            string decodedString = Encoding.UTF8.GetString(data);
        
            await UniTask.Delay(500); // Wait 500ms

            // create file unity
            File.WriteAllText("Assets/" + worldName + ".unity", decodedString);
        
            await UniTask.Delay(500); // Wait 500ms
        }
        catch (Exception e)
        {
            Debug.LogWarning("Error: " + e.Message);
            ErrorType(1);
            return;
        }

        Debug.Log("File decode successfully -> " + pathUnity);
        EditorUtility.DisplayDialog("Success", "File decode successfully \n" + pathUnity, "Nice !");
        importMap = false;
    }
}
#endif