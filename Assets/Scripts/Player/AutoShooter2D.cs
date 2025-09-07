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

    [Header("Fire (пули)")]
    public GameObject bulletPrefab;       // префаб пули
    public float rpm = 240f;              // скорострельность
    public float damage = 1f;
    public float bulletSpeed = 12f;
    public float bulletLifeTime = 2f;
    public float spreadDeg = 4f;          // разброс на выстрел (углы)

    [Header("Tuning")]
    public float aimDeadZone = 0.01f;     // «стоим»/«идём»
    public float weaponAngleOffsetDeg = 0f;

    [Header("Collision masks")]
    public LayerMask blockMask;           // стены/препятствия (для пули)

    [Header("Debug")]
    public bool debugDraw = false;

    float cooldown;
    PlayerMove2D owner;
    Rigidbody2D rb;
    SpriteRenderer bodySR, weaponSR;
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

        // --- 1) цель
        Transform target = GetNearest(Physics2D.OverlapCircleAll(transform.position, range, enemyMask));
        bool hasTarget = target != null;

        // --- 2) направление прицеливания
        Vector2 aimDir;
        if (hasTarget)
        {
            Vector2 pivotPos = weaponPivot ? (Vector2)weaponPivot.position : (Vector2)transform.position;
            aimDir = ((Vector2)target.position - pivotPos);
            if (aimDir.sqrMagnitude < 0.0001f) aimDir = lastAimDir; else aimDir.Normalize();
            owner?.SetAimFacing(aimDir.x); // тело — по стрельбе
        }
        else
        {
            owner?.SetAimFacing(0f);      // тело — по движению
            float vx = rb ? rb.linearVelocity.x : 0f;
            if (Mathf.Abs(vx) >= aimDeadZone) aimDir = (vx > 0f) ? Vector2.right : Vector2.left;
            else aimDir = (bodySR && bodySR.flipX) ? Vector2.left : Vector2.right;
        }

        // --- 3) поворот оружия и зеркалирование целиком (чтобы Muzzle не «уезжал»)
        if (weaponPivot)
        {
            Vector2 dir = (aimDir.sqrMagnitude > 0.0001f) ? aimDir : Vector2.right;
            Quaternion toAim = Quaternion.FromToRotation(Vector3.right, new Vector3(dir.x, dir.y, 0f));
            weaponPivot.rotation = toAim * Quaternion.Euler(0f, 0f, weaponAngleOffsetDeg);

            bool left = hasTarget && aimDir.x < 0f;
            weaponPivot.localScale = new Vector3(1f, left ? -1f : 1f, 1f);

            if (weaponSR) weaponSR.flipX = false; // перестраховка
            if (muzzle)
            {
                muzzle.localPosition = muzzleLocalRight;     // базовая точка (зеркало сделает остальное)
                muzzle.localRotation = Quaternion.identity;
            }
        }

        lastAimDir = aimDir;

        if (target && bodySR) // есть цель и есть рендер тела
        {
            bool wantLeft = (aimDir.x < 0f);   // куда нам НУЖНО смотреть
            bool bodyLeft = bodySR.flipX;      // куда тело СЕЙЧАС смотрит

            if (bodyLeft != wantLeft)
            {
                // Мы уже попросили PlayerMove2D развернуться (через SetAimFacing),
                // но разворот применится после этого Update. Ждём следующий кадр.
                return;
            }
        }
        // --- 4) стрельба только при наличии цели
        if (!hasTarget || !muzzle || !bulletPrefab) return;

        if (cooldown <= 0f)
        {
            cooldown = 60f / Mathf.Max(1f, rpm);

            // итоговое направление выстрела с разбросом
            Vector2 shootDir = weaponPivot ? (Vector2)weaponPivot.right : Vector2.right;
            float spread = Random.Range(-spreadDeg * 0.5f, spreadDeg * 0.5f);
            shootDir = Quaternion.Euler(0f, 0f, spread) * shootDir;

            // спавним пулю
            GameObject go = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
            var b = go.GetComponent<Bullet2D>();
            if (b)
            {
                b.lifeTime = bulletLifeTime;
                b.Init(shootDir, bulletSpeed, damage, 0.5f, enemyMask, blockMask); // стан = 0.5с
            }

            if (debugDraw)
                Debug.DrawLine(muzzle.position, muzzle.position + (Vector3)shootDir * 2f, Color.yellow, 0.05f);
        }
        else if (debugDraw && muzzle && weaponPivot)
        {
            Debug.DrawLine(muzzle.position, muzzle.position + (Vector3)weaponPivot.right * 1.5f, Color.yellow, 0.02f);
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
