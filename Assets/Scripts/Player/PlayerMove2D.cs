using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove2D : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 5f;

    Rigidbody2D rb;

    [Header("Visual/Weapon (optional)")]
    public Transform weaponPivot;       // перетяни сюда WeaponPivot
    public Vector2 weaponLocalPos = new(0.35f, 0.15f);
    SpriteRenderer weaponSR;


    SpriteRenderer sr;
    int lastDirX = 1; // 1 = вправо, -1 = влево

    // --- NEW: приоритет направления от прицеливания/стрельбы ---
    int aimOverride = 0; // -1/0/1
    public void SetAimFacing(float x)
    {
        aimOverride = Mathf.Abs(x) > 0.01f ? (x > 0f ? 1 : -1) : 0;
    }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>(); // возьмём с того же объекта
        if (weaponPivot) weaponSR = weaponPivot.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector2 input = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();

        float speed = walkSpeed;
        rb.linearVelocity = input * speed;


        if (input.x > 0.01f) lastDirX = 1;
        else if (input.x < -0.01f) lastDirX = -1;

        // применяем приоритет от стрельбы, если есть
        int faceX = (aimOverride != 0) ? aimOverride : lastDirX;
        if (sr != null)
            sr.flipX = (faceX == -1);

        if (weaponPivot)
        {
            float sign = sr && sr.flipX ? -1f : 1f;
            weaponPivot.localPosition = new Vector3(sign * weaponLocalPos.x, weaponLocalPos.y, 0f);
            if (weaponSR) weaponSR.flipX = false; // поворот оружия только углом, без flipX
        }
    }
}
