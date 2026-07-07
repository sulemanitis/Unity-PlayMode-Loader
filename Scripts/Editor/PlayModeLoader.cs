#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PlayModeLoader
{
    private static PlayModeLoaderSettings Settings => PlayModeLoaderSettings.instance;

    static PlayModeLoader()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        Debug.Log($"PlayModeLoader startup, sceneGuid loaded as: '{Settings.sceneGuid}'");
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        return new SettingsProvider("Project/Play Mode Loader", SettingsScope.Project)
        {
            label = "Play Mode Loader",

            guiHandler = editorContext =>
            {
                EditorGUILayout.LabelField("Play Mode Loader Settings", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                Settings.enabled = EditorGUILayout.Toggle("Enable Loader", Settings.enabled);
                Settings.saveBeforePlay = EditorGUILayout.Toggle("Save Before Play", Settings.saveBeforePlay);

                EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

                if (buildScenes == null || buildScenes.Length == 0)
                {
                    EditorGUILayout.HelpBox(
                        "No scenes found in Build Settings. Add scenes there first.",
                        MessageType.Warning
                    );
                }
                else
                {
                    string[] sceneNames = new string[buildScenes.Length];
                    int currentIndex = 0;

                    for (int i = 0; i < buildScenes.Length; i++)
                    {
                        string sceneName = Path.GetFileNameWithoutExtension(buildScenes[i].path);
                        sceneNames[i] = $"{i}: {sceneName}";

                        if (buildScenes[i].guid.ToString() == Settings.sceneGuid)
                        {
                            currentIndex = i;
                        }
                    }

                    int selectedIndex = EditorGUILayout.Popup("Loading Scene", currentIndex, sceneNames);
                    string selectedGuid = buildScenes[selectedIndex].guid.ToString();

                    if (selectedGuid != Settings.sceneGuid)
                    {
                        Settings.sceneGuid = selectedGuid;
                        Settings.SaveSettings();
                    }

                    if (!buildScenes[selectedIndex].enabled)
                    {
                        EditorGUILayout.HelpBox(
                            "Selected scene is disabled in Build Settings, but the loader will still use it in the editor.",
                            MessageType.Info
                        );
                    }
                }

                EditorGUILayout.HelpBox(
                    "This setting is stored per project in ProjectSettings/PlayModeLoaderSettings.asset. Scene is referenced by its GUID, so renaming, moving, or reordering the scene in Build Settings is safe.",
                    MessageType.Info
                );

                if (EditorGUI.EndChangeCheck())
                {
                    Settings.SaveSettings();
                }
            }
        };
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!Settings.enabled)
            return;

        if (string.IsNullOrEmpty(Settings.sceneGuid))
            return;

        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

        if (buildScenes == null || buildScenes.Length == 0)
            return;

        string loadingScenePath = null;

        for (int i = 0; i < buildScenes.Length; i++)
        {
            if (buildScenes[i].guid.ToString() == Settings.sceneGuid)
            {
                loadingScenePath = buildScenes[i].path;
                break;
            }
        }

        if (string.IsNullOrEmpty(loadingScenePath))
            return;

        if (!File.Exists(loadingScenePath))
            return;

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            if (Settings.saveBeforePlay)
            {
                bool saved = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                if (!saved)
                {
                    EditorApplication.isPlaying = false;
                    return;
                }
            }

            SaveCurrentlyOpenScenes();

            EditorSceneManager.OpenScene(loadingScenePath, OpenSceneMode.Single);
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            RestorePreviousScenes();
        }
    }

    private static void SaveCurrentlyOpenScenes()
    {
        Settings.previousScenePaths.Clear();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)
                continue;

            if (string.IsNullOrEmpty(scene.path))
                continue;

            Settings.previousScenePaths.Add(scene.path);
        }

        Settings.SaveSettings();
    }

    private static void RestorePreviousScenes()
    {
        if (Settings.previousScenePaths == null)
            return;

        if (Settings.previousScenePaths.Count == 0)
            return;

        bool openedFirstScene = false;

        for (int i = 0; i < Settings.previousScenePaths.Count; i++)
        {
            string scenePath = Settings.previousScenePaths[i];

            if (string.IsNullOrEmpty(scenePath))
                continue;

            if (!File.Exists(scenePath))
                continue;

            if (!openedFirstScene)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                openedFirstScene = true;
            }
            else
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
        }
    }

    [MenuItem("Edit/Play Mode Loader/Open Project Settings")]
    private static void OpenProjectSettings()
    {
        SettingsService.OpenProjectSettings("Project/Play Mode Loader");
    }
}

[FilePath("ProjectSettings/PlayModeLoaderSettings.asset", FilePathAttribute.Location.ProjectFolder)]
public class PlayModeLoaderSettings : ScriptableSingleton<PlayModeLoaderSettings>
{
    public bool enabled = true;
    public bool saveBeforePlay = true;
    public string sceneGuid = "";

    public List<string> previousScenePaths = new();

    private void OnEnable()
    {
        Debug.Log($"PlayModeLoaderSettings.OnEnable, sceneGuid: '{sceneGuid}'");
    }

    public void SaveSettings()
    {
        Save(true);
    }
}

#endif