using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class LightPuzzleLevelBuilderWindow : EditorWindow
{
    private enum BuilderTool
    {
        Select,
        Place,
        Erase
    }

    private const string DefaultPalettePath = "Assets/LevelBuilder/LevelBuilderPalette.asset";
    private const string DefaultDefinitionsFolder = "Assets/LevelBuilder/Definitions";
    private const float LeftPanelWidth = 280f;
    private const float RightPanelWidth = 430f;

    private LevelBuilderPalette palette;
    private GameObject sourceLevelPrefab;
    private LevelObjectDefinition selectedDefinition;
    private BuilderTool activeTool = BuilderTool.Place;

    private readonly List<LevelEntityConfig> entities = new List<LevelEntityConfig>();
    private Vector2Int gridSize = new Vector2Int(16, 10);
    private float cellSize = 1f;
    private float cellPixels = 36f;
    private Vector2 gridPan;
    private bool replaceCellOnPlace = true;

    private string levelName = "Level_New";
    private string exportFolder = "Assets/Prefab/Levels";
    private int selectedEntityIndex = -1;
    private int nextEntityNumber = 1;
    private Vector2 leftScroll;
    private Vector2 rightScroll;
    private Vector2 tableScroll;
    private GameObject previewRoot;

    [MenuItem("Tools/Light Puzzle/Level Builder")]
    public static void Open()
    {
        GetWindow<LightPuzzleLevelBuilderWindow>("Level Builder");
    }

    private void OnEnable()
    {
        if (palette == null)
        {
            palette = AssetDatabase.LoadAssetAtPath<LevelBuilderPalette>(DefaultPalettePath);
        }

        if (palette != null)
        {
            exportFolder = palette.DefaultExportFolder;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        DrawLeftPanel();
        DrawGridPanel();
        DrawRightPanel();
        EditorGUILayout.EndHorizontal();
        HandleKeyboard();
    }

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(LeftPanelWidth));
        leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

        EditorGUILayout.LabelField("Level Source", EditorStyles.boldLabel);
        sourceLevelPrefab = (GameObject)EditorGUILayout.ObjectField("Level Prefab", sourceLevelPrefab, typeof(GameObject), false);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New Unsaved"))
        {
            NewLevel();
        }

        EditorGUI.BeginDisabledGroup(sourceLevelPrefab == null);
        if (GUILayout.Button("Load Prefab"))
        {
            LoadFromPrefab(sourceLevelPrefab);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Load All Features Test Draft"))
        {
            LoadAllFeaturesTestDraft();
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Palette", EditorStyles.boldLabel);
        palette = (LevelBuilderPalette)EditorGUILayout.ObjectField("Palette", palette, typeof(LevelBuilderPalette), false);

        if (GUILayout.Button("Create/Load Default Palette"))
        {
            palette = CreateDefaultPalette();
            if (palette != null)
            {
                exportFolder = palette.DefaultExportFolder;
            }
        }

        DrawPaletteButtons();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
        gridSize = EditorGUILayout.Vector2IntField("Grid Size", gridSize);
        cellSize = Mathf.Max(0.01f, EditorGUILayout.FloatField("Cell Size", cellSize));
        cellPixels = EditorGUILayout.Slider("Zoom", cellPixels, 18f, 72f);
        replaceCellOnPlace = EditorGUILayout.Toggle("Replace Cell", replaceCellOnPlace);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
        activeTool = (BuilderTool)GUILayout.Toolbar((int)activeTool, Enum.GetNames(typeof(BuilderTool)));
        selectedDefinition = (LevelObjectDefinition)EditorGUILayout.ObjectField("Brush", selectedDefinition, typeof(LevelObjectDefinition), false);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate Scene Preview"))
        {
            GeneratePreview();
        }

        if (GUILayout.Button("Clear Preview"))
        {
            ClearPreview();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawGridPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        Rect gridRect = GUILayoutUtility.GetRect(420f, 620f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        DrawGrid(gridRect);
        HandleGridInput(gridRect);
        EditorGUILayout.EndVertical();
    }

    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(RightPanelWidth));
        rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

        EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
        levelName = EditorGUILayout.TextField("Level Name", levelName);
        exportFolder = EditorGUILayout.TextField("Export Folder", exportFolder);

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(sourceLevelPrefab == null);
        if (GUILayout.Button("Overwrite Prefab"))
        {
            ExportOverwrite();
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Save As New"))
        {
            ExportAsNew();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);
        DrawValidation();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Level Data", EditorStyles.boldLabel);
        DrawEntityTable();

        EditorGUILayout.Space(8f);
        DrawSelectedEntityInspector();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawPaletteButtons()
    {
        if (palette == null)
        {
            EditorGUILayout.HelpBox("Assign or create a LevelBuilderPalette first.", MessageType.Info);
            return;
        }

        IReadOnlyList<LevelObjectDefinition> definitions = palette.Definitions;
        if (definitions == null || definitions.Count == 0)
        {
            EditorGUILayout.HelpBox("Palette has no definitions.", MessageType.Warning);
            return;
        }

        for (int i = 0; i < definitions.Count; i++)
        {
            LevelObjectDefinition definition = definitions[i];
            if (definition == null)
            {
                continue;
            }

            GUI.backgroundColor = definition.EditorColor;
            bool clicked = GUILayout.Button(definition.EntityId + "  " + definition.DisplayName, GUILayout.Height(26f));
            GUI.backgroundColor = Color.white;

            if (clicked)
            {
                selectedDefinition = definition;
                activeTool = BuilderTool.Place;
            }
        }
    }

    private void DrawGrid(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f));

        Vector2 origin = rect.center + gridPan;
        int minX = -gridSize.x / 2;
        int maxX = minX + gridSize.x - 1;
        int minY = -gridSize.y / 2;
        int maxY = minY + gridSize.y - 1;

        Handles.BeginGUI();
        Color minorLine = new Color(1f, 1f, 1f, 0.08f);
        Color majorLine = new Color(1f, 1f, 1f, 0.18f);

        for (int x = minX; x <= maxX + 1; x++)
        {
            float px = origin.x + (x - 0.5f) * cellPixels;
            Handles.color = x == 0 ? majorLine : minorLine;
            Handles.DrawLine(new Vector3(px, rect.yMin), new Vector3(px, rect.yMax));
        }

        for (int y = minY; y <= maxY + 1; y++)
        {
            float py = origin.y - (y - 0.5f) * cellPixels;
            Handles.color = y == 0 ? majorLine : minorLine;
            Handles.DrawLine(new Vector3(rect.xMin, py), new Vector3(rect.xMax, py));
        }

        Handles.color = new Color(0.3f, 0.85f, 1f, 0.5f);
        Handles.DrawLine(new Vector3(origin.x - 7f, origin.y), new Vector3(origin.x + 7f, origin.y));
        Handles.DrawLine(new Vector3(origin.x, origin.y - 7f), new Vector3(origin.x, origin.y + 7f));
        Handles.EndGUI();

        for (int i = 0; i < entities.Count; i++)
        {
            LevelEntityConfig entity = entities[i];
            Rect cellRect = GetCellRect(rect, entity.gridPosition);
            Color color = entity.definition != null ? entity.definition.EditorColor : new Color(0.6f, 0.6f, 0.6f);
            color.a = selectedEntityIndex == i ? 0.95f : 0.75f;
            EditorGUI.DrawRect(cellRect, color);

            string label = string.IsNullOrWhiteSpace(entity.entityId) ? "?" : entity.entityId;
            label = label.Replace("OBJ_", string.Empty).Replace("LIG_", string.Empty).Replace("ENE_", string.Empty);
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.black },
                fontSize = cellPixels >= 34f ? 10 : 8
            };
            GUI.Label(cellRect, label, style);

            if (selectedEntityIndex == i)
            {
                Handles.BeginGUI();
                Handles.color = Color.yellow;
                Handles.DrawAAPolyLine(3f,
                    new Vector3(cellRect.xMin, cellRect.yMin),
                    new Vector3(cellRect.xMax, cellRect.yMin),
                    new Vector3(cellRect.xMax, cellRect.yMax),
                    new Vector3(cellRect.xMin, cellRect.yMax),
                    new Vector3(cellRect.xMin, cellRect.yMin)
                );
                Handles.EndGUI();
            }
        }

        GUI.Label(new Rect(rect.xMin + 8f, rect.yMin + 6f, 360f, 20f), "Left click: place/select | Right click: erase | Middle drag: pan | Drag definition asset onto grid");
    }

    private void HandleGridInput(Rect rect)
    {
        Event current = Event.current;
        if (!rect.Contains(current.mousePosition))
        {
            return;
        }

        if (HandleDefinitionDrag(rect, current))
        {
            return;
        }

        if (current.type == EventType.MouseDrag && current.button == 2)
        {
            gridPan += current.delta;
            current.Use();
            Repaint();
            return;
        }

        if (current.type != EventType.MouseDown)
        {
            return;
        }

        Vector2Int cell = MouseToGrid(rect, current.mousePosition);

        if (current.button == 1 || activeTool == BuilderTool.Erase)
        {
            EraseAt(cell);
            current.Use();
            return;
        }

        if (current.button != 0)
        {
            return;
        }

        if (activeTool == BuilderTool.Place && selectedDefinition != null)
        {
            PlaceAt(cell, selectedDefinition);
        }
        else
        {
            selectedEntityIndex = FindEntityIndexAt(cell);
        }

        current.Use();
        Repaint();
    }

    private bool HandleDefinitionDrag(Rect rect, Event current)
    {
        if (current.type != EventType.DragUpdated && current.type != EventType.DragPerform)
        {
            return false;
        }

        LevelObjectDefinition draggedDefinition = GetDraggedDefinition();
        if (draggedDefinition == null)
        {
            return false;
        }

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

        if (current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            selectedDefinition = draggedDefinition;
            PlaceAt(MouseToGrid(rect, current.mousePosition), draggedDefinition);
        }

        current.Use();
        return true;
    }

    private LevelObjectDefinition GetDraggedDefinition()
    {
        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
        {
            if (DragAndDrop.objectReferences[i] is LevelObjectDefinition definition)
            {
                return definition;
            }
        }

        return null;
    }

    private void DrawEntityTable()
    {
        tableScroll = EditorGUILayout.BeginScrollView(tableScroll, GUILayout.MinHeight(180f), GUILayout.MaxHeight(280f));

        for (int i = 0; i < entities.Count; i++)
        {
            LevelEntityConfig entity = entities[i];
            GUI.backgroundColor = selectedEntityIndex == i ? new Color(1f, 0.95f, 0.55f) : Color.white;
            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select", GUILayout.Width(54f)))
            {
                selectedEntityIndex = i;
            }

            entity.displayName = EditorGUILayout.TextField(entity.displayName, GUILayout.Width(110f));
            entity.entityId = EditorGUILayout.TextField(entity.entityId, GUILayout.Width(78f));
            entity.gridPosition.x = EditorGUILayout.IntField(entity.gridPosition.x, GUILayout.Width(42f));
            entity.gridPosition.y = EditorGUILayout.IntField(entity.gridPosition.y, GUILayout.Width(42f));
            entity.rotationZ = EditorGUILayout.FloatField(entity.rotationZ, GUILayout.Width(52f));

            if (GUILayout.Button("Del", GUILayout.Width(40f)))
            {
                entities.RemoveAt(i);
                if (selectedEntityIndex >= entities.Count)
                {
                    selectedEntityIndex = entities.Count - 1;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Def", GUILayout.Width(26f));
            LevelObjectDefinition newDefinition = (LevelObjectDefinition)EditorGUILayout.ObjectField(entity.definition, typeof(LevelObjectDefinition), false);
            if (newDefinition != entity.definition)
            {
                ApplyDefinitionToExistingEntity(entity, newDefinition);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSelectedEntityInspector()
    {
        if (selectedEntityIndex < 0 || selectedEntityIndex >= entities.Count)
        {
            EditorGUILayout.HelpBox("Select an entity to edit exact gameplay data.", MessageType.Info);
            return;
        }

        LevelEntityConfig entity = entities[selectedEntityIndex];
        EditorGUILayout.LabelField("Selected Entity", EditorStyles.boldLabel);
        entity.localId = EditorGUILayout.TextField("Local Id", entity.localId);
        entity.displayName = EditorGUILayout.TextField("Name", entity.displayName);
        entity.sourcePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", entity.sourcePrefab, typeof(GameObject), false);

        EditorGUILayout.Space(4f);
        entity.gridPosition = EditorGUILayout.Vector2IntField("Grid Position", entity.gridPosition);
        entity.rotationZ = EditorGUILayout.FloatField("Rotation Z", entity.rotationZ);

        EditorGUILayout.Space(4f);
        entity.configureLightSource = EditorGUILayout.Toggle("Configure Light", entity.configureLightSource);
        if (entity.configureLightSource)
        {
            entity.lightColor = (LightColorChannel)EditorGUILayout.EnumFlagsField("Light Color", entity.lightColor);
            entity.lightIntensity = EditorGUILayout.FloatField("Light Intensity", entity.lightIntensity);
        }

        entity.configureReceiver = EditorGUILayout.Toggle("Configure Receiver", entity.configureReceiver);
        if (entity.configureReceiver)
        {
            entity.receiverRequiresColor = EditorGUILayout.Toggle("Requires Color", entity.receiverRequiresColor);
            entity.receiverRequiresExactColor = EditorGUILayout.Toggle("Exact Color", entity.receiverRequiresExactColor);
            entity.receiverRequiredColor = (LightColorChannel)EditorGUILayout.EnumFlagsField("Required Color", entity.receiverRequiredColor);
            entity.receiverRequiredIntensity = EditorGUILayout.FloatField("Required Intensity", entity.receiverRequiredIntensity);
            entity.countsForWin = EditorGUILayout.Toggle("Counts For Win", entity.countsForWin);
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Push/Rotate", EditorStyles.boldLabel);
        entity.isPushable = EditorGUILayout.Toggle("Pushable", entity.isPushable);
        entity.isHorizontalPushable = EditorGUILayout.Toggle("Horizontal Push", entity.isHorizontalPushable);
        entity.isVerticalPushable = EditorGUILayout.Toggle("Vertical Push", entity.isVerticalPushable);
        entity.isRotatable = EditorGUILayout.Toggle("Rotatable", entity.isRotatable);
        entity.canSprintPush = EditorGUILayout.Toggle("Sprint Push", entity.canSprintPush);

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Logic Nodes", EditorStyles.boldLabel);
        entity.mergeColorChannels = EditorGUILayout.Toggle("Merge Color Channels", entity.mergeColorChannels);
        entity.splitIntoRgbComponents = EditorGUILayout.Toggle("Split Into RGB", entity.splitIntoRgbComponents);

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Enemy Clear", EditorStyles.boldLabel);
        entity.enemyRequiresColor = EditorGUILayout.Toggle("Enemy Requires Color", entity.enemyRequiresColor);
        entity.enemyRequiresExactColor = EditorGUILayout.Toggle("Enemy Exact Color", entity.enemyRequiresExactColor);
        entity.enemyRequiredColor = (LightColorChannel)EditorGUILayout.EnumFlagsField("Enemy Color", entity.enemyRequiredColor);
        entity.enemyRequiredIntensity = EditorGUILayout.FloatField("Enemy Intensity", entity.enemyRequiredIntensity);

        EditorGUILayout.Space(4f);
        entity.linkedDoorIds = EditorGUILayout.TextField("Linked Door Ids", entity.linkedDoorIds);
        string patrolText = PatrolPointsToText(entity.patrolPoints);
        string newPatrolText = EditorGUILayout.TextField("Patrol Points", patrolText);
        if (newPatrolText != patrolText)
        {
            entity.patrolPoints = ParsePatrolPoints(newPatrolText);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate -90"))
        {
            entity.rotationZ -= 90f;
        }

        if (GUILayout.Button("Rotate +90"))
        {
            entity.rotationZ += 90f;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawValidation()
    {
        List<string> issues = ValidateDraft();
        if (issues.Count == 0)
        {
            EditorGUILayout.HelpBox("No blocking validation issues.", MessageType.Info);
            return;
        }

        string message = string.Join("\n", issues);
        EditorGUILayout.HelpBox(message, MessageType.Warning);
    }

    private List<string> ValidateDraft()
    {
        List<string> issues = new List<string>();
        bool hasPlayer = false;
        bool hasGoal = false;

        for (int i = 0; i < entities.Count; i++)
        {
            LevelEntityConfig entity = entities[i];

            if (entity == null)
            {
                continue;
            }

            hasPlayer |= entity.entityId == "PL_001";
            hasGoal |= entity.entityId == "OBJ_GOA" || entity.countsForWin;

            if (string.IsNullOrWhiteSpace(entity.localId))
            {
                issues.Add("Entity " + i + " is missing Local Id.");
            }

            if (string.IsNullOrWhiteSpace(entity.entityId))
            {
                issues.Add("Entity " + entity.displayName + " is missing Entity Id.");
            }

            if (entity.sourcePrefab == null && entity.definition == null)
            {
                issues.Add(entity.displayName + " has no prefab/definition; fallback object will be generated.");
            }
        }

        if (!hasPlayer)
        {
            issues.Add("No PL_001 player spawn/player entity found.");
        }

        if (!hasGoal)
        {
            issues.Add("No goal/win receiver found.");
        }

        return issues;
    }

    private void NewLevel()
    {
        sourceLevelPrefab = null;
        entities.Clear();
        selectedEntityIndex = -1;
        nextEntityNumber = 1;
        levelName = "Level_New";
        gridSize = new Vector2Int(16, 10);
        cellSize = 1f;
    }

    private void LoadAllFeaturesTestDraft()
    {
        if (palette == null)
        {
            palette = CreateDefaultPalette();
        }

        NewLevel();
        levelName = "Level_AllFeatures_Test";
        gridSize = new Vector2Int(20, 12);

        AddDraftEntity("PL_001", "Player_01", new Vector2Int(-8, -4), 0f);

        LevelEntityConfig redLight = AddDraftEntity("LIG_SRC", "RedLight_01", new Vector2Int(-8, 2), 0f);
        redLight.configureLightSource = true;
        redLight.lightColor = LightColorChannel.Red;
        redLight.lightIntensity = 10f;

        LevelEntityConfig greenLight = AddDraftEntity("LIG_SRC", "GreenLight_01", new Vector2Int(-8, -1), 0f);
        greenLight.configureLightSource = true;
        greenLight.lightColor = LightColorChannel.Green;
        greenLight.lightIntensity = 10f;

        AddDraftEntity("OBJ_MIR", "Mirror_01", new Vector2Int(-4, 2), 45f);
        LevelEntityConfig merger = AddDraftEntity("OBJ_MER", "Merger_01", new Vector2Int(-1, 1), 0f);
        merger.mergeColorChannels = true;

        LevelEntityConfig splitter = AddDraftEntity("OBJ_SPL", "Splitter_01", new Vector2Int(2, 1), 0f);
        splitter.splitIntoRgbComponents = true;

        LevelEntityConfig converter = AddDraftEntity("OBJ_CON", "Converter_Blue_01", new Vector2Int(4, 2), 0f);
        converter.lightColor = LightColorChannel.Blue;

        LevelEntityConfig goal = AddDraftEntity("OBJ_GOA", "Goal_Blue_01", new Vector2Int(8, 2), 0f);
        goal.configureReceiver = true;
        goal.receiverRequiresColor = true;
        goal.receiverRequiresExactColor = true;
        goal.receiverRequiredColor = LightColorChannel.Blue;
        goal.receiverRequiredIntensity = 5f;
        goal.countsForWin = true;

        LevelEntityConfig door = AddDraftEntity("OBJ_DOR", "Door_01", new Vector2Int(6, -2), 0f);
        door.linkedDoorIds = string.Empty;

        LevelEntityConfig doorSwitch = AddDraftEntity("OBJ_SWI", "Switch_01", new Vector2Int(2, -2), 0f);
        doorSwitch.configureReceiver = true;
        doorSwitch.receiverRequiresColor = true;
        doorSwitch.receiverRequiresExactColor = true;
        doorSwitch.receiverRequiredColor = LightColorChannel.Yellow;
        doorSwitch.receiverRequiredIntensity = 8f;
        doorSwitch.countsForWin = false;
        doorSwitch.linkedDoorIds = "Door_01";

        LevelEntityConfig enemyBlocker = AddDraftEntity("ENE_001", "EnemyBlocker_Red_01", new Vector2Int(0, 4), 0f);
        enemyBlocker.enemyRequiresColor = true;
        enemyBlocker.enemyRequiresExactColor = true;
        enemyBlocker.enemyRequiredColor = LightColorChannel.Red;
        enemyBlocker.enemyRequiredIntensity = 5f;

        LevelEntityConfig destroyer = AddDraftEntity("ENE_002", "EnemyDestroyer_01", new Vector2Int(-3, -3), 0f);
        destroyer.patrolPoints = new[]
        {
            new Vector2(-3f, -3f),
            new Vector2(0f, -3f),
            new Vector2(0f, -1f),
            new Vector2(-3f, -1f)
        };

        for (int x = -9; x <= 9; x++)
        {
            AddDraftEntity("OBJ_WAL", "Wall_Top_" + x, new Vector2Int(x, 5), 0f);
            AddDraftEntity("OBJ_WAL", "Wall_Bottom_" + x, new Vector2Int(x, -5), 0f);
        }

        for (int y = -4; y <= 4; y++)
        {
            AddDraftEntity("OBJ_WAL", "Wall_Left_" + y, new Vector2Int(-9, y), 0f);
            AddDraftEntity("OBJ_WAL", "Wall_Right_" + y, new Vector2Int(9, y), 0f);
        }

        selectedEntityIndex = entities.Count > 0 ? 0 : -1;
        Repaint();
    }

    private LevelEntityConfig AddDraftEntity(string entityId, string displayName, Vector2Int position, float rotationZ)
    {
        LevelObjectDefinition definition = palette != null ? palette.FindById(entityId) : null;
        LevelEntityConfig config = new LevelEntityConfig
        {
            localId = "E" + nextEntityNumber.ToString("000"),
            entityId = entityId,
            displayName = displayName,
            gridPosition = position,
            rotationZ = rotationZ
        };
        nextEntityNumber++;

        if (definition != null)
        {
            definition.ApplyDefaults(config);
            config.displayName = displayName;
            config.gridPosition = position;
            config.rotationZ = rotationZ;
        }

        entities.Add(config);
        return config;
    }

    private void PlaceAt(Vector2Int cell, LevelObjectDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        if (replaceCellOnPlace)
        {
            EraseAt(cell, false);
        }

        LevelEntityConfig entity = new LevelEntityConfig
        {
            localId = "E" + nextEntityNumber.ToString("000"),
            gridPosition = cell
        };
        nextEntityNumber++;

        definition.ApplyDefaults(entity);
        entity.displayName = MakeUniqueEntityName(definition.DisplayName);

        entities.Add(entity);
        selectedEntityIndex = entities.Count - 1;
    }

    private void ApplyDefinitionToExistingEntity(LevelEntityConfig entity, LevelObjectDefinition definition)
    {
        if (entity == null)
        {
            return;
        }

        Vector2Int oldPosition = entity.gridPosition;
        string oldLocalId = entity.localId;
        string oldName = entity.displayName;

        if (definition != null)
        {
            definition.ApplyDefaults(entity);
        }

        entity.definition = definition;
        entity.gridPosition = oldPosition;
        entity.localId = oldLocalId;

        if (!string.IsNullOrWhiteSpace(oldName))
        {
            entity.displayName = oldName;
        }
    }

    private void EraseAt(Vector2Int cell, bool repaint = true)
    {
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            if (entities[i].gridPosition == cell)
            {
                entities.RemoveAt(i);
                if (selectedEntityIndex == i)
                {
                    selectedEntityIndex = -1;
                }
                else if (selectedEntityIndex > i)
                {
                    selectedEntityIndex--;
                }

                break;
            }
        }

        if (repaint)
        {
            Repaint();
        }
    }

    private int FindEntityIndexAt(Vector2Int cell)
    {
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            if (entities[i].gridPosition == cell)
            {
                return i;
            }
        }

        return -1;
    }

    private Rect GetCellRect(Rect gridRect, Vector2Int cell)
    {
        Vector2 origin = gridRect.center + gridPan;
        Vector2 center = new Vector2(origin.x + cell.x * cellPixels, origin.y - cell.y * cellPixels);
        return new Rect(center.x - cellPixels * 0.45f, center.y - cellPixels * 0.45f, cellPixels * 0.9f, cellPixels * 0.9f);
    }

    private Vector2Int MouseToGrid(Rect gridRect, Vector2 mousePosition)
    {
        Vector2 origin = gridRect.center + gridPan;
        Vector2 local = mousePosition - origin;
        int x = Mathf.RoundToInt(local.x / cellPixels);
        int y = Mathf.RoundToInt(-local.y / cellPixels);
        return new Vector2Int(x, y);
    }

    private void HandleKeyboard()
    {
        Event current = Event.current;
        if (current.type != EventType.KeyDown)
        {
            return;
        }

        if (selectedEntityIndex < 0 || selectedEntityIndex >= entities.Count)
        {
            return;
        }

        LevelEntityConfig entity = entities[selectedEntityIndex];
        bool used = true;

        switch (current.keyCode)
        {
            case KeyCode.Q:
                entity.rotationZ -= 90f;
                break;
            case KeyCode.E:
                entity.rotationZ += 90f;
                break;
            case KeyCode.Delete:
            case KeyCode.Backspace:
                entities.RemoveAt(selectedEntityIndex);
                selectedEntityIndex = Mathf.Min(selectedEntityIndex, entities.Count - 1);
                break;
            case KeyCode.UpArrow:
                entity.gridPosition += Vector2Int.up;
                break;
            case KeyCode.DownArrow:
                entity.gridPosition += Vector2Int.down;
                break;
            case KeyCode.LeftArrow:
                entity.gridPosition += Vector2Int.left;
                break;
            case KeyCode.RightArrow:
                entity.gridPosition += Vector2Int.right;
                break;
            default:
                used = false;
                break;
        }

        if (used)
        {
            current.Use();
            Repaint();
        }
    }

    private void LoadFromPrefab(GameObject prefab)
    {
        string path = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrWhiteSpace(path))
        {
            EditorUtility.DisplayDialog("Load Level", "Selected object is not a prefab asset.", "OK");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            nextEntityNumber = 1;
            LevelDefinition definition = root.GetComponent<LevelDefinition>();
            if (definition != null)
            {
                levelName = definition.LevelId;
                gridSize = definition.GridSize;
                cellSize = definition.CellSize;
            }
            else
            {
                levelName = Path.GetFileNameWithoutExtension(path);
            }

            entities.Clear();
            LevelEntity[] levelEntities = root.GetComponentsInChildren<LevelEntity>(true);
            for (int i = 0; i < levelEntities.Length; i++)
            {
                LevelEntityConfig config = levelEntities[i].Config.Clone();
                if (string.IsNullOrWhiteSpace(config.localId))
                {
                    config.localId = "E" + nextEntityNumber.ToString("000");
                }

                if (config.definition == null && palette != null)
                {
                    config.definition = palette.FindById(config.entityId);
                }

                entities.Add(config);
            }

            selectedEntityIndex = entities.Count > 0 ? 0 : -1;
            nextEntityNumber = Mathf.Max(nextEntityNumber, entities.Count + 1);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private void GeneratePreview()
    {
        ClearPreview();
        previewRoot = BuildLevelRoot("[Preview] " + levelName);
        Selection.activeGameObject = previewRoot;
    }

    private void ClearPreview()
    {
        if (previewRoot != null)
        {
            DestroyImmediate(previewRoot);
            previewRoot = null;
        }
    }

    private void ExportOverwrite()
    {
        if (sourceLevelPrefab == null)
        {
            return;
        }

        string path = AssetDatabase.GetAssetPath(sourceLevelPrefab);
        if (string.IsNullOrWhiteSpace(path))
        {
            EditorUtility.DisplayDialog("Export Level", "Source level is not a prefab asset.", "OK");
            return;
        }

        ExportToPath(path);
    }

    private void ExportAsNew()
    {
        EnsureAssetFolder(exportFolder);
        string safeName = MakeSafeFileName(levelName);
        string path = AssetDatabase.GenerateUniqueAssetPath(exportFolder.TrimEnd('/') + "/" + safeName + ".prefab");
        ExportToPath(path);
    }

    private void ExportToPath(string path)
    {
        if (entities.Count == 0)
        {
            if (!EditorUtility.DisplayDialog("Export Empty Level", "This level has no entities. Export anyway?", "Export", "Cancel"))
            {
                return;
            }
        }

        GameObject root = BuildLevelRoot(levelName);
        try
        {
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            sourceLevelPrefab = savedPrefab;
            EditorUtility.DisplayDialog("Export Level", "Saved level prefab:\n" + path, "OK");
        }
        finally
        {
            DestroyImmediate(root);
        }
    }

    private GameObject BuildLevelRoot(string rootName)
    {
        GameObject root = new GameObject(string.IsNullOrWhiteSpace(rootName) ? "Level_New" : rootName);
        LevelDefinition definition = root.AddComponent<LevelDefinition>();

        GameObject entitiesRoot = new GameObject("Entities");
        entitiesRoot.transform.SetParent(root.transform, false);
        definition.Configure(levelName, gridSize, cellSize, entitiesRoot.transform);

        List<GeneratedEntity> generatedEntities = new List<GeneratedEntity>();
        Dictionary<string, GameObject> lookup = new Dictionary<string, GameObject>();

        for (int i = 0; i < entities.Count; i++)
        {
            LevelEntityConfig config = entities[i].Clone();
            GameObject instance = CreateEntityGameObject(config);
            instance.transform.SetParent(entitiesRoot.transform, false);
            instance.transform.localPosition = new Vector3(config.gridPosition.x * cellSize, config.gridPosition.y * cellSize, 0f);
            instance.transform.localRotation = Quaternion.Euler(0f, 0f, config.rotationZ);
            instance.name = string.IsNullOrWhiteSpace(config.displayName) ? config.entityId : config.displayName;

            LevelEntity levelEntity = instance.GetComponent<LevelEntity>();
            if (levelEntity == null)
            {
                levelEntity = instance.AddComponent<LevelEntity>();
            }
            levelEntity.SetConfig(config);

            ApplyGameplayConfig(instance, config);

            generatedEntities.Add(new GeneratedEntity(config, instance));
            AddLookup(lookup, config.localId, instance);
            AddLookup(lookup, config.displayName, instance);
            AddLookup(lookup, config.entityId, instance);
        }

        ResolveGeneratedLinks(generatedEntities, lookup);
        definition.SetEntitySnapshot(CloneConfigs(entities));
        return root;
    }

    private GameObject CreateEntityGameObject(LevelEntityConfig config)
    {
        GameObject prefab = config.sourcePrefab != null
            ? config.sourcePrefab
            : config.definition != null
                ? config.definition.Prefab
                : null;

        GameObject instance = null;

        if (prefab != null)
        {
            instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                instance = Instantiate(prefab);
            }
        }

        if (instance == null)
        {
            instance = new GameObject(string.IsNullOrWhiteSpace(config.displayName) ? config.entityId : config.displayName);
            AddFallbackComponents(instance, config);
        }

        EnsureRequiredComponents(instance, config);
        return instance;
    }

    private void AddFallbackComponents(GameObject instance, LevelEntityConfig config)
    {
        LevelObjectKind kind = config.definition != null ? config.definition.Kind : LevelObjectKind.Custom;

        switch (kind)
        {
            case LevelObjectKind.LightSource:
                instance.AddComponent<LightSource>();
                break;
            case LevelObjectKind.Goal:
                instance.layer = 7;
                instance.AddComponent<BoxCollider2D>();
                instance.AddComponent<LightReceiver>();
                break;
            case LevelObjectKind.ColorConverter:
                instance.layer = 7;
                instance.AddComponent<BoxCollider2D>();
                instance.AddComponent<LightColorConverter>();
                break;
            case LevelObjectKind.DoorSwitch:
                instance.layer = 7;
                instance.AddComponent<BoxCollider2D>();
                instance.AddComponent<LightReceiver>();
                instance.AddComponent<DoorSwitch>();
                break;
            case LevelObjectKind.Door:
                instance.layer = 7;
                instance.AddComponent<BoxCollider2D>();
                instance.AddComponent<DoorBlocker>();
                break;
            case LevelObjectKind.EnemyBlocker:
                instance.layer = 7;
                instance.AddComponent<BoxCollider2D>();
                instance.AddComponent<EnemyBlocker>();
                break;
            case LevelObjectKind.EnemyDestroyer:
                instance.layer = 7;
                instance.AddComponent<Rigidbody2D>();
                instance.AddComponent<BoxCollider2D>();
                instance.AddComponent<EnemyDestroyer>();
                break;
            case LevelObjectKind.Wall:
                instance.layer = 7;
                instance.AddComponent<BoxCollider2D>();
                break;
        }
    }

    private void EnsureRequiredComponents(GameObject instance, LevelEntityConfig config)
    {
        LevelObjectKind kind = config.definition != null ? config.definition.Kind : LevelObjectKind.Custom;

        switch (kind)
        {
            case LevelObjectKind.LightSource:
                AddComponentIfMissing<LightSource>(instance);
                break;
            case LevelObjectKind.Goal:
                instance.layer = 7;
                AddComponentIfMissing<BoxCollider2D>(instance);
                AddComponentIfMissing<LightReceiver>(instance);
                break;
            case LevelObjectKind.ColorConverter:
                instance.layer = 7;
                AddComponentIfMissing<BoxCollider2D>(instance);
                AddComponentIfMissing<LightColorConverter>(instance);
                break;
            case LevelObjectKind.DoorSwitch:
                instance.layer = 7;
                AddComponentIfMissing<BoxCollider2D>(instance);
                AddComponentIfMissing<LightReceiver>(instance);
                AddComponentIfMissing<DoorSwitch>(instance);
                break;
            case LevelObjectKind.Door:
                instance.layer = 7;
                AddComponentIfMissing<BoxCollider2D>(instance);
                AddComponentIfMissing<DoorBlocker>(instance);
                break;
            case LevelObjectKind.EnemyBlocker:
                instance.layer = 7;
                AddComponentIfMissing<BoxCollider2D>(instance);
                AddComponentIfMissing<EnemyBlocker>(instance);
                break;
            case LevelObjectKind.EnemyDestroyer:
                instance.layer = 7;
                AddComponentIfMissing<Rigidbody2D>(instance);
                AddComponentIfMissing<BoxCollider2D>(instance);
                AddComponentIfMissing<EnemyDestroyer>(instance);
                break;
            case LevelObjectKind.Wall:
                instance.layer = 7;
                AddComponentIfMissing<BoxCollider2D>(instance);
                break;
        }
    }

    private static T AddComponentIfMissing<T>(GameObject instance) where T : Component
    {
        T component = instance.GetComponentInChildren<T>(true);
        if (component == null)
        {
            component = instance.AddComponent<T>();
        }

        return component;
    }

    private void ApplyGameplayConfig(GameObject instance, LevelEntityConfig config)
    {
        LightSource lightSource = instance.GetComponentInChildren<LightSource>(true);
        if (config.configureLightSource && lightSource != null)
        {
            lightSource.Configure(config.lightColor, config.lightIntensity);
        }

        LightReceiver lightReceiver = instance.GetComponentInChildren<LightReceiver>(true);
        if (lightReceiver != null)
        {
            if (config.configureReceiver)
            {
                lightReceiver.ConfigureRequirement(
                    config.receiverRequiredIntensity,
                    config.receiverRequiresColor,
                    config.receiverRequiredColor,
                    config.receiverRequiresExactColor
                );
            }

            lightReceiver.SetCountsForWin(config.countsForWin);
        }

        PushableMirror mirror = instance.GetComponentInChildren<PushableMirror>(true);
        if (mirror != null)
        {
            SetSerializedBool(mirror, "isPushable", config.isPushable);
            SetSerializedBool(mirror, "isHorizontalPushable", config.isHorizontalPushable);
            SetSerializedBool(mirror, "isVerticalPushable", config.isVerticalPushable);
            SetSerializedBool(mirror, "isRotationable", config.isRotatable);
            SetSerializedBool(mirror, "canSprintPush", config.canSprintPush);
        }

        LightColorConverter converter = instance.GetComponentInChildren<LightColorConverter>(true);
        if (converter != null)
        {
            SetSerializedEnum(converter, "outputColor", (int)config.lightColor);
        }

        LightMerger merger = instance.GetComponentInChildren<LightMerger>(true);
        if (merger != null)
        {
            SetSerializedBool(merger, "mergeColorChannels", config.mergeColorChannels);
        }

        LightSplitter splitter = instance.GetComponentInChildren<LightSplitter>(true);
        if (splitter != null)
        {
            SetSerializedBool(splitter, "splitIntoRgbComponents", config.splitIntoRgbComponents);
        }

        EnemyBlocker enemyBlocker = instance.GetComponentInChildren<EnemyBlocker>(true);
        if (enemyBlocker != null)
        {
            enemyBlocker.ConfigureRequirement(
                config.enemyRequiresColor,
                config.enemyRequiredColor,
                config.enemyRequiresExactColor,
                config.enemyRequiredIntensity
            );
        }
    }

    private void ResolveGeneratedLinks(List<GeneratedEntity> generatedEntities, Dictionary<string, GameObject> lookup)
    {
        for (int i = 0; i < generatedEntities.Count; i++)
        {
            LevelEntityConfig config = generatedEntities[i].config;
            GameObject instance = generatedEntities[i].gameObject;

            DoorSwitch doorSwitch = instance.GetComponentInChildren<DoorSwitch>(true);
            if (doorSwitch != null)
            {
                DoorBlocker[] doors = ResolveDoorLinks(config.linkedDoorIds, lookup);
                doorSwitch.ConfigureLinkedDoors(doors);
                SetSerializedObjectArray(doorSwitch, "linkedDoors", doors);
            }

            EnemyDestroyer destroyer = instance.GetComponentInChildren<EnemyDestroyer>(true);
            if (destroyer != null && config.patrolPoints != null && config.patrolPoints.Length > 0)
            {
                Transform[] patrolPoints = CreatePatrolPointChildren(instance.transform, config.patrolPoints);
                destroyer.ConfigurePatrolPoints(patrolPoints);
                SetSerializedObjectArray(destroyer, "patrolPoints", patrolPoints);
            }
        }
    }

    private DoorBlocker[] ResolveDoorLinks(string linkedDoorIds, Dictionary<string, GameObject> lookup)
    {
        if (string.IsNullOrWhiteSpace(linkedDoorIds))
        {
            return Array.Empty<DoorBlocker>();
        }

        string[] tokens = linkedDoorIds.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        List<DoorBlocker> doors = new List<DoorBlocker>();

        for (int i = 0; i < tokens.Length; i++)
        {
            string key = tokens[i].Trim();
            if (lookup.TryGetValue(key, out GameObject target))
            {
                DoorBlocker door = target.GetComponentInChildren<DoorBlocker>(true);
                if (door != null)
                {
                    doors.Add(door);
                }
            }
        }

        return doors.ToArray();
    }

    private Transform[] CreatePatrolPointChildren(Transform parent, Vector2[] points)
    {
        GameObject patrolRoot = new GameObject("PatrolPoints");
        patrolRoot.transform.SetParent(parent, false);

        Transform[] transforms = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            GameObject point = new GameObject("Patrol_" + (i + 1).ToString("00"));
            point.transform.SetParent(patrolRoot.transform, false);
            point.transform.localPosition = new Vector3(points[i].x * cellSize, points[i].y * cellSize, 0f);
            transforms[i] = point.transform;
        }

        return transforms;
    }

    private static void SetSerializedBool(UnityEngine.Object target, string propertyName, bool value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetSerializedEnum(UnityEngine.Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetSerializedObjectArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.arraySize = values != null ? values.Length : 0;
        for (int i = 0; values != null && i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddLookup(Dictionary<string, GameObject> lookup, string key, GameObject value)
    {
        if (string.IsNullOrWhiteSpace(key) || value == null)
        {
            return;
        }

        lookup[key.Trim()] = value;
    }

    private List<LevelEntityConfig> CloneConfigs(List<LevelEntityConfig> source)
    {
        List<LevelEntityConfig> clones = new List<LevelEntityConfig>();
        for (int i = 0; i < source.Count; i++)
        {
            clones.Add(source[i].Clone());
        }

        return clones;
    }

    private string MakeUniqueEntityName(string baseName)
    {
        string safeBaseName = string.IsNullOrWhiteSpace(baseName) ? "Entity" : baseName.Replace(" ", "_");
        int number = 1;
        string candidate = safeBaseName + "_" + number.ToString("00");

        while (entities.Exists(e => e.displayName == candidate))
        {
            number++;
            candidate = safeBaseName + "_" + number.ToString("00");
        }

        return candidate;
    }

    private static string MakeSafeFileName(string rawName)
    {
        string name = string.IsNullOrWhiteSpace(rawName) ? "Level_New" : rawName.Trim();
        name = Regex.Replace(name, @"[^a-zA-Z0-9_\-]", "_");
        return string.IsNullOrWhiteSpace(name) ? "Level_New" : name;
    }

    private string PatrolPointsToText(Vector2[] points)
    {
        if (points == null || points.Length == 0)
        {
            return string.Empty;
        }

        List<string> tokens = new List<string>();
        for (int i = 0; i < points.Length; i++)
        {
            tokens.Add(points[i].x.ToString("0.##") + "," + points[i].y.ToString("0.##"));
        }

        return string.Join(";", tokens);
    }

    private Vector2[] ParsePatrolPoints(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<Vector2>();
        }

        string[] pointTokens = text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        List<Vector2> points = new List<Vector2>();

        for (int i = 0; i < pointTokens.Length; i++)
        {
            string[] xy = pointTokens[i].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (xy.Length != 2)
            {
                continue;
            }

            if (float.TryParse(xy[0].Trim(), out float x) && float.TryParse(xy[1].Trim(), out float y))
            {
                points.Add(new Vector2(x, y));
            }
        }

        return points.ToArray();
    }

    private static void EnsureAssetFolder(string folder)
    {
        string normalized = folder.Replace("\\", "/").TrimEnd('/');
        if (AssetDatabase.IsValidFolder(normalized))
        {
            return;
        }

        string[] parts = normalized.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private LevelBuilderPalette CreateDefaultPalette()
    {
        EnsureAssetFolder("Assets/LevelBuilder");
        EnsureAssetFolder(DefaultDefinitionsFolder);

        List<LevelObjectDefinition> definitions = new List<LevelObjectDefinition>
        {
            CreateOrUpdateDefinition("LOD_Player", "PL_001", "Player", LevelObjectKind.Player, "Assets/Prefab/Player.prefab", new Color(0.3f, 0.55f, 1f), false, false, false),
            CreateOrUpdateDefinition("LOD_LightSource", "LIG_SRC", "Light Source", LevelObjectKind.LightSource, "Assets/Prefab/LightSource.prefab", new Color(1f, 0.92f, 0.2f), true, false, false),
            CreateOrUpdateDefinition("LOD_Mirror", "OBJ_MIR", "Mirror", LevelObjectKind.Mirror, "Assets/Prefab/ReflectMirror.prefab", new Color(0.5f, 0.9f, 1f), false, false, false),
            CreateOrUpdateDefinition("LOD_Wall", "OBJ_WAL", "Wall", LevelObjectKind.Wall, "Assets/Prefab/Wall.prefab", new Color(0.55f, 0.55f, 0.55f), false, false, false),
            CreateOrUpdateDefinition("LOD_Goal", "OBJ_GOA", "Goal", LevelObjectKind.Goal, "Assets/Prefab/Door.prefab", new Color(0.2f, 1f, 0.45f), false, true, true),
            CreateOrUpdateDefinition("LOD_Merger", "OBJ_MER", "Merger", LevelObjectKind.Merger, "Assets/Prefab/LightMerger.prefab", new Color(1f, 0.55f, 0.2f), false, false, false),
            CreateOrUpdateDefinition("LOD_Splitter", "OBJ_SPL", "Splitter", LevelObjectKind.Splitter, "Assets/Prefab/LightSpliter.prefab", new Color(0.75f, 0.45f, 1f), false, false, false),
            CreateOrUpdateDefinition("LOD_Converter", "OBJ_CON", "Converter", LevelObjectKind.ColorConverter, string.Empty, new Color(1f, 0.25f, 0.85f), true, false, false),
            CreateOrUpdateDefinition("LOD_Switch", "OBJ_SWI", "Door Switch", LevelObjectKind.DoorSwitch, string.Empty, new Color(0.1f, 0.9f, 0.9f), false, true, false),
            CreateOrUpdateDefinition("LOD_Door", "OBJ_DOR", "Door", LevelObjectKind.Door, "Assets/Prefab/Door.prefab", new Color(0.2f, 0.7f, 1f), false, false, false),
            CreateOrUpdateDefinition("LOD_EnemyBlocker", "ENE_001", "Enemy Blocker", LevelObjectKind.EnemyBlocker, string.Empty, new Color(1f, 0.25f, 0.25f), false, false, false),
            CreateOrUpdateDefinition("LOD_EnemyDestroyer", "ENE_002", "Enemy Destroyer", LevelObjectKind.EnemyDestroyer, string.Empty, new Color(0.9f, 0.15f, 0.15f), false, false, false)
        };

        LevelBuilderPalette createdPalette = AssetDatabase.LoadAssetAtPath<LevelBuilderPalette>(DefaultPalettePath);
        if (createdPalette == null)
        {
            createdPalette = CreateInstance<LevelBuilderPalette>();
            AssetDatabase.CreateAsset(createdPalette, DefaultPalettePath);
        }

        SerializedObject serializedPalette = new SerializedObject(createdPalette);
        SerializedProperty exportFolderProperty = serializedPalette.FindProperty("defaultExportFolder");
        if (exportFolderProperty != null)
        {
            exportFolderProperty.stringValue = "Assets/Prefab/Levels";
        }

        SerializedProperty definitionsProperty = serializedPalette.FindProperty("definitions");
        if (definitionsProperty != null)
        {
            definitionsProperty.arraySize = definitions.Count;
            for (int i = 0; i < definitions.Count; i++)
            {
                definitionsProperty.GetArrayElementAtIndex(i).objectReferenceValue = definitions[i];
            }
        }

        serializedPalette.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(createdPalette);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return createdPalette;
    }

    private LevelObjectDefinition CreateOrUpdateDefinition(
        string assetName,
        string entityId,
        string displayName,
        LevelObjectKind kind,
        string prefabPath,
        Color color,
        bool configureLight,
        bool configureReceiver,
        bool countsForWin
    )
    {
        string path = DefaultDefinitionsFolder + "/" + assetName + ".asset";
        LevelObjectDefinition definition = AssetDatabase.LoadAssetAtPath<LevelObjectDefinition>(path);

        if (definition == null)
        {
            definition = CreateInstance<LevelObjectDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }

        SerializedObject serializedDefinition = new SerializedObject(definition);
        SetDefinitionProperty(serializedDefinition, "entityId", entityId);
        SetDefinitionProperty(serializedDefinition, "displayName", displayName);
        SetDefinitionProperty(serializedDefinition, "kind", (int)kind);
        SetDefinitionProperty(serializedDefinition, "prefab", string.IsNullOrWhiteSpace(prefabPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
        SetDefinitionProperty(serializedDefinition, "editorColor", color);
        SetDefinitionProperty(serializedDefinition, "footprint", Vector2Int.one);
        SetDefinitionProperty(serializedDefinition, "configureLightSource", configureLight);
        SetDefinitionProperty(serializedDefinition, "configureReceiver", configureReceiver);
        SetDefinitionProperty(serializedDefinition, "receiverRequiresColor", configureReceiver);
        SetDefinitionProperty(serializedDefinition, "receiverRequiresExactColor", true);
        SetDefinitionProperty(serializedDefinition, "countsForWin", countsForWin);
        serializedDefinition.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(definition);
        return definition;
    }

    private static void SetDefinitionProperty(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetDefinitionProperty(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.enumValueIndex = value;
        }
    }

    private static void SetDefinitionProperty(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetDefinitionProperty(SerializedObject serializedObject, string propertyName, Color value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.colorValue = value;
        }
    }

    private static void SetDefinitionProperty(SerializedObject serializedObject, string propertyName, Vector2Int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.vector2IntValue = value;
        }
    }

    private static void SetDefinitionProperty(SerializedObject serializedObject, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private readonly struct GeneratedEntity
    {
        public readonly LevelEntityConfig config;
        public readonly GameObject gameObject;

        public GeneratedEntity(LevelEntityConfig config, GameObject gameObject)
        {
            this.config = config;
            this.gameObject = gameObject;
        }
    }
}
