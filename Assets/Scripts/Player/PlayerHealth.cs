using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Min(1f)] public float maxHp = 5f;
    public float hp;

    SpriteRenderer sr;
    bool dead;

    void Awake()
    {
        hp = maxHp;
        sr = GetComponent<SpriteRenderer>();
    }

    public void Take(float dmg)
    {
        if (dead) return;
        hp = Mathf.Max(0f, hp - dmg);

        // лёгкий визуальный фидбек: краткий «подмиг» цветом
        if (sr) sr.color = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.6f), 0.35f);
        CancelInvoke(nameof(ResetColor));
        Invoke(nameof(ResetColor), 0.08f);

        if (hp <= 0f) Die();
    }

    void ResetColor() { if (sr) sr.color = Color.white; }

    void Die()
    {
        dead = true;
        Debug.Log("PLAYER DEAD");
        // выключаем управление/стрельбу и останавливаемся
        var mv = GetComponent<PlayerMove2D>(); if (mv) mv.enabled = false;
        var sh = GetComponent<AutoShooter2D>(); if (sh) sh.enabled = false;
        var rb = GetComponent<Rigidbody2D>(); if (rb) rb.linearVelocity = Vector2.zero;
        // позже: экран «поражения» и рестарт
    }
}
