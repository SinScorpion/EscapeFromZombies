using UnityEngine;

public class FlipByVelocity2D : MonoBehaviour
{
    // „ей вектор скорости читаем (обычно Rigidbody2D "Body")
    public Rigidbody2D sourceRb;

    // true, если твой спрайт »«Ќј„јЋ№Ќќ смотрит вправо; false Ч если влево
    public bool faceRightByDefault = true;

    SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        if (!sourceRb || !sr) return;

        float vx = sourceRb.linearVelocity.x;
        if (Mathf.Abs(vx) < 0.01f) return; // стоим Ч не мен€ем лицо

        bool movingRight = vx > 0f;

        // flipX = инвертируем, если двигаемс€ Ђне в базовую сторонуї
        sr.flipX = faceRightByDefault ? !movingRight : movingRight;
    }
}
