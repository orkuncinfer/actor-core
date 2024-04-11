using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    float deltaTime = 0.0f;
    [SerializeField] private bool _showFPS = true;

    void Update()
    {
        // Capture the time taken to render the last frame and update the deltaTime.
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (Input.GetKeyDown(KeyCode.U))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = .3f;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
            else
            {
                Time.timeScale = 1;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
        }
    }

    void OnGUI()
    {
        if(!_showFPS) return;
        // Calculate the current FPS
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;

        // Format the display string
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        // Set the display position and size
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();

        // Set the rectangle size and position for the FPS display
        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.white;

        // Draw the FPS text on the screen
        GUI.Label(rect, text, style);
    }
}