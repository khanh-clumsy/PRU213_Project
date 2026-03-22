using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bgm;

    void Awake()
    {
        // Singleton (chỉ có 1 BGM)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        PlayMusic();
    }

    public void PlayMusic()
    {
        if (audioSource == null) return;

        audioSource.clip = bgm;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}