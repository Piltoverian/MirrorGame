using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private GameObject levelContentPrefab;
    [SerializeField] private Transform levelParent;
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool clearExistingChildren = true;

    private GameObject currentLevelInstance;

    public GameObject CurrentLevelInstance => currentLevelInstance;

    private void Start()
    {
        if (loadOnStart)
        {
            LoadAssignedLevel();
        }
    }

    public void LoadAssignedLevel()
    {
        LoadLevel(levelContentPrefab);
    }

    public void LoadLevel(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("LevelLoader has no level content prefab assigned.");
            return;
        }

        Transform parent = levelParent != null ? levelParent : transform;

        if (clearExistingChildren)
        {
            ClearChildren(parent);
        }
        else if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
        }

        currentLevelInstance = Instantiate(prefab, parent);
        currentLevelInstance.transform.localPosition = Vector3.zero;
        currentLevelInstance.transform.localRotation = Quaternion.identity;
        currentLevelInstance.transform.localScale = Vector3.one;
    }

    public void ClearLoadedLevel()
    {
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
            currentLevelInstance = null;
        }
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }

        currentLevelInstance = null;
    }
}
