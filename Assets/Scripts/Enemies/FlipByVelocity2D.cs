using UnityEngine;

public class FlipByVelocity2D : MonoBehaviour
{
    // ��� ������ �������� ������ (������ Rigidbody2D "Body")
    public Rigidbody2D sourceRb;

    // true, ���� ���� ������ ���������� ������� ������; false � ���� �����
    public bool faceRightByDefault = true;

    SpriteRenderer sr;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void LateUpdate()
    {
        if (!sourceRb || !sr) return;

        float vx = sourceRb.linearVelocity.x;
        if (Mathf.Abs(vx) < 0.01f) return; // ����� � �� ������ ����

        bool movingRight = vx > 0f;

        // flipX = �����������, ���� ��������� ��� � ������� �������
        sr.flipX = faceRightByDefault ? !movingRight : movingRight;
    }
}
