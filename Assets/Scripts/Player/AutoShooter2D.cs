using UnityEngine;

public class AutoShooter2D : MonoBehaviour
{
    [SerializeField] private float flipHysteresisX = 0.15f; // порог по |x| для смены стороны (липкость)
    private int faceSign = 1; // 1 = вправо, -1 = влево (липкая сторона)

    [Header("Targeting")]
    public float range = 6f;
    public LayerMask enemyMask;

    [Header("Weapon nodes")]
    public Transform weaponPivot;   // Player/WeaponPivot
    public Transform muzzle;        // Player/WeaponPivot/Muzzle
    private Vector3 muzzleLocalRight;

    [Header("Fire (пули)")]
    public GameObject bulletPrefab;
    public float rpm = 240f;
    public float damage = 1f;
    public float bulletSpeed = 12f;
    public float bulletLifeTime = 2f;
    public float spreadDeg = 4f;

    [Header("Tuning")]
    public float aimDeadZone = 0.01f;
    public float weaponAngleOffsetDeg = 0f;

    [Header("Collision masks")]
    public LayerMask blockMask; // стены/препятствия для пули

    [Header("Debug")]
    public bool debugDraw = false;

    // ------- SFX -------
    [Header("SFX")]
    public AudioSource sfxSource;            // перетащи сюда AudioSource с WeaponPivot
    public AudioClip[] shotClips;            // можно 1 или несколько клипов
    [Range(0f, 1f)] public float shotVolume = 0.85f;
    public Vector2 pitchJitter = new Vector2(0.97f, 1.03f);

    private float cooldown;
    private PlayerMove2D owner;
    private Rigidbody2D rb;
    private SpriteRenderer bodySR, weaponSR;
    private Vector2 lastAimDir = Vector2.right;

    void Awake()
    {
        owner = GetComponent<PlayerMove2D>();
        rb = GetComponent<Rigidbody2D>();
        bodySR = GetComponent<SpriteRenderer>();
        if (weaponPivot) weaponSR = weaponPivot.GetComponent<SpriteRenderer>();
        if (!sfxSource && weaponPivot) sfxSource = weaponPivot.GetComponent<AudioSource>();
        if (muzzle) muzzleLocalRight = muzzle.localPosition; // базовая «правая» локальная точка дула
    }

    // обновляем «липкую» сторону, только если явно ушли от нуля по X
    private void UpdateFace(float desiredX)
    {
        if (Mathf.Abs(desiredX) > flipHysteresisX)
            faceSign = (desiredX >= 0f) ? 1 : -1;
    }

    void Update()
    {
        cooldown -= Time.deltaTime;

        // 1) цель (может отсутствовать)
        Transform target = GetNearest(Physics2D.OverlapCircleAll(transform.position, range, enemyMask));
        bool hasTarget = target != null;

        // 2) направление прицеливания и сторона тела
        Vector2 aimDir;
        if (hasTarget)
        {
            Vector2 pivotPos = weaponPivot ? (Vector2)weaponPivot.position : (Vector2)transform.position;
            aimDir = ((Vector2)target.position - pivotPos);
            if (aimDir.sqrMagnitude < 0.0001f) aimDir = lastAimDir; else aimDir.Normalize();

            UpdateFace(aimDir.x);            // липкая сторона по цели
            owner?.SetAimFacing(faceSign);   // тело смотрит по стрельбе (липко)
        }
        else
        {
            owner?.SetAimFacing(0f);         // без цели — тело по движению
            float vx = rb ? rb.linearVelocity.x : 0f;
            UpdateFace(vx);

            if (Mathf.Abs(vx) >= aimDeadZone)
                aimDir = vx > 0f ? Vector2.right : Vector2.left;
            else
                aimDir = (bodySR && bodySR.flipX) ? Vector2.left : Vector2.right;
        }

        // 3) поворот оружия (только вращение)
        if (weaponPivot)
        {
            Vector2 dir = (aimDir.sqrMagnitude > 0.0001f) ? aimDir : Vector2.right;
            Quaternion toAim = Quaternion.FromToRotation(Vector3.right, new Vector3(dir.x, dir.y, 0f));
            Quaternion offset = Quaternion.Euler(0f, 0f, weaponAngleOffsetDeg);
            weaponPivot.rotation = toAim * offset;

            // флип pivot'а по Y разрешаем ТОЛЬКО когда:
            //   1) есть цель (идёт атака) И
            //   2) тело уже реально повернулось влево (bodySR.flipX == true)
            bool bodyLeft = bodySR ? bodySR.flipX : (faceSign < 0);
            bool leftShootVisual = hasTarget && bodyLeft;

            weaponPivot.localScale = new Vector3(1f, leftShootVisual ? -1f : 1f, 1f);

            if (weaponSR) { weaponSR.flipX = false; weaponSR.flipY = false; } // спрайтовые флипы не используем
            if (muzzle)
            {
                muzzle.localPosition = muzzleLocalRight;     // дуло всегда в «правой» локальной точке
                muzzle.localRotation = Quaternion.identity;
            }
        }

        lastAimDir = aimDir;

        // 3.5) Гейт: сначала тело развернётся к цели, потом стреляем
        if (hasTarget && bodySR)
        {
            bool wantLeft = (faceSign < 0);   // «куда нужно» по логике прицеливания
            bool bodyLeft = bodySR.flipX;     // «куда сейчас смотрит тело»
            if (bodyLeft != wantLeft)
                return; // ждём кадр, пока PlayerMove2D применит разворот
        }

        // 4) Стрельба
        if (!hasTarget || !muzzle || !bulletPrefab) return;

        if (cooldown <= 0f)
        {
            cooldown = 60f / Mathf.Max(1f, rpm);

            Vector2 shootDir = weaponPivot ? (Vector2)weaponPivot.right : Vector2.right;
            float spread = Random.Range(-spreadDeg * 0.5f, spreadDeg * 0.5f);
            shootDir = Quaternion.Euler(0f, 0f, spread) * shootDir;

            GameObject go = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
            var b = go.GetComponent<Bullet2D>();
            if (b)
            {
                b.lifeTime = bulletLifeTime;
                b.Init(shootDir, bulletSpeed, damage, 0.5f, enemyMask, blockMask); // стан 0.5с (подберём)
            }

            PlayShotSfx(); // <<<<<<<<<<<<<< ВОТ ЗДЕСЬ ВОСПРОИЗВОДИМ ЗВУК

            if (debugDraw)
                Debug.DrawLine(muzzle.position, muzzle.position + (Vector3)shootDir * 2f, Color.yellow, 0.05f);
        }
        else if (debugDraw && muzzle && weaponPivot)
        {
            Debug.DrawLine(muzzle.position, muzzle.position + (Vector3)weaponPivot.right * 1.5f, Color.yellow, 0.02f);
        }
    }

    private void PlayShotSfx()
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
