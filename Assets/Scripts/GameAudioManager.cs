using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip coinClip;
    [SerializeField] private AudioClip collisionClip;
    [SerializeField] private AudioClip buttonClip;

    public static GameAudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayCoin() => PlaySfx(coinClip);
    public void PlayCollision() => PlaySfx(collisionClip);
    public void PlayButton() => PlaySfx(buttonClip);

    public void SetMusicEnabled(bool enabled)
    {
        if (musicSource != null)
            musicSource.mute = !enabled;

        PlayerPrefs.SetInt("ColorDashMusicEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSfxEnabled(bool enabled)
    {
        if (sfxSource != null)
            sfxSource.mute = !enabled;

        PlayerPrefs.SetInt("ColorDashSfxEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void Start()
    {
        bool musicEnabled = PlayerPrefs.GetInt("ColorDashMusicEnabled", 1) == 1;
        bool sfxEnabled = PlayerPrefs.GetInt("ColorDashSfxEnabled", 1) == 1;

        if (musicSource != null)
            musicSource.mute = !musicEnabled;

        if (sfxSource != null)
            sfxSource.mute = !sfxEnabled;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null && !sfxSource.mute)
            sfxSource.PlayOneShot(clip);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
