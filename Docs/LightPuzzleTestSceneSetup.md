# Light Puzzle Test Scene Setup

## Basic Scene

1. Create a new scene from `Assets/Scenes/PlaySceneTest.unity`, or duplicate an existing level.
2. Make sure the scene contains:
   - `LightRendererPipeLine` with `Light Ray Prefab` assigned to the current line-renderer prefab.
   - `Player` prefab.
   - `GridManager`.
   - `WinningTest` and the win-screen UI if win-condition testing is needed.
3. In `LightRendererPipeLine`, set `Light Collision Mask` to include the layers used by light-blocking colliders. The current project uses layer 7 for wall/door/light utilities and layer 8 for the Player prefab; the default code includes layers 6, 7, and 8.

## Test 1: Travel Time + Player Blocking Light

1. Place a `LightSource` on the left and rotate its local X/right axis toward the right.
2. Place a `Door`, or any object with `LightReceiver`, on the right.
3. Enter Play Mode: the beam should grow over time according to `Default Light Speed` in `LightRendererPipeLine`.
4. Move the Player across the beam path: the beam should stop at the Player, then continue once the Player leaves the path.

## Test 2: RGB Goal Node

1. On `LightSource`, set:
   - `Light Color`: Red, Green, Blue, or White.
   - `Light Luminosity`: for example, 10.
2. On the goal object with `LightReceiver`, set:
   - `Require Specific Color`: true.
   - `Required Color`: the color being tested.
   - `Require Exact Color`: true if the exact color is required.
   - `Lumosity To Open`: the intensity threshold.
   - `Counts For Win`: true.
3. `WinningTest` wins when every `LightReceiver` with `Counts For Win` enabled returns `IsOpened`.

## Test 3: Mirror Reflection + Sprint Push

1. Use the `ReflectMirror` prefab.
2. On `PushableMirror`, suggested values are:
   - `Push Speed`: 1.2 to 1.6.
   - `Sprint Push Speed`: 2.8 to 3.5.
   - `Push Acceleration`: 5 to 8.
   - `Sprint Push Acceleration`: 9 to 12.
   - `Push Deceleration`: 10 to 14.
   - `Can Sprint Push`: true.
3. Enter Play Mode:
   - Use WASD/arrow keys to move.
   - Hold Left Shift while pushing a mirror to test Sprint Push.
   - Use Q/E to rotate according to the `Mirror Sign` setting.

## Test 4: Merge/Split/Converter

1. Merge:
   - Place 2 different-colored `LightSource` objects and aim both beams at `LightMerger`.
   - `Minimum Input Rays`: 2.
   - `Merge Color Channels`: true.
   - Red + Green outputs Yellow, Red + Blue outputs Magenta, Green + Blue outputs Cyan, and RGB outputs White.
2. Split:
   - Place `LightSplitter`.
   - Enable `Split Into Rgb Components` to split White/Yellow/Cyan/Magenta into base-color beams.
   - For explicit output directions, create 3 empty child transforms and assign them to `Configured Outputs`.
3. Converter:
   - Create an object with a collider on the light-collision layer and add `LightColorConverter`.
   - Set `Output Color`.
   - Assign `Light Output` to an empty child placed slightly outside the collider, with local Y/up pointing in the output direction if `Keep Incoming Direction` is false.

## Test 5: Door Switch

1. Create a door blocker:
   - Object with collider + sprite.
   - Add `DoorBlocker`.
2. Create a switch:
   - Object with collider on the light-collision layer.
   - Add `LightReceiver` and `DoorSwitch`.
   - In `LightReceiver`, set the correct color/intensity requirement, and disable `Counts For Win` if the switch is only used to open a door.
   - In `DoorSwitch`, assign `Linked Doors` to the target `DoorBlocker`.
3. When the switch receives the required light, the door collider is disabled and the door visual changes state.

## Test 6: Enemy

1. `EnemyBlocker`:
   - Object with collider on the light-collision layer.
   - Add `EnemyBlocker`.
   - Set `Required Color` and `Luminosity To Destroy`.
   - When it receives enough matching light, the enemy is cleared/destroyed.
2. `EnemyDestroyer`:
   - Object with collider + `Rigidbody2D`.
   - Add `EnemyDestroyer`.
   - Create empty patrol-point objects and assign them to `Patrol Points`.
   - Create `SoftLockManager` in the scene and assign the retry/soft-lock panel if available.
   - When the enemy is hit by light, it finds the nearest mirror, breaks it, and triggers soft-lock.

## Test 7: JSON Spawn

1. Create a `LevelSpawner` GameObject and add `LightPuzzleLevelSpawner`.
2. Assign prefabs to `Prefab Bindings` by ID: `PL_001`, `LIG_SRC`, `OBJ_MIR`, `OBJ_MER`, `OBJ_SPL`, `OBJ_CON`, `OBJ_SWI`, `OBJ_DOR`, `OBJ_WAL`, `OBJ_GOA`, `ENE_001`, `ENE_002`.
3. Create a sample JSON `TextAsset`:

```json
{
  "objects": [
    { "id": "LIG_SRC", "position": { "x": -4, "y": 0 }, "rotationZ": 0, "configureLightSource": true, "color": 1, "intensity": 10 },
    { "id": "OBJ_MIR", "position": { "x": 0, "y": 0 }, "rotationZ": 45 },
    { "id": "OBJ_GOA", "position": { "x": 4, "y": 0 }, "rotationZ": 0, "configureReceiver": true, "receiverRequiresColor": true, "receiverRequiredColor": 1, "receiverRequiredIntensity": 10 }
  ]
}
```

`color` and `receiverRequiredColor` use bitmasks: Red = 1, Green = 2, Blue = 4, Yellow = 3, Magenta = 5, Cyan = 6, White = 7.
