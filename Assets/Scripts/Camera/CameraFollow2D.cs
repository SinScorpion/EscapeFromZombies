using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;          // на кого смотрим (Player)
    public float smoothTime = 0.12f;  // «вязкость» следования
    public Vector2 offset;            // сдвиг, если захочешь сместить взгляд

    [Header("Optional bounds (выключить на сейчас)")]
    public bool clamp = false;
    public Vector2 min;   // левый-нижний угол мира
    public Vector2 max;   // правый-верхний угол мира

    Vector3 vel;
    Camera cam;

    void Awake() => cam = GetComponent<Camera>();

    void LateUpdate()
    {
        if (!target) return;

        // куда хотим прийти (z остаётся у камеры: обычно -10)
        var desired = new Vector3(target.position.x + offset.x,
                                  target.position.y + offset.y,
                                  transform.position.z);

        var pos = Vector3.SmoothDamp(transform.position, desired, ref vel, smoothTime);

        if (clamp && cam != null)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            pos.x = Mathf.Clamp(pos.x, min.x + halfW, max.x - halfW);
            pos.y = Mathf.Clamp(pos.y, min.y + halfH, max.y - halfH);
        }

        transform.position = pos;
    }
}
