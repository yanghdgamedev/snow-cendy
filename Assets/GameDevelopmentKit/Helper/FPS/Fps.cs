using UnityEngine;
using System.Collections;

public class Fps : MonoBehaviour
{
    // for ui.
    private int screenLongSide;
    private Rect boxRect;
    private GUIStyle style = new GUIStyle();

    // for fps calculation.
    private int frameCount;
    private float elapsedTime;
    private double frameRate;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        UpdateUISize();
#if ENV_PROD || ENV_CREATIVE
        Destroy(this);
#endif
    }

    /// <summary>
    /// Monitor changes in resolution and calcurate FPS
    /// </summary>
    private void Update()
    {
        // FPS calculation
        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;
        if (elapsedTime > 0.5f)
        {
            frameRate = System.Math.Round(frameCount / elapsedTime, 1, System.MidpointRounding.AwayFromZero);
            frameCount = 0;
            elapsedTime = 0;

            // Update the UI size if the resolution has changed
            if (screenLongSide != Mathf.Max(Screen.width, Screen.height))
            {
                UpdateUISize();
            }
        }
    }

    /// <summary>
    /// Resize the UI according to the screen resolution
    /// </summary>
    private void UpdateUISize()
    {
        screenLongSide = Mathf.Max(Screen.width, Screen.height);
        var rectLongSide = screenLongSide / 15;
        boxRect = new Rect(1, 50, rectLongSide, rectLongSide / 3);
        style.fontSize = (int)(screenLongSide / 50);
        style.normal.textColor = Color.white;
    }

    /// <summary>
    /// Display FPS
    /// </summary>
    private void OnGUI()
    {
        GUI.Box(boxRect, "");
        GUI.Label(boxRect, " " + frameRate, style);
    }
}