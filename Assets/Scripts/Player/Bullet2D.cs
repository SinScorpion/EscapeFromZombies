using UnityEngine;

public class Bullet2D : MonoBehaviour
{
    [Header("Runtime params")]
    public float speed = 12f;
    public float lifeTime = 2f;
    public float damage = 1f;
    public float stunSeconds = 0.5f;
    public LayerMask hitMask;     // кого считать «целью» (Enemy)
    public LayerMask blockMask;   // стены/препятствия

    Vector2 dir;
    float t;

    // Инициализация снаружи
    public void Init(Vector2 direction, float spd, float dmg, float stun, LayerMask hit, LayerMask block)
    {
        dir = direction.normalized;
        speed = spd;
        damage = dmg;
        stunSeconds = stun;
        hitMask = hit;
        blockMask = block;
        t = 0f;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        t += dt;
        if (t >= lifeTime) { Destroy(gameObject); return; }

        Vector2 from = transform.position;
        Vector2 to = from + dir * speed * dt;

        // Пролет лучом, чтобы не проскочить тонкие коллайдеры
        RaycastHit2D hit = Physics2D.Raycast(from, dir, (to - from).magnitude, hitMask | blockMask);
        if (hit.collider != null)
        {
            // Попали во врага?
            if (((1 << hit.collider.gameObject.layer) & hitMask) != 0)
            {
                var hp = hit.collider.GetComponentInParent<ZombieHealth>();
                if (hp) hp.Take(damage);

                var stun = hit.collider.GetComponentInParent<ZombieStun2D>();
                if (stun) stun.Apply(stunSeconds);
            }
            // В любом случае — уничтожаемся при первом столкновении
            Destroy(gameObject);
            return;
        }

        transform.position = to;
    }
}
