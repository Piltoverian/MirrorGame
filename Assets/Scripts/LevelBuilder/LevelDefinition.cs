using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class LevelDefinition : MonoBehaviour
{
    [SerializeField] private string levelId = "Level_New";
    [SerializeField] private Vector2Int gridSize = new Vector2Int(16, 10);
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Transform entitiesRoot;
    [SerializeField] private List<LevelEntityConfig> entitySnapshot = new List<LevelEntityConfig>();

    public string LevelId => levelId;
    public Vector2Int GridSize => gridSize;
    public float CellSize => cellSize;
    public Transform EntitiesRoot => entitiesRoot != null ? entitiesRoot : transform;
    public IReadOnlyList<LevelEntityConfig> EntitySnapshot => entitySnapshot;

    public void Configure(string newLevelId, Vector2Int newGridSize, float newCellSize, Transform newEntitiesRoot)
    {
        levelId = string.IsNullOrWhiteSpace(newLevelId) ? name : newLevelId;
        gridSize = newGridSize;
        cellSize = Mathf.Max(0.01f, newCellSize);
        entitiesRoot = newEntitiesRoot;
    }

    public void SetEntitySnapshot(List<LevelEntityConfig> configs)
    {
        entitySnapshot.Clear();

        if (configs == null)
        {
            return;
        }

        for (int i = 0; i < configs.Count; i++)
        {
            if (configs[i] != null)
            {
                entitySnapshot.Add(configs[i].Clone());
            }
        }
    }

    public List<LevelEntityConfig> BuildSnapshotFromChildren()
    {
        List<LevelEntityConfig> configs = new List<LevelEntityConfig>();
        LevelEntity[] entities = GetComponentsInChildren<LevelEntity>(true);

        for (int i = 0; i < entities.Length; i++)
        {
            if (entities[i] != null && entities[i].Config != null)
            {
                configs.Add(entities[i].Config.Clone());
            }
        }

        SetEntitySnapshot(configs);
        return configs;
    }
}
