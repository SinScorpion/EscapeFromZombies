using UnityEngine;

public class ZombieHealth : MonoBehaviour
{
    [Header("HP")]
    [Min(0.1f)] public float hp = 3f;

    [Header("SFX (optional)")]
    public AudioSource sfxSource;          // можно оставить пустым
    public AudioClip[] hitClips;           // несколько звуков ранения
    public AudioClip[] deathClips;         // несколько звуков смерти
    [Range(0f, 1f)] public float hitVolume = 1f;
    [Range(0f, 1f)] public float deathVolume = 1f;

    public void Take(float dmg)
    {
        if (hp <= 0f) return;
        hp -= dmg;

        if (hp > 0f)
        {
            PlayOneOf(hitClips, hitVolume, useClipAtPoint: false); // при жизни можно играть на sfxSource
        }
        else
        {
            // на смерть лучше не зависеть от источника на зомби, т.к. объект сейчас уничтожим
            PlayOneOf(deathClips, deathVolume, useClipAtPoint: true);
            Destroy(gameObject);
        }
    }

    void PlayOneOf(AudioClip[] pool, float volume, bool useClipAtPoint)
    {
        if (pool == null || pool.Length == 0) return;
        var clip = pool[Random.Range(0, pool.Length)];
        if (clip == null) return;

        if (useClipAtPoint || sfxSource == null)
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        else
            sfxSource.PlayOneShot(clip, volume);
    }
}
