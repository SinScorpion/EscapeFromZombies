using UnityEngine;

[RequireComponent(typeof(ZombieSimpleAI2D), typeof(Rigidbody2D))]
public class ZombieStun2D : MonoBehaviour
{
    public float remaining;      // таймер стана
    ZombieSimpleAI2D ai;
    Rigidbody2D rb;

    void Awake()
    {
        ai = GetComponent<ZombieSimpleAI2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Apply(float seconds)
    {
        if (seconds > remaining) remaining = seconds;
        if (ai && ai.enabled) ai.enabled = false;   // отключаем ИИ
        if (rb) rb.linearVelocity = Vector2.zero;   // мгновенно гасим скорость
    }

    void Update()
    {
        if (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            if (rb) rb.linearVelocity = Vector2.zero; // держим на нуле пока длится стан
            if (remaining <= 0f && ai) ai.enabled = true;
        }
    }
}
