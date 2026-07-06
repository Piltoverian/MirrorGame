using UnityEngine;

public class LevelSelectButton : ChangeSceneButton
{
    [SerializeField] private bool AutoUnlocked = false;
    [SerializeField] private int level;
    private void Start()
    {
        if (sceneName == null || sceneName == "")
        {
            Debug.LogWarning("Scene name is not set for LevelSelectButton.");
            gameObject.SetActive(false);
            return;
        }
        if (!AutoUnlocked)
        {
            SaveData saveData = SaveDataHelper.GetGameData();
            if (saveData != null)
            {
                bool isUnlocked = saveData.GetLevelUnlockStatus(level);
                gameObject.SetActive(isUnlocked);
            }
            else
            {
                Debug.LogWarning("Failed to load save data.");
                gameObject.SetActive(false);
            }
        }
        else
        {
            gameObject.SetActive(true);
            SaveData saveData = SaveDataHelper.GetGameData();
            if (!saveData.GetLevelUnlockStatus(level))
            {
                saveData.ChangeLevel(level, true);
                SaveDataHelper.SaveGame(saveData);
            }
        }
    }
}
