using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ZombieSimpleAI2D : MonoBehaviour
{
    public Transform target;   // игрок
    public float speed = 2f;

    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        if (!target) { rb.linearVelocity = Vector2.zero; return; }
        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        rb.linearVelocity = dir * speed;
    }
}
