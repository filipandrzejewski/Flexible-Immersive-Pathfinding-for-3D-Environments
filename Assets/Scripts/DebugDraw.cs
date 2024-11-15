using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Static editor utility class for drawing.
public static class DebugDraw
{

    // Last known pixel-ratio of the scene view camera. This represents the world-space distance between each pixel.
    // This cache gets around an issue where the scene view camera is null on certain editor frames.
    private static float lastKnownSceneViewPixelRatio = 0.001f;

    // Draws a line in the scene
    public static void DrawLine(Vector3 start, Vector3 end, float thickness, Color color, float duration = 0f)
    {
#if UNITY_EDITOR
        Vector3 lineDir = (end - start).normalized;
        Vector3 toCamera = Vector3.back;

        Camera camera = null;
        // Get the current scene view camera.
        if (SceneView.currentDrawingSceneView != null)
        {
            camera = SceneView.currentDrawingSceneView.camera;
        }

        if (camera == null)
        {
            // Default to the
            camera = Camera.current;
        }
        if (camera != null)
        {
            toCamera = (camera.transform.position - start).normalized;
            lastKnownSceneViewPixelRatio = (camera.orthographicSize * 2f) / camera.pixelHeight;
        }

        Vector3 orthogonal = Vector3.Cross(lineDir, toCamera).normalized;
        int pixelThickness = Mathf.CeilToInt(thickness);
        float totalThick = lastKnownSceneViewPixelRatio * pixelThickness;
        for (int i = 0; i < pixelThickness; i++)
        {
            Vector3 offset = orthogonal * ((i * lastKnownSceneViewPixelRatio) - (totalThick / 2f));
            Debug.DrawLine(start + offset, end + offset, color, duration);
        }
#endif //UNITY_EDITOR
    }

}
