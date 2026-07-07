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
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        return new SettingsProvider("Project/Play Mode Loader", SettingsScope.Project)
        {
            label = "Play Mode Loader",

            guiHandler = _ =>
            {
                EditorGUILayout.LabelField("Play Mode Loader Settings", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                Settings.enabled = EditorGUILayout.Toggle("Enable Loader", Settings.enabled);
                Settings.saveBeforePlay = EditorGUILayout.Toggle("Save Before Play", Settings.saveBeforePlay);

                EditorGUILayout.BeginHorizontal();
                Settings.loadingScenePath = EditorGUILayout.TextField("Loading Scene Path", Settings.loadingScenePath);

                if (GUILayout.Button("Select", GUILayout.Width(70)))
                {
                    string selectedPath = EditorUtility.OpenFilePanel(
                        "Select Loading Scene",
                        Application.dataPath,
                        "unity"
                    );

                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        Settings.loadingScenePath = AbsolutePathToAssetPath(selectedPath);
                        Settings.SaveSettings();
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(Settings.loadingScenePath))
                {
                    if (!File.Exists(Settings.loadingScenePath))
                    {
                        EditorGUILayout.HelpBox(
                            "Selected scene path is invalid or missing.",
                            MessageType.Warning
                        );
                    }
                }

                EditorGUILayout.HelpBox(
                    "This setting is stored per project in ProjectSettings/PlayModeLoaderSettings.asset.",
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

        string loadingScenePath = Settings.loadingScenePath;

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

    private static string AbsolutePathToAssetPath(string absolutePath)
    {
        absolutePath = absolutePath.Replace("\\", "/");

        string dataPath = Application.dataPath.Replace("\\", "/");

        if (absolutePath.StartsWith(dataPath))
            return "Assets" + absolutePath.Substring(dataPath.Length);

        return absolutePath;
    }

    [MenuItem("Edit/Play Mode Loader/Select Loading Scene")]
    private static void SelectSceneMenu()
    {
        string selectedPath = EditorUtility.OpenFilePanel(
            "Select Loading Scene",
            Application.dataPath,
            "unity"
        );

        if (string.IsNullOrEmpty(selectedPath))
            return;

        Settings.loadingScenePath = AbsolutePathToAssetPath(selectedPath);
        Settings.SaveSettings();

        Debug.Log($"Play Mode Loader: Loading scene set to {Settings.loadingScenePath}");
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
    public string loadingScenePath = "";

    public List<string> previousScenePaths = new();

    public void SaveSettings()
    {
        Save(true);
    }
}

#endif