using UnityEngine;

public class Environment : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
    }

    /// <summary>
    /// While in training, we skip animations and auto retry when game over
    /// </summary>
    /// <returns>
    /// True if it's not editor because we only train AI in standalone build
    /// </returns>
    public static bool IsTraining
    {
        get 
        {
             return !Application.isEditor;
        }
    }
}