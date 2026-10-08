# Phantasm's Arsenal

One DLL containing four editable Blueprinter weapon packs for **Nuclear Option**.

![Blackout missile rendered in Unity](docs/images/blackout.png)

[Download the DLL](https://github.com/XBarni999/Phantasms-Arsenal/raw/refs/heads/main/dist/Phantasms-Arsenal.dll) · [Weapon gallery](docs/GALLERY.md) · [Editing guide](docs/EDITING.md)

| Pack | Weapons |
| --- | --- |
| Poseidon | R-460 aircraft, nuclear and TEL anti-ship variants |
| Killjoy | HSM-290 conventional and nuclear ballistic missiles |
| Apex | Apex drones, airborne variants and cargo pallets |
| Circuit Breaker | Blackout HPM missile, Locust mine dispensers, Lawn Chair and Zhdan smart mines |

## Installation

Requires BepInEx and Blueprinter **2.0.1 or newer**. Install `dist/Phantasms-Arsenal.dll` in `BepInEx/plugins/PhantasmsArsenal/`.

Before installing, move the separate Poseidon, Killjoy, Apex and Circuit Breaker DLLs and `.nobp` files outside `BepInEx/plugins`. The Arsenal replaces them and uses their existing weapon identities and configuration IDs. Loading both copies can duplicate weapons or patches. Third-party aircraft packs remain separate dependencies for their optional carrier integrations.

## Gallery

All images are rendered from the editable weapon prefabs in Unity. See [the gallery](docs/GALLERY.md).

## Editing and building

The editable pack lives at `Assets/Blueprinter/Mods/PhantasmsArsenal` in the Blueprinter project. See [the editing guide](docs/EDITING.md) for folders and weapon locations. The repository's `UnityAssets/PhantasmsArsenal` contains the same editable assets, with unique Unity GUIDs.

Use **Blueprinter → Phantasm's Arsenal → Build weapon bundle**, then run `./Build.ps1`. The finished bundle is embedded in the DLL and verified against its source SHA-256.

The DLL preserves each pack's runtime plugin and configuration identity. A single embedded Blueprinter bundle contains the combined assets and operations.

## Validation

Build, asset references and embedded bundle verification are recorded under `verification`. Package verification does not establish live mission behavior. A combined loadout/launch test is still required.
