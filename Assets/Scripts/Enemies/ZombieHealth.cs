using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [Min(0.1f)] public float hp = 3f;

    public void Take(float dmg)
    {
        hp -= dmg;
        if (hp <= 0f) Die();
    }

    void Die()
    {
        Destroy(gameObject);
        // TODO: позже Ч звук/частицы/лут
    }
}
