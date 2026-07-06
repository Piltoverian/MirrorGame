using UnityEngine;

public class WinningTest : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private int currentLevel;
    [SerializeField] private bool playWinSound = true;

    private bool hasWon;

    private void LateUpdate()
    {
        if (hasWon)
        {
            return;
        }

        LightReceiver[] doors = FindObjectsByType<LightReceiver>(FindObjectsSortMode.None);

        if (doors.Length > 0)
        {
            foreach (LightReceiver door in doors)
            {
                if (!door.IsOpened())
                {
                    return;
                }
            }
        }

        HandleWin();
    }

    private void HandleWin()
    {
        hasWon = true;

        if (playWinSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWin();
        }

        Time.timeScale = 0f;

        SaveData saveData = SaveDataHelper.GetGameData();
        int nextLevel = currentLevel + 1;

        if (nextLevel - 1 < saveData.GetLevelsCondition().Count)
        {
            saveData.ChangeLevel(nextLevel, true);
            SaveDataHelper.SaveGame(saveData);
        }

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
    }
}
