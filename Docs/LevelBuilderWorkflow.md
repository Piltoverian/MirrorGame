# Light Puzzle Level Builder Workflow

## Opening The Tool

Open the editor window from:

`Tools > Light Puzzle > Level Builder`

This tool builds levels from grid/table data inside an Editor Window, then exports that data as a level-content prefab. The PlayScene only needs a `LevelLoader` with a level prefab assigned to the `Level Content Prefab` field.

## Creating The Palette

1. Click `Create/Load Default Palette`.
2. The tool creates:
   - `Assets/LevelBuilder/LevelBuilderPalette.asset`
   - `LevelObjectDefinition` assets in `Assets/LevelBuilder/Definitions`
3. The default palette automatically links existing prefabs when they are found:
   - `Assets/Prefab/Player.prefab`
   - `Assets/Prefab/LightSource.prefab`
   - `Assets/Prefab/ReflectMirror.prefab`
   - `Assets/Prefab/Wall.prefab`
   - `Assets/Prefab/Door.prefab`
   - `Assets/Prefab/LightMerger.prefab`
   - `Assets/Prefab/LightSpliter.prefab`

For objects that do not have dedicated prefabs yet, such as converters, switches, and enemies, the tool still creates definitions. When exporting, if a definition has no prefab, the tool generates a fallback GameObject with the required collider/components so the level can be tested quickly.

## Placing Objects On The Grid

1. Select an object from the palette on the left.
2. The tool switches to `Place` mode.
3. Click the grid to place the object.
4. Right-click, or choose `Erase`, to remove the object in a cell.
5. Middle-mouse drag pans the grid.
6. Drag a `LevelObjectDefinition` asset directly onto the grid for quick placement.

Keyboard shortcuts while an entity is selected:

- `Q`: rotate -90 degrees.
- `E`: rotate +90 degrees.
- Arrow keys: move the entity by 1 cell.
- `Delete`/`Backspace`: delete the entity.

To create a ready-made all-features test level, click `Load All Features Test Draft`. The tool populates the grid with a player, light sources, mirror, merger, RGB splitter, converter, goal, switch-door setup, enemies, and walls. Then use `Generate Scene Preview` to inspect it or `Save As New` to export it as a prefab.

## Editing Level Data

The `Level Data` table on the right supports quick edits:

- Name
- Entity ID
- X/Y
- Rotation Z
- Definition

The `Selected Entity` panel edits gameplay configuration:

- Light color/intensity.
- Receiver required color/intensity/counts for win.
- Pushable/rotatable/sprint push.
- Linked Door Ids for switches.
- Patrol Points for enemy destroyers.
- Split Into RGB for splitters.
- Merge Color Channels for mergers.
- Enemy clear color/intensity for enemy blockers.

`Linked Door Ids` accepts `localId` or `displayName`, separated by commas or semicolons. Example:

`E004,Door_02`

`Patrol Points` uses this format:

`0,0;3,0;3,2;0,2`

These points are local grid coordinates around the enemy.

## Scene Preview

- `Generate Scene Preview`: creates preview objects in the current scene.
- `Clear Preview`: removes the preview.

The preview is only for quick inspection in the Scene View. The source of truth remains the data in the Level Builder Window and the exported prefab.

## Exporting A Prefab

When building a new level, no source level prefab is required:

1. Enter `Level Name`.
2. Enter/select `Export Folder`; the default is `Assets/Prefab/Levels`.
3. Click `Save As New`.

When editing an existing prefab:

1. Drag the level prefab into `Level Prefab`.
2. Click `Load Prefab`.
3. Edit the level on the grid/table.
4. Click `Overwrite Prefab` to replace it, or `Save As New` to create a new copy.

Exported prefabs use this structure:

```text
Level_001
  LevelDefinition
  Entities
    Player_01
      LevelEntity
    LightSource_01
      LevelEntity
    Mirror_01
      LevelEntity
```

## Using A Level In PlayScene

1. Create a `LevelLoader` GameObject in the PlayScene.
2. Add the `LevelLoader` component.
3. Drag the exported level prefab into `Level Content Prefab`.
4. Enable `Load On Start`.
5. The PlayScene instantiates the level prefab at runtime.
