using UnityEngine;

public class WinningTest : MonoBehaviour
{
    [SerializeField] private GameObject winScreen;
    [SerializeField] private int currentLevel;
    private void LateUpdate()
    {
        var doors = FindObjectsByType<LightReceiver>();
        if (doors.Length > 0)
        {
            foreach (var door in doors)
            {
                if (!door.IsOpened())
                {
                    return;
                }
            }
        }
        Time.timeScale = 0;
        SaveData saveData = SaveDataHelper.GetGameData();
        int nextLevel = currentLevel + 1;
        if (nextLevel-1 < saveData.GetLevelsCondition().Count)
        {
            saveData.ChangeLevel(nextLevel, true);
            SaveDataHelper.SaveGame(saveData);
        }
        winScreen.SetActive(true);
    }
}
