#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Cysharp.Threading.Tasks;
using UnityEditor.SceneManagement;
using System.Security.Cryptography;
using System.Text;

public class AvatarExporter : EditorWindow
{
    public string ActualAvatarName = "";
    public string Actualversion = "";
    public GameObject Prefab = null;

    private static bool firstTime = false; // First time

    private bool exportAvatar = false; // Export the Avatar

    public static string PathSaveAvatar = ""; // Path to save the Avatar

    [MenuItem("BNA SDK/Avatar/Avatar Exporter")]
    public static void ShowWindow()
    {
        CheckFolderExist();
        GetWindow(typeof(AvatarExporter));
    }

    void OnGUI()
    {
        if (firstTime)
        {
            GUILayout.Space(5.0f);
            GUILayout.Label("Setup", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5.0f);

            if (GUILayout.Button("Setup project"))
            {
                SetupProject();
            }
        }
        else
        {
            GUILayout.Space(5.0f);
            GUILayout.Label("Menu Exporter", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5.0f);

            Texture banner = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Editor/img/Banniere01.png", typeof(Texture));
            GUILayout.Box(banner, EditorStyles.centeredGreyMiniLabel);

            GUILayout.Space(5.0f);

            GUILayout.Label("Exporting a Avatar is not difficult but simple!\n" +
                            "Above all, it is important to retrieve your avatar information before uploading.", EditorStyles.textArea);

            GUILayout.Space(10.0f);

            if (GUILayout.Button("Get Information of your Avatar"))
            {
                getAvatarInformation();
            }

            GUILayout.Space(5.0f);

            GUILayout.Label("Namer Avatar", EditorStyles.boldLabel);
            ActualAvatarName = EditorGUILayout.TextField("", ActualAvatarName);

            GUILayout.Space(5.0f);

            GUILayout.Label("Version Avatar", EditorStyles.boldLabel);
            Actualversion = EditorGUILayout.TextField("", Actualversion);

            GUILayout.Space(5.0f);

            GUILayout.Label("Prefab", EditorStyles.boldLabel);
            // Prefab = EditorGUILayout.TextField(null, Prefab);
            Prefab = (GameObject)EditorGUILayout.ObjectField("", Prefab, typeof(GameObject), true);

            GUILayout.Space(5.0f);

            if(Prefab)
            {
                if (GUILayout.Button("Export Avatar"))
                {
                    ExportAvatar();
                }
            }
        }
    }

    public async void ExportAvatar()
    {
        exportAvatar = true;

        await UniTask.Delay(1000);

        // Get PathSaveWorld
        PathSaveAvatar = PlayerPrefs.GetString("PathSaveAvatar");

        // Check if the PathSaveWorld is empty
        if (PathSaveAvatar == "")
        {
            ErrorType(1);
            return;
        }

        // Clear console
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);

        Debug.Log("Export Avatar...");

        await UniTask.Delay(1000);

        if(ActualAvatarName.Length == 0)
        {
            ErrorType(2);
            return;
        }

        if(Actualversion.Length == 0)
        {
            ErrorType(3);
            return;
        }

        if(Prefab == null)
        {
            ErrorType(4);
            return;
        }

        if(!Prefab.activeInHierarchy)
        {
            ErrorType(5);
            return;
        }

        Debug.Log($"Avatar name: {ActualAvatarName} | Version: {Actualversion} | Prefab detected: {Prefab.name}");

        await UniTask.Delay(1000);

        Debug.Log("Your avatar is being exported...");

        try
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            ErrorType(7);
            return;
        }

        Debug.Log("Scene saved...");

        await UniTask.Delay(1000);

        Debug.Log("Creation of your prefab...");

        try
        {
             PrefabUtility.SaveAsPrefabAsset(Prefab, $@"Assets\Editor\Export\{ActualAvatarName + Actualversion}.prefab");
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            ErrorType(6);
            return;
        }

        await UniTask.Delay(500);

        Debug.Log("Successful creation of the prefab...");

        await UniTask.Delay(1000);

        try
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            ErrorType(7);
            return;
        }

        Debug.Log("Scene saved...");

        await UniTask.Delay(1000);

        try
        {
            AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
            assetBundleBuild.assetNames = new[] { $@"Assets\Editor\Export\{ActualAvatarName + Actualversion}.prefab" };
            assetBundleBuild.assetBundleName = $"{ActualAvatarName}.bnaa";

            BuildPipeline.BuildAssetBundles($@"{PathSaveAvatar}", new[] { assetBundleBuild }, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

            AssetDatabase.Refresh();
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            ErrorType(8);
            return;
        }

        await UniTask.Delay(2000);

        Debug.Log("Your Avatar has been successfully exported...");
        EditorUtility.DisplayDialog("Success", "Your Avatar has been successfully exported.\n" + PathSaveAvatar, "Nice !");

        exportAvatar = false;

    }


    public void getAvatarInformation()
    {
        foreach (BNAAvatarDescriptor i in Resources.FindObjectsOfTypeAll<BNAAvatarDescriptor>())
        {
            if (i.gameObject.activeSelf && i.enabled && i.gameObject.activeInHierarchy)
            {
                var getComponent = i.gameObject.GetComponent<BNAAvatarDescriptor>();
                Prefab = i.gameObject;
                ActualAvatarName = getComponent.NameOfAvatar;
                Actualversion = getComponent.VersionOfAvatar;
                break;
            }
        }
    }

    public void ErrorType(byte _code)
    {
        exportAvatar = false;

        switch (_code)
        {
            case 8:
                EditorUtility.DisplayDialog("Error", "Error 8: Can't export prefab, please retry.", "Ok");
                Debug.LogError("Error 8: Can't export prefab, please retry.");
                break;
            case 7:
                EditorUtility.DisplayDialog("Error", "Error 7: Can't save map, please retry.", "Ok");
                Debug.LogError("Error 7: Can't save map, please retry.");
                break;
            case 6:
                EditorUtility.DisplayDialog("Error", "Error 6: Calling apply or revert methods on an object which is not part of a Prefab instance is not supported.\n" +
                    "The avatar prefab could not be created.", "Ok");
                Debug.LogError("Error 6: Calling apply or revert methods on an object which is not part of a Prefab instance is not supported.\n" +
                    "The avatar prefab could not be created.");
                break;
            case 5:
                EditorUtility.DisplayDialog("Error", "Error 5: Your prefab is not active, please active and retry.", "Ok");
                Debug.LogError("Error 5: Your prefab is not active, please active and retry.");
                break;
            case 4:
                EditorUtility.DisplayDialog("Error", "Error 4: Your prefab is not setup, please retry.", "Ok");
                Debug.LogError("Error 4: Your prefab is not setup, please retry.");
                break;
            case 3:
                EditorUtility.DisplayDialog("Error", "Error 3: Please put the version of your Avatar.", "Ok");
                Debug.LogWarning("Error 3: Please put the version of your Avatar.");
                break;
            case 2:
                EditorUtility.DisplayDialog("Error", "Error 2: Please put the name of your Avatar.", "Ok");
                Debug.LogWarning("Error 2: Please put the name of your Avatar.");
                break;
            case 1:
                EditorUtility.DisplayDialog("Error", "Error 1: Is not a prefab, please retry.", "Ok");
                Debug.LogError("Error 1: Is not a prefab, please retry.");
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
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BNA SDK";
            if (Directory.Exists(path))
            {
                if (PlayerPrefs.HasKey("firstTimeExportAvatar"))
                {
                    PlayerPrefs.GetInt("firstTimeExportAvatar", firstTime ? 1 : 0);
                }

                string pathExportAvatar = path + @"\ExportAvatar";
                if (!Directory.Exists(pathExportAvatar))
                {
                    Directory.CreateDirectory(pathExportAvatar);
                }

                PathSaveAvatar = pathExportAvatar;

                // save PathSaveWorld
                PlayerPrefs.SetString("PathSaveAvatar", PathSaveAvatar);

                Debug.Log("Folder exist -> " + path);
            }
            else
            {
                PlayerPrefs.SetInt("firstTimeExportAvatar", 1);
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
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\BNA SDK";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string pathExportAvatar = path + @"\ExportAvatar";
            if (!Directory.Exists(pathExportAvatar))
            {
                Directory.CreateDirectory(pathExportAvatar);
            }

            firstTime = false;
            PlayerPrefs.SetInt("firstTimeExportAvatar", firstTime ? 1 : 0);

            PathSaveAvatar = pathExportAvatar;

            PlayerPrefs.SetString("PathSaveAvatar", PathSaveAvatar);

            Debug.Log("Setup successfully -> " + path);
            EditorUtility.DisplayDialog("Info", "Setup successfully \n" + path, "Ok");
        }
        catch (Exception e)
        {
            Debug.LogError("Error: " + e.Message);
        }
    }

}
#endif