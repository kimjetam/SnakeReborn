using UnityEngine;

public class QuitGameHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit(); // Closes the game
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in Unity Editor
            #endif
        }
    }
}
