using System;
using UnityEngine;

public class LightPuzzleLevelSpawner : MonoBehaviour
{
    [Serializable]
    public class EntityPrefabBinding
    {
        public string id;
        public GameObject prefab;
    }

    [Serializable]
    public class LightPuzzleLevelData
    {
        public LightPuzzleSpawnEntry[] objects;
    }

    [Serializable]
    public class LightPuzzleSpawnEntry
    {
        public string id;
        public Vector2 position;
        public float rotationZ;
        public LightColorChannel color = LightColorChannel.White;
        public float intensity = 10f;
        public bool configureLightSource = false;
        public bool configureReceiver = false;
        public bool receiverRequiresColor = false;
        public bool receiverRequiresExactColor = true;
        public LightColorChannel receiverRequiredColor = LightColorChannel.White;
        public float receiverRequiredIntensity = 10f;
    }

    [SerializeField] private TextAsset levelJson;
    [SerializeField] private EntityPrefabBinding[] prefabBindings;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool clearPreviousSpawnedObjects = true;

    private readonly string[] knownIds =
    {
        "PL_001",
        "LIG_SRC",
        "OBJ_MIR",
        "OBJ_MER",
        "OBJ_SPL",
        "OBJ_CON",
        "OBJ_SWI",
        "OBJ_DOR",
        "OBJ_WAL",
        "OBJ_GOA",
        "ENE_001",
        "ENE_002"
    };

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnFromAssignedJson();
        }
    }

    public void SpawnFromAssignedJson()
    {
        if (levelJson == null)
        {
            Debug.LogWarning("LightPuzzleLevelSpawner has no level JSON assigned.");
            return;
        }

        SpawnFromJson(levelJson.text);
    }

    public void SpawnFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("Cannot spawn level from empty JSON.");
            return;
        }

        LightPuzzleLevelData levelData = JsonUtility.FromJson<LightPuzzleLevelData>(json);
        if (levelData == null || levelData.objects == null)
        {
            Debug.LogError("Level JSON is invalid. Expected root object with an 'objects' array.");
            return;
        }

        if (clearPreviousSpawnedObjects)
        {
            ClearSpawnedChildren();
        }

        for (int i = 0; i < levelData.objects.Length; i++)
        {
            SpawnEntry(levelData.objects[i]);
        }
    }

    public string[] GetKnownIds()
    {
        return knownIds;
    }

    private void SpawnEntry(LightPuzzleSpawnEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.id))
        {
            return;
        }

        GameObject prefab = GetPrefab(entry.id);
        if (prefab == null)
        {
            Debug.LogWarning("No prefab binding found for ID: " + entry.id);
            return;
        }

        GameObject instance = Instantiate(
            prefab,
            entry.position,
            Quaternion.Euler(0f, 0f, entry.rotationZ),
            transform
        );

        ConfigureSpawnedObject(instance, entry);
    }

    private void ConfigureSpawnedObject(GameObject instance, LightPuzzleSpawnEntry entry)
    {
        LightSource lightSource = instance.GetComponentInChildren<LightSource>();
        if (entry.configureLightSource && lightSource != null)
        {
            lightSource.Configure(entry.color, entry.intensity);
        }

        LightReceiver lightReceiver = instance.GetComponentInChildren<LightReceiver>();
        if (entry.configureReceiver && lightReceiver != null)
        {
            lightReceiver.ConfigureRequirement(
                entry.receiverRequiredIntensity,
                entry.receiverRequiresColor,
                entry.receiverRequiredColor,
                entry.receiverRequiresExactColor
            );
        }
    }

    private GameObject GetPrefab(string id)
    {
        if (prefabBindings == null)
        {
            return null;
        }

        for (int i = 0; i < prefabBindings.Length; i++)
        {
            if (prefabBindings[i] != null && prefabBindings[i].id == id)
            {
                return prefabBindings[i].prefab;
            }
        }

        return null;
    }

    private void ClearSpawnedChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
