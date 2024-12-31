using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class EditorPlayModeSceneLoader
{
    private static string lastOpenedScenePathKey = "LastOpenedScenePath"; // EditorPrefs key
    private static string connectScenePath = "Assets/Scenes/ConnectScene.unity"; // Path to the ConnectScene
    private static string lastOpenedSceneBeforePlayMode = string.Empty;

    static EditorPlayModeSceneLoader()
    {
        // Subscribe to the play mode state change and scene opened events
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        // Only save the scene path if it's not the ConnectScene unless it's the last opened scene before Play Mode
        if (scene.path != connectScenePath || (scene.path == connectScenePath && string.IsNullOrEmpty(lastOpenedSceneBeforePlayMode)))
        {
            // Save the path of the scene that has been opened in EditorPrefs
            EditorPrefs.SetString(lastOpenedScenePathKey, scene.path);

            // Debug log to confirm that the scene has been saved
            Debug.Log($"Scene opened: {scene.path}");
        }
        else
        {
            Debug.Log($"Skipping saving scene path for ConnectScene: {scene.path}");
        }
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode) // Before entering Play Mode
        {
            // Store the current scene before entering Play Mode (to restore after exiting)
            lastOpenedSceneBeforePlayMode = EditorSceneManager.GetActiveScene().path;

            // Specify the scene to load before Play Mode
            string scenePath = connectScenePath;

            // Load the scene only if it's different from the current scene
            if (EditorSceneManager.GetActiveScene().path != scenePath)
            {
                Debug.Log($"Loading scene '{scenePath}' before entering Play Mode.");
                EditorSceneManager.OpenScene(scenePath);
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode) // After exiting Play Mode
        {
            // Retrieve the previously opened scene path from EditorPrefs
            string lastOpenedScenePath = EditorPrefs.GetString(lastOpenedScenePathKey, string.Empty);

            // Debug log to confirm the path retrieval
            Debug.Log($"Last opened scene path: {lastOpenedScenePath}");

            // If the last opened scene path is valid, restore the original scene
            if (!string.IsNullOrEmpty(lastOpenedScenePath) && EditorSceneManager.GetActiveScene().path != lastOpenedScenePath)
            {
                Debug.Log($"Restoring scene '{lastOpenedScenePath}' after exiting Play Mode.");
                EditorSceneManager.OpenScene(lastOpenedScenePath);
            }
            else
            {
                // If no path is found or the same scene is active, ensure we restore the last scene before play mode
                if (!string.IsNullOrEmpty(lastOpenedSceneBeforePlayMode) && EditorSceneManager.GetActiveScene().path != lastOpenedSceneBeforePlayMode)
                {
                    Debug.Log($"Restoring scene '{lastOpenedSceneBeforePlayMode}' after exiting Play Mode.");
                    EditorSceneManager.OpenScene(lastOpenedSceneBeforePlayMode);
                }
            }
        }
    }
}