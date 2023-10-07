using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
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
             //return !Application.isEditor;
             return true;
        }
    }
}