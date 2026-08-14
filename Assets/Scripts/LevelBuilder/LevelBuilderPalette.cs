using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Light Puzzle/Level Builder Palette", fileName = "LevelBuilderPalette")]
public class LevelBuilderPalette : ScriptableObject
{
    [SerializeField] private string defaultExportFolder = "Assets/Prefab/Levels";
    [SerializeField] private List<LevelObjectDefinition> definitions = new List<LevelObjectDefinition>();

    public string DefaultExportFolder => defaultExportFolder;
    public IReadOnlyList<LevelObjectDefinition> Definitions => definitions;

    public LevelObjectDefinition FindById(string entityId)
    {
        for (int i = 0; i < definitions.Count; i++)
        {
            LevelObjectDefinition definition = definitions[i];
            if (definition != null && definition.EntityId == entityId)
            {
                return definition;
            }
        }

        return null;
    }
}
