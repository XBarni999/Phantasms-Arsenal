# Editing Phantasm's Arsenal

Open the Blueprinter project and navigate to:

`Assets/Blueprinter/Mods/PhantasmsArsenal/Packs/`

Each pack has its own folder: **Poseidon**, **Killjoy**, **Apex**, **CircuitBreaker**.

| Folder | What to edit |
| --- | --- |
| Prefabs | Missile/drone body, seeker, motor, mass, effects and deployable objects |
| Mounts | Weapon racks, pylons and cargo mounts |
| WeaponInfo | Display name, description, range, icon and weapon category |
| Definitions | Unit identity and detectability |
| MountInfo | Ammunition count and mounting data |
| Operations | Carrier compatibility and Blueprinter registration |
| Models | Imported meshes and their source materials |
| Materials / Textures / Icons | Appearance and inventory images |
| Animations | Wing deployment and other authored animations |

## Weapon map

| Weapon | Pack / prefab |
| --- | --- |
| Poseidon conventional | Poseidon/Prefabs/R_460_Poseidon_TEST.prefab |
| Poseidon nuclear | Poseidon/Prefabs/R_460_Poseidon_Nuclear.prefab |
| Poseidon TEL missile | Poseidon/Prefabs/R_460_Poseidon_TEL.prefab |
| Poseidon TEL vehicle | Poseidon/Prefabs/R460_Poseidon_TEL.prefab |
| Killjoy conventional | Killjoy/Prefabs/Kinzhal_HE.prefab |
| Killjoy nuclear | Killjoy/Prefabs/Kinzhal_Nuclear.prefab |
| Apex ground drone | Apex/Prefabs/Apex6_Ground.prefab |
| Apex airborne drone | Apex/Prefabs/Apex8_Air.prefab |
| Apex pallets | Apex/Prefabs/Apex6_Pallet8.prefab and Apex8_Pallet4.prefab |
| Blackout | CircuitBreaker/Prefabs/Blackout.prefab |
| Locust dispenser | CircuitBreaker/Prefabs/Locust.prefab |
| Lawn Chair dispenser | CircuitBreaker/Prefabs/LawnChair.prefab |
| Locust contact mine | CircuitBreaker/Prefabs/LocustMine.prefab |
| Zhdan smart mine | CircuitBreaker/Prefabs/ZhdanMine.prefab |

Edit the combined pack directly. Original separate mod folders are retained as reference; their Unity GUIDs differ from the combined copy. Keep internal weapon keys unchanged unless you also update runtime matching logic. Existing custom names and icons were copied without redesign.

Runtime C# belongs under the repository's `Runtime` folders. Gameplay scripts are compiled into the DLL, not imported as duplicate game scripts in Unity.

## Build

1. Save the edited assets.
2. Open Blueprinter's standard Mod Builder and select `PhantasmsArsenal`.
3. Use display name `Phantasm's Arsenal` and version `1.0.0`, then copy the output into the repository's `Bundles` folder.
4. Run `dotnet build Runtime/Arsenal.csproj -c Release`, setting game and Blueprinter reference paths for your installation.
5. Install `Runtime/bin/Release/net472/Phantasms-Arsenal.dll`, restart the game, and test the changed weapon in a mission.

When updating the repository after editing in Unity, copy the mod folder together with its `.meta` files into `UnityAssets/PhantasmsArsenal`.
