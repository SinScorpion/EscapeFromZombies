using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance;
    AudioSource src;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        src = GetComponent<AudioSource>();
        src.loop = true;           // на вс€кий случай
        if (!src.isPlaying) src.Play();
    }

    // (опционально) смена трека по коду
    public void Play(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null) return;
        src.Stop();
        src.clip = clip;
        src.volume = volume;
        src.loop = loop;
        src.Play();
    }
}
