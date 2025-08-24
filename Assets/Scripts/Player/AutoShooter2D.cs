using UnityEngine;

public class AutoShooter2D : MonoBehaviour
{
    [Header("Targeting")]
    public float range = 6f;
    public LayerMask enemyMask;

    [Header("Weapon nodes")]
    public Transform weaponPivot;   // Player/WeaponPivot
    public Transform muzzle;        // Player/WeaponPivot/Muzzle
    Vector3 muzzleLocalRight;

    [Header("Fire (временно луч, позже пули)")]
    public float rpm = 240f;
    public float damage = 1f;

    [Header("Tuning")]
    public float aimDeadZone = 0.01f;        // порог скорости по X для «идём влево/вправо»
    public float weaponAngleOffsetDeg = 0f;  // подстройка, если спрайт не идеально вправо

    [Header("Debug")]
    public bool debugDraw = false;

    float cooldown;
    PlayerMove2D owner;
    Rigidbody2D rb;
    SpriteRenderer bodySR;
    SpriteRenderer weaponSR;
    Vector2 lastAimDir = Vector2.right;

    void Awake()
    {
        owner = GetComponent<PlayerMove2D>();
        rb = GetComponent<Rigidbody2D>();
        bodySR = GetComponent<SpriteRenderer>();
        if (weaponPivot) weaponSR = weaponPivot.GetComponent<SpriteRenderer>();
        if (muzzle) muzzleLocalRight = muzzle.localPosition;
    }

    void Update()
    {
        cooldown -= Time.deltaTime;

        // --- 1) цель (может не быть)
        Transform target = GetNearest(Physics2D.OverlapCircleAll(transform.position, range, enemyMask));
        bool hasTarget = target != null;
        // --- 2) направление прицеливания
        Vector2 aimDir;

        if (target) // приоритет: цель
        {
            Vector2 pivotPos = weaponPivot ? (Vector2)weaponPivot.position : (Vector2)transform.position;
            aimDir = ((Vector2)target.position - pivotPos).normalized;
            if (aimDir.sqrMagnitude < 0.0001f) aimDir = lastAimDir;
            owner?.SetAimFacing(aimDir.x); // тело поворачиваем по стрельбе
        }
        else // цели нет: живём по движению
        {
            owner?.SetAimFacing(0f); // даём PlayerMove2D рулить телом по движению

            float vx = rb ? rb.linearVelocity.x : 0f;
            if (Mathf.Abs(vx) >= aimDeadZone)
                aimDir = (vx > 0f) ? Vector2.right : Vector2.left;
            else
                aimDir = (bodySR && bodySR.flipX) ? Vector2.left : Vector2.right; // стоим — держим сторону тела
        }

       
        if (weaponPivot)
        {
            // aimDir уже посчитан выше (в сторону цели или движения)
            Vector2 dir = (aimDir.sqrMagnitude > 0.0001f) ? aimDir.normalized : Vector2.right;

            // базовый поворот: ось +X ствола смотрит туда же, куда dir
            Quaternion toAim = Quaternion.FromToRotation(Vector3.right, new Vector3(dir.x, dir.y, 0f));

            // если спрайт пистолета чуть "косо" нарисован — подправь градусы тут (обычно ±3..5)
            Quaternion offset = Quaternion.Euler(0f, 0f, weaponAngleOffsetDeg);

            weaponPivot.rotation = toAim * offset;
            bool leftShoot = hasTarget && aimDir.x < 0f;
            // на всякий случай: если кто-то случайно зафлипал спрайт руками — принудительно отключим
            if (weaponSR)
            {
                weaponSR.flipX = false;
                weaponSR.flipY = leftShoot;
            }

            if (muzzle)
            {
                var lp = muzzleLocalRight;
                lp.x = leftShoot ? -Mathf.Abs(lp.x) : Mathf.Abs(lp.x);
                muzzle.localPosition = lp;
                muzzle.localRotation = Quaternion.identity; // на всякий случай
            }
        }

        lastAimDir = aimDir;

        // --- 4) стрельба только при наличии цели
        if (!target) return;

        if (cooldown <= 0f)
        {
            var hp = target.GetComponent<ZombieHealth>();
            if (hp != null)
            {
                hp.Take(damage);
                cooldown = 60f / Mathf.Max(1f, rpm);

                if (debugDraw && muzzle)
                    Debug.DrawLine(muzzle.position, target.position, Color.yellow, 0.05f);
            }
        }
        else if (debugDraw && muzzle)
        {
            // короткая линия вдоль ствола — для проверки вылета из Muzzle
            Debug.DrawLine(muzzle.position, muzzle.position + (Vector3)(weaponPivot.right * 1.5f), Color.yellow, 0.02f);
        }
    }

    Transform GetNearest(Collider2D[] hits)
    {
        if (hits == null || hits.Length == 0) return null;
        float best = float.MaxValue; Transform bestT = null;
        Vector3 p = transform.position;
        for (int i = 0; i < hits.Length; i++)
        {
            float d = (hits[i].transform.position - p).sqrMagnitude;
            if (d < best) { best = d; bestT = hits[i].transform; }
        }
        return bestT;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
