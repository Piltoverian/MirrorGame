using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    [Header("SFX")]
    public AudioClip gameStart;
    public AudioClip win;

    public AudioClip shine;
    public AudioClip shine_C;
    public AudioClip shine_DE;
    public AudioClip shine_E;
    public AudioClip shine_F;
    public AudioClip shine_FG;
    public AudioClip shine_G;

    private AudioClip currentMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    #region BGM

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (currentMusic == clip && bgmSource.isPlaying)
            return;

        currentMusic = clip;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopMusic()
    {
        bgmSource.Stop();
    }

    #endregion

    #region SFX

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayWin()
    {
        PlaySFX(win);
    }

    public void PlayGameStart()
    {
        PlaySFX(gameStart);
    }

    #endregion

    #region Volume

    public void SetMusicVolume(float value)
    {
        bgmSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
    }

    #endregion
}