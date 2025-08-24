using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZombieTouchDamage2D : MonoBehaviour
{
    public float dps = 2f; // урона в секунду при соприкосновении

    void OnCollisionStay2D(Collision2D col)
    {
        var hp = col.collider.GetComponent<PlayerHealth>();
        if (hp != null)
            hp.Take(dps * Time.deltaTime);
    }
}
