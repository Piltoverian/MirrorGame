using System;
using UnityEngine;

[Serializable]
public class LevelEntityConfig
{
    public string localId;
    public string entityId;
    public string displayName;
    public LevelObjectDefinition definition;
    public GameObject sourcePrefab;
    public Vector2Int gridPosition;
    public float rotationZ;

    public bool configureLightSource;
    public LightColorChannel lightColor = LightColorChannel.White;
    public float lightIntensity = 10f;

    public bool configureReceiver;
    public bool receiverRequiresColor;
    public bool receiverRequiresExactColor = true;
    public LightColorChannel receiverRequiredColor = LightColorChannel.White;
    public float receiverRequiredIntensity = 10f;
    public bool countsForWin;

    public bool isPushable = true;
    public bool isHorizontalPushable = true;
    public bool isVerticalPushable = true;
    public bool isRotatable = true;
    public bool canSprintPush = true;

    public bool splitIntoRgbComponents;
    public bool mergeColorChannels = true;

    public bool enemyRequiresColor = true;
    public bool enemyRequiresExactColor = true;
    public LightColorChannel enemyRequiredColor = LightColorChannel.Red;
    public float enemyRequiredIntensity = 5f;

    public string linkedDoorIds;
    public Vector2[] patrolPoints = Array.Empty<Vector2>();

    public LevelEntityConfig Clone()
    {
        return new LevelEntityConfig
        {
            localId = localId,
            entityId = entityId,
            displayName = displayName,
            definition = definition,
            sourcePrefab = sourcePrefab,
            gridPosition = gridPosition,
            rotationZ = rotationZ,
            configureLightSource = configureLightSource,
            lightColor = lightColor,
            lightIntensity = lightIntensity,
            configureReceiver = configureReceiver,
            receiverRequiresColor = receiverRequiresColor,
            receiverRequiresExactColor = receiverRequiresExactColor,
            receiverRequiredColor = receiverRequiredColor,
            receiverRequiredIntensity = receiverRequiredIntensity,
            countsForWin = countsForWin,
            isPushable = isPushable,
            isHorizontalPushable = isHorizontalPushable,
            isVerticalPushable = isVerticalPushable,
            isRotatable = isRotatable,
            canSprintPush = canSprintPush,
            splitIntoRgbComponents = splitIntoRgbComponents,
            mergeColorChannels = mergeColorChannels,
            enemyRequiresColor = enemyRequiresColor,
            enemyRequiresExactColor = enemyRequiresExactColor,
            enemyRequiredColor = enemyRequiredColor,
            enemyRequiredIntensity = enemyRequiredIntensity,
            linkedDoorIds = linkedDoorIds,
            patrolPoints = patrolPoints != null ? (Vector2[])patrolPoints.Clone() : Array.Empty<Vector2>()
        };
    }
}

[DisallowMultipleComponent]
public class LevelEntity : MonoBehaviour
{
    [SerializeField] private LevelEntityConfig config = new LevelEntityConfig();

    public LevelEntityConfig Config => config;
    public string LocalId => config.localId;
    public string EntityId => config.entityId;

    public void SetConfig(LevelEntityConfig newConfig)
    {
        config = newConfig != null ? newConfig.Clone() : new LevelEntityConfig();
        if (!string.IsNullOrWhiteSpace(config.displayName))
        {
            gameObject.name = config.displayName;
        }
    }
}
