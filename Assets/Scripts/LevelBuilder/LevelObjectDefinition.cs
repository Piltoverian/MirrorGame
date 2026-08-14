using UnityEngine;

public enum LevelObjectKind
{
    Custom,
    Player,
    LightSource,
    Mirror,
    Wall,
    Goal,
    Merger,
    Splitter,
    ColorConverter,
    DoorSwitch,
    Door,
    EnemyBlocker,
    EnemyDestroyer
}

[CreateAssetMenu(menuName = "Light Puzzle/Level Object Definition", fileName = "LOD_NewObject")]
public class LevelObjectDefinition : ScriptableObject
{
    [SerializeField] private string entityId = "OBJ_CUSTOM";
    [SerializeField] private string displayName = "New Object";
    [SerializeField] private LevelObjectKind kind = LevelObjectKind.Custom;
    [SerializeField] private GameObject prefab;
    [SerializeField] private Texture2D icon;
    [SerializeField] private Color editorColor = Color.white;
    [SerializeField] private Vector2Int footprint = Vector2Int.one;
    [SerializeField] private float defaultRotationZ;

    [Header("Light Defaults")]
    [SerializeField] private bool configureLightSource;
    [SerializeField] private LightColorChannel lightColor = LightColorChannel.White;
    [SerializeField] private float lightIntensity = 10f;

    [Header("Receiver Defaults")]
    [SerializeField] private bool configureReceiver;
    [SerializeField] private bool receiverRequiresColor;
    [SerializeField] private bool receiverRequiresExactColor = true;
    [SerializeField] private LightColorChannel receiverRequiredColor = LightColorChannel.White;
    [SerializeField] private float receiverRequiredIntensity = 10f;
    [SerializeField] private bool countsForWin;

    [Header("Interaction Defaults")]
    [SerializeField] private bool isPushable = true;
    [SerializeField] private bool isHorizontalPushable = true;
    [SerializeField] private bool isVerticalPushable = true;
    [SerializeField] private bool isRotatable = true;
    [SerializeField] private bool canSprintPush = true;

    public string EntityId => entityId;
    public string DisplayName => displayName;
    public LevelObjectKind Kind => kind;
    public GameObject Prefab => prefab;
    public Texture2D Icon => icon;
    public Color EditorColor => editorColor;
    public Vector2Int Footprint => footprint;
    public float DefaultRotationZ => defaultRotationZ;

    public void ApplyDefaults(LevelEntityConfig config)
    {
        if (config == null)
        {
            return;
        }

        config.entityId = entityId;
        config.displayName = displayName;
        config.definition = this;
        config.sourcePrefab = prefab;
        config.rotationZ = defaultRotationZ;
        config.lightColor = lightColor;
        config.lightIntensity = lightIntensity;
        config.configureLightSource = configureLightSource;
        config.configureReceiver = configureReceiver;
        config.receiverRequiresColor = receiverRequiresColor;
        config.receiverRequiresExactColor = receiverRequiresExactColor;
        config.receiverRequiredColor = receiverRequiredColor;
        config.receiverRequiredIntensity = receiverRequiredIntensity;
        config.countsForWin = countsForWin;
        config.isPushable = isPushable;
        config.isHorizontalPushable = isHorizontalPushable;
        config.isVerticalPushable = isVerticalPushable;
        config.isRotatable = isRotatable;
        config.canSprintPush = canSprintPush;
    }
}
