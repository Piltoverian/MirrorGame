using UnityEngine;

public class SoftLockManager : MonoBehaviour
{
    public static SoftLockManager Instance { get; private set; }

    [SerializeField] private GameObject softLockPanel;
    [SerializeField] private bool pauseOnSoftLock = false;

    private bool isSoftLocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (softLockPanel != null)
        {
            softLockPanel.SetActive(false);
        }
    }

    public void SetSoftLocked(bool softLocked)
    {
        isSoftLocked = softLocked;

        if (softLockPanel != null)
        {
            softLockPanel.SetActive(isSoftLocked);
        }

        if (pauseOnSoftLock)
        {
            Time.timeScale = isSoftLocked ? 0f : 1f;
        }
    }

    public bool IsSoftLocked()
    {
        return isSoftLocked;
    }
}
