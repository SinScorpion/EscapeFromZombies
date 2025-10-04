using UnityEngine;
using Lean.Pool; // <-- добавили

public class Bullet2D : MonoBehaviour
{
    [Header("Runtime params")]
    public float speed = 12f;
    public float lifeTime = 2f;
    public float damage = 1f;
    public float stunSeconds = 0.5f;
    public LayerMask hitMask;
    public LayerMask blockMask;

    [Header("Visual")]
    [Tooltip("Если спрайт пули изначально направлен не вдоль +X, укажи поправку (в градусах).")]
    public float angleOffsetDeg = 0f;

    Vector2 dir;
    float t;

    public void Init(Vector2 direction, float spd, float dmg, float stun, LayerMask hit, LayerMask block)
    {
        dir = direction.normalized;
        speed = spd;
        damage = dmg;
        stunSeconds = stun;
        hitMask = hit;
        blockMask = block;
        t = 0f;

        transform.rotation =
            Quaternion.FromToRotation(Vector3.right, new Vector3(dir.x, dir.y, 0f)) *
            Quaternion.Euler(0f, 0f, angleOffsetDeg);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        t += dt;
        if (t >= lifeTime) { LeanPool.Despawn(gameObject); return; } // <-- было Destroy

        Vector2 from = transform.position;
        Vector2 to = from + dir * speed * dt;

        var hit = Physics2D.Raycast(from, dir, (to - from).magnitude, hitMask | blockMask);
        if (hit.collider != null)
        {
            if (((1 << hit.collider.gameObject.layer) & hitMask) != 0)
            {
                var hp = hit.collider.GetComponentInParent<ZombieHealth>();
                if (hp) hp.Take(damage);

                var stun = hit.collider.GetComponentInParent<ZombieStun2D>();
                if (stun) stun.Apply(stunSeconds);
            }

            LeanPool.Despawn(gameObject); // <-- было Destroy
            return;
        }

        transform.position = to;
    }
}
