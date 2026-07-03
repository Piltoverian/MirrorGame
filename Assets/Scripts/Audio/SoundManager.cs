using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;

    [Header("Scene Auto Play")]
    [SerializeField] private bool autoPlayMusicOnSceneLoaded = true;
    [SerializeField] private bool playGameStartOnGameplaySceneLoaded = true;
    [SerializeField] private string[] mainMenuSceneNames = { "MainMenu", "Menu", "LevelSelect" };
    [SerializeField] private string[] gameplaySceneNames = new string[0];

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
    private int lastAutoHandledFrame = -1;
    private string lastAutoHandledSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void Start()
    {
        if (Instance == this && autoPlayMusicOnSceneLoaded)
        {
            PlayAudioForScene(SceneManager.GetActiveScene());
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!autoPlayMusicOnSceneLoaded)
        {
            return;
        }

        PlayAudioForScene(scene);
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = CreateAudioSource("BGM Source", true);
        }

        if (sfxSource == null)
        {
            sfxSource = CreateAudioSource("SFX Source", false);
        }

        SetupAudioSource(bgmSource, true);
        SetupAudioSource(sfxSource, false);
    }

    private AudioSource CreateAudioSource(string sourceName, bool loop)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        SetupAudioSource(source, loop);
        return source;
    }

    private void SetupAudioSource(AudioSource source, bool loop)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
    }

    private void PlayAudioForScene(Scene scene)
    {
        if (!scene.IsValid())
        {
            return;
        }

        if (lastAutoHandledFrame == Time.frameCount && lastAutoHandledSceneName == scene.name)
        {
            return;
        }

        lastAutoHandledFrame = Time.frameCount;
        lastAutoHandledSceneName = scene.name;

        if (IsMainMenuScene(scene.name))
        {
            PlayMainMenuMusic();
            return;
        }

        if (IsGameplayScene(scene.name))
        {
            PlayGameplayMusic();

            if (playGameStartOnGameplaySceneLoaded)
            {
                PlayGameStart();
            }
        }
    }

    private bool IsMainMenuScene(string sceneName)
    {
        return ContainsSceneName(mainMenuSceneNames, sceneName);
    }

    private bool IsGameplayScene(string sceneName)
    {
        if (ContainsSceneName(gameplaySceneNames, sceneName))
        {
            return true;
        }

        return !IsMainMenuScene(sceneName);
    }

    private bool ContainsSceneName(string[] sceneNames, string sceneName)
    {
        if (sceneNames == null || string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        for (int i = 0; i < sceneNames.Length; i++)
        {
            if (string.Equals(sceneNames[i], sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    #region BGM

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
        {
            return;
        }

        if (currentMusic == clip && bgmSource.isPlaying)
        {
            return;
        }

        currentMusic = clip;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopMusic()
    {
        if (bgmSource == null)
        {
            return;
        }

        bgmSource.Stop();
        currentMusic = null;
    }

    #endregion

    #region SFX

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

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
        if (bgmSource == null)
        {
            return;
        }

        bgmSource.volume = Mathf.Clamp01(value);
    }

    public void SetSFXVolume(float value)
    {
        if (sfxSource == null)
        {
            return;
        }

        sfxSource.volume = Mathf.Clamp01(value);
    }

    #endregion
}
