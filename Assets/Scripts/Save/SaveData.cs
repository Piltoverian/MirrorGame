using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    [SerializeField]List<bool> LevelsCondition = new List<bool>();
    public List<bool> GetLevelsCondition()
    {
        return LevelsCondition;
    }

    public void ChangeLevel(int level, bool newStatus)
    {
        int index = level-1;
        if (index < 0 )
        {
            Debug.LogWarning("Level index out of range.");
            return;
        }
        else if (level >= LevelsCondition.Count)
        {
            // Expand the list to accommodate the new level
            for (int i = LevelsCondition.Count; i <= index; i++)
            {
                LevelsCondition.Add(false); // Default to false for new levels
            }
            Debug.LogWarning("Level index out of range. Expanding LevelsCondition list.");
            SaveDataHelper.SaveGame(this); // Save the updated data
        }
        LevelsCondition[index] = newStatus;
    }

    public bool GetLevelUnlockStatus(int level)
    {
        int index = level - 1;
        if (index < 0)
        {
            return false;
        }
        else if (level >= LevelsCondition.Count)
        {
            // Expand the list to accommodate the new level
            for (int i = LevelsCondition.Count; i <= index; i++)
            {
                LevelsCondition.Add(false); // Default to false for new levels
            }
            SaveDataHelper.SaveGame(this); // Save the updated data
            Debug.LogWarning("Level index out of range. Expanding LevelsCondition list.");
            return false;
        }
        return LevelsCondition[index];
    }
}

public static class SaveDataHelper
{
    const string SaveFileName = "savegame.json";
    const string SaveFilePath = "./" + SaveFileName;
    public static void SaveGame(SaveData saveData)
    {
        if (File.Exists(SaveFilePath))
        {
            string json = JsonUtility.ToJson(saveData);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log("Game saved!");

        }
        else
        {
            Debug.LogWarning("Save directory does not exist.");
            File.Create(SaveFilePath).Dispose();
            SaveData newSaveData = new SaveData();
            File.WriteAllText(SaveFilePath, JsonUtility.ToJson(newSaveData));
        }
    }
    public static SaveData GetGameData()
    {
        if (!File.Exists(SaveFilePath))
        {
            //create new file
            File.Create(SaveFilePath).Dispose();
            SaveData newSaveData = new SaveData();
            File.WriteAllText(SaveFilePath, JsonUtility.ToJson(newSaveData));
            Debug.LogWarning("Save directory does not exist.");
            return new SaveData();
        }
        else
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            return saveData;
        }
    }
}
