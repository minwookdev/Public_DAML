using UnityEditor;

[InitializeOnLoad]
public class EditorSaveUserDataHelper
{
    static EditorSaveUserDataHelper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingPlayMode) {
            
        }
        else {
            
        }
    }
}