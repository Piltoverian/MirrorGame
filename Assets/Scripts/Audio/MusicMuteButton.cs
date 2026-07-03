using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MusicMuteButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button button;
    [SerializeField] private bool autoBindButtonClick = true;

    [Header("Optional Visual")]
    [SerializeField] private GameObject musicOnVisual;
    [SerializeField] private GameObject musicOffVisual;
    [SerializeField] private Text label;
    [SerializeField] private string musicOnText = "Music: ON";
    [SerializeField] private string musicOffText = "Music: OFF";

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        if (autoBindButtonClick && button != null)
        {
            button.onClick.RemoveListener(ToggleMusicMute);
            button.onClick.AddListener(ToggleMusicMute);
        }

        RefreshVisual();
    }

    private void OnDisable()
    {
        if (autoBindButtonClick && button != null)
        {
            button.onClick.RemoveListener(ToggleMusicMute);
        }
    }

    public void ToggleMusicMute()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[MusicMuteButton] Cannot toggle music mute because SoundManager.Instance is missing.");
            return;
        }

        SoundManager.Instance.ToggleMusicMute();
        RefreshVisual();
    }

    public void SetMusicMuted(bool muted)
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[MusicMuteButton] Cannot set music mute because SoundManager.Instance is missing.");
            return;
        }

        SoundManager.Instance.SetMusicMuted(muted);
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        bool isMuted = SoundManager.Instance != null && SoundManager.Instance.IsMusicMuted;

        if (musicOnVisual != null)
        {
            musicOnVisual.SetActive(!isMuted);
        }

        if (musicOffVisual != null)
        {
            musicOffVisual.SetActive(isMuted);
        }

        if (label != null)
        {
            label.text = isMuted ? musicOffText : musicOnText;
        }
    }
}
