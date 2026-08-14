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

        LightReceiver[] receivers = FindObjectsByType<LightReceiver>();
        bool foundWinReceiver = false;

        if (receivers.Length > 0)
        {
            foreach (LightReceiver receiver in receivers)
            {
                if (!receiver.CountsForWin())
                {
                    continue;
                }

                foundWinReceiver = true;

                if (!receiver.IsOpened())
                {
                    return;
                }
            }
        }

        if (foundWinReceiver)
        {
            HandleWin();
        }
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
