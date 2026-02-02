using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour
{
    public float zoomSpeed = 6f;     // units per scroll notch
    public float minSize = 6f;       // zoomed in
    public float maxSize = 30f;      // zoomed out

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Update()
    {
        if (Mouse.current == null) return;

        float scrollY = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scrollY) < 0.01f) return;

        // InputSystem scroll is usually ~±120 per notch → normalize to ±1
        float scrollNotches = scrollY / 120f;

        cam.orthographicSize = Mathf.Clamp(
            cam.orthographicSize - scrollNotches * zoomSpeed,
            minSize,
            maxSize
        );
    }
}
