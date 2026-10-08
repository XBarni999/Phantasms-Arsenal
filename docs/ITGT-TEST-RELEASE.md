# I-TGT GPS — test build 1.1.4

This prerelease adds an in-flight GPS planning screen to Phantasm's Arsenal: create several coordinate marks, assign them to weapon types, pylons or individual stores, then release compatible weapons using normal flight controls.

## What is included

- Up to 16 GPS marks on a terrain-derived 2048×2048 topographic map with relief, contours, zoom and pan. Each new mission builds its own terrain layer.
- **DL→GPS** copies the selected datalink target's known position into a mark. The copied point is a coordinate snapshot, not a moving target track.
- **TYPE / PYLON / STORE** assignments with individual-store priority, a next-release designation readout and the game's existing release order.
- A world-space GPS diamond, distance, bearing and approximate bomb delivery cues. Brighter outlined symbology has no background panel and yields to maps and menus.
- Key rebinding in the game's Settings screen, an aircraft silhouette with a clear nose, and camera pan/tilt/zoom suppression while the pointer is over I-TGT. No permanent launcher button remains on the flight screen.
- A GPS-specific approach path for guided cluster dispensers, using a real exposed enemy near the mark and the native deployment mechanism.
- The existing Poseidon, Killjoy, Apex and Circuit Breaker weapon packs in the same DLL; their embedded Blueprinter assets were reused.

![I-TGT GPS screen](https://raw.githubusercontent.com/XBarni999/Phantasms-Arsenal/main/docs/images/itgt.png)

## Start here

[One-minute quick start](https://github.com/XBarni999/Phantasms-Arsenal/blob/main/docs/ITGT-QUICKSTART.md) · [Detailed functions and multi-target planning guide](https://github.com/XBarni999/Phantasms-Arsenal/blob/main/docs/ITGT-GUIDE.md) · [Bomb/missile compatibility table](https://github.com/XBarni999/Phantasms-Arsenal/blob/main/docs/GPS-WEAPONS.md)

For a first release: select compatible ordnance, press **F6**, create or import T01, leave **SCOPE = TYPE**, press **BIND**, then **ARM**. Check **NEXT GPS T01**, close the screen and use the normal weapon trigger. Change the opening key in **Settings → I-TGT GPS SETTINGS**. DISARM restores native targeting.

## Install

Requires BepInEx and Blueprinter 2.0.1 or newer. Close the game and replace the existing Arsenal DLL with the attached `Phantasms-Arsenal.dll` in `BepInEx/plugins`. Keep only one Arsenal DLL. Separate Poseidon/Killjoy/Apex/Circuit Breaker copies must remain outside the plugins folder, as the Arsenal already contains them.

The ZIP contains that same DLL under `BepInEx/plugins` plus the guides, compatibility table, changelog and validation notes. Existing BepInEx configuration is retained. Do not create a second copy of the DLL if your existing installation uses a plugins subfolder.

## Test scope

Release compilation passed with zero errors/warnings. Earlier builds were opened in missions and delivered GPO-500 GPS coordinates through a normal release. User testing reported successful vanilla cluster target acquisition/impact and datalink import. The newest HUD contrast, UI suppression and marker refinements still need live visual confirmation.

Delivery cues are estimates: wind and intervening terrain are not predicted. A green window does not guarantee impact. Guided cluster deployment needs an actual visible enemy surface unit near the coordinate; an empty mark is not a target object. Locust/Lawn Chair are unguided CCIP dispensers. Laser terminal guidance needs illumination. GPS release is available in single player or to the multiplayer host; remote clients retain native targeting.

The table lists adapter compatibility for 145 loaded bomb/missile definitions (78 eligible, 67 ineligible), not 145 completed flight tests. Marks and assignments reset when changing aircraft or missions. Test settings persistence, camera isolation, multi-store release order, cluster deployment and each weapon's physical delivery envelope before relying on this build for a full mission.

DLL SHA-256: `438EDAC4D968AEF75BAC9FC2D7F15DE714563B0A4B3341D672C261C1D54E12E3`.
