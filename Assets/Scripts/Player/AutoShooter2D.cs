using UnityEngine;
using Lean.Pool;

public class AutoShooter2D : MonoBehaviour
{
    [Header("Flip")]
    [SerializeField] private float flipHysteresisX = 0.15f; // порог по |x| для смены стороны
    private int faceSign = 1;                                // 1 = вправо, -1 = влево
    private Vector2 lastAimDir = Vector2.right;

    [Header("Targeting")]
    public float range = 6f;
    public LayerMask enemyMask;

    [Header("Weapon nodes")]
    public Transform weaponPivot;   // Player/WeaponPivot
    public Transform muzzle;        // Player/WeaponPivot/Muzzle
    public float weaponAngleOffsetDeg = 0f;
    private Vector3 muzzleLocalRight;
    private SpriteRenderer bodySR, weaponSR;
    private Rigidbody2D rb;
    private PlayerMove2D owner;

    [Header("Fire (пули)")]
    public GameObject bulletPrefab;   // префаб из пула
    public float rpm = 240f;
    public float damage = 1f;
    public float bulletSpeed = 12f;
    public float bulletLifeTime = 2f;
    public float spreadDeg = 4f;

    [Header("Tuning")]
    public float aimDeadZone = 0.01f;

    [Header("Collision masks")]
    public LayerMask blockMask;       // стены

    [Header("Debug")]
    public bool debugDraw = false;

    [Header("SFX")]
    public AudioSource sfxSource;     // AudioSource на WeaponPivot
    public AudioClip[] shotClips;
    [Range(0f, 1f)] public float shotVolume = 0.85f;
    public Vector2 pitchJitter = new Vector2(0.97f, 1.03f);

    private float cooldown;

    void Awake()
    {
        owner = GetComponent<PlayerMove2D>();
        rb = GetComponent<Rigidbody2D>();
        bodySR = GetComponent<SpriteRenderer>();
        if (weaponPivot) weaponSR = weaponPivot.GetComponent<SpriteRenderer>();
        if (!sfxSource && weaponPivot) sfxSource = weaponPivot.GetComponent<AudioSource>();
        if (muzzle) muzzleLocalRight = muzzle.localPosition;
    }

    void UpdateFace(float desiredX)
    {
        if (Mathf.Abs(desiredX) > flipHysteresisX)
            faceSign = desiredX >= 0f ? 1 : -1;
    }

    void Update()
    {
        cooldown -= Time.deltaTime;

        // --- цель
        var hits = Physics2D.OverlapCircleAll(transform.position, range, enemyMask);
        Transform target = GetNearest(hits);
        bool hasTarget = target != null;

        // --- направление прицеливания + сторона тела
        Vector2 aimDir;
        if (hasTarget)
        {
            Vector2 from = weaponPivot ? (Vector2)weaponPivot.position : (Vector2)transform.position;
            aimDir = ((Vector2)target.position - from);
            if (aimDir.sqrMagnitude < 0.0001f) aimDir = lastAimDir; else aimDir.Normalize();

            UpdateFace(aimDir.x);           // липкая сторона «по цели»
            owner?.SetAimFacing(faceSign);  // тело смотрит туда же
        }
        else
        {
            owner?.SetAimFacing(0f);        // без цели — телом рулит движение
            float vx = rb ? rb.linearVelocity.x : 0f;
            UpdateFace(vx);

            if (Mathf.Abs(vx) >= aimDeadZone)
                aimDir = vx > 0f ? Vector2.right : Vector2.left;
            else
                aimDir = (bodySR && bodySR.flipX) ? Vector2.left : Vector2.right;
        }

        // --- поворот оружия
        if (weaponPivot)
        {
            Vector2 dir = aimDir.sqrMagnitude > 0.0001f ? aimDir : Vector2.right;
            var toAim = Quaternion.FromToRotation(Vector3.right, new Vector3(dir.x, dir.y, 0f));
            var offset = Quaternion.Euler(0f, 0f, weaponAngleOffsetDeg);
            weaponPivot.rotation = toAim * offset;

            // флип спрайта оружия по Y только когда есть цель И тело реально развернулось влево
            bool bodyLeft = bodySR ? bodySR.flipX : (faceSign < 0);
            bool leftShootVisual = hasTarget && bodyLeft;
            weaponPivot.localScale = new Vector3(1f, leftShootVisual ? -1f : 1f, 1f);

            if (weaponSR) { weaponSR.flipX = false; weaponSR.flipY = false; }
            if (muzzle)
            {
                muzzle.localPosition = muzzleLocalRight;       // фиксированная локальная точка
                muzzle.localRotation = Quaternion.identity;
            }
        }

        lastAimDir = aimDir;

        // --- гейт: сначала повернуть тело, потом стрелять
        if (hasTarget && bodySR)
        {
            bool wantLeft = (faceSign < 0);
            if (bodySR.flipX != wantLeft) return;
        }

        // --- стрельба (через пул)
        if (!hasTarget || !muzzle || !bulletPrefab) return;
        if (cooldown > 0f) { if (debugDraw) DebugDrawIdle(); return; }

        cooldown = 60f / Mathf.Max(1f, rpm);

        Vector2 shootDir = weaponPivot ? (Vector2)weaponPivot.right : Vector2.right;
        float spread = Random.Range(-spreadDeg * 0.5f, spreadDeg * 0.5f);
        shootDir = Quaternion.Euler(0f, 0f, spread) * shootDir;

        var rot = Quaternion.FromToRotation(Vector3.right, new Vector3(shootDir.x, shootDir.y, 0f));
        GameObject go = LeanPool.Spawn(bulletPrefab, muzzle.position, rot);
        var b = go.GetComponent<Bullet2D>();
        if (b)
        {
            b.lifeTime = bulletLifeTime;
            // берём stunSeconds из самого префаба, чтобы совпадало с инспектором
            b.Init(shootDir, bulletSpeed, damage, b.stunSeconds, enemyMask, blockMask);
        }

        PlayShotSfx();

        if (debugDraw)
            Debug.DrawLine(muzzle.position, muzzle.position + (Vector3)shootDir * 2f, Color.yellow, 0.05f);
    }

    void DebugDrawIdle()
    {
        if (muzzle && weaponPivot)
            Debug.DrawLine(muzzle.position, muzzle.position + (Vector3)weaponPivot.right * 1.5f, Color.yellow, 0.02f);
    }

    void PlayShotSfx()
    {
        if (!sfxSource || shotClips == null || shotClips.Length == 0) return;
        sfxSource.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        var clip = shotClips[Random.Range(0, shotClips.Length)];
        sfxSource.PlayOneShot(clip, shotVolume);
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
