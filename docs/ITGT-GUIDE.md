# I-TGT GPS — controls and mission planning

Start with the [one-minute quick guide](ITGT-QUICKSTART.md). This guide covers the complete workflow and multiple assignments.

![I-TGT GPS targeting screen](images/itgt.png)

## Opening and settings

Press **F6** by default. To change it, open the game's **Settings**, find **I-TGT GPS SETTINGS**, click the binding and press a new key. **Esc** cancels. The binding saves automatically. The same panel can disable the native map's topographic background.

There is no permanent launcher when I-TGT is closed. Pressing the binding again closes it without disarming GPS.

## Creating marks

- Left-click the map to create T01. Keep up to **16 marks**.
- Select a Data Link target and press **DL→GPS** to copy the first selected target's known HQ position. A message explains when no position is available.
- **PREV / NEXT** select marks; **DEL** deletes the selected mark and its assignments. Deleting all marks resets numbering to T01.

Coordinates are mission east/north positions in kilometres and elevation in metres, not real-world latitude/longitude. Marks are coordinate snapshots: moving targets can leave them. Import does not create continuous tracking. Repeating DL→GPS creates another mark rather than updating the previous one.

## Assigning weapons

1. Select a weapon using normal controls or **WPN**.
2. Choose a mark with **PREV / NEXT**.
3. Use **SCOPE**: **CYCLE** (default) cycles through all marks, **TYPE** fixes the selected weapon type to one mark, **PYLON** fixes one pylon, and **STORE** fixes one mounted bomb/missile.
4. For PYLON or STORE, use **SLOT** to choose the mounted store. The header shows its pylon and store number.
5. Press **BIND**. Repeat for other marks or stores.
6. Press **ARM / GPS**. In TYPE/PYLON/STORE this also binds the active mark, so check the selection first. In CYCLE it enables automatic sequencing and removes a fixed TYPE assignment without resetting the current queue. BIND in CYCLE explicitly resets to the first mark.

STORE overrides PYLON; PYLON overrides fixed TYPE; otherwise CYCLE supplies the next mark. **NEXT GPS Txx** shows the next store's assignment in native release order. SLOT selects a store for programming; it does not change release order.

CYCLE follows mark creation order and wraps after the final mark. Ten weapons and six marks produce T01, T02, T03, T04, T05, T06, T01, T02, T03, T04. Each weapon type has its own queue. Reading a preview or attempting a blocked/failed release does not consume a mark: advancement happens only when the missile/bomb is actually spawned. Explicit STORE/PYLON/TYPE releases do not consume the automatic queue. Disarming and rearming CYCLE preserves its position; BIND resets it. Deleted marks are skipped, and clearing the plan resets the queues.

## Approaching and releasing

Close I-TGT and check native weapon safeties. The HUD diamond shows the point, distance and bearing; an edge marker indicates an off-screen point. The white tip of the aircraft symbol on the GPS map marks its nose.

| Cue | Meaning |
| --- | --- |
| TURN LEFT / RIGHT | Turn toward the mark |
| CLIMB / TOO LOW | Gain altitude |
| TOO FAR / HOLD | Continue approaching |
| RELEASE WINDOW ~ | Approximate delivery window |

The estimate uses current altitude, velocity and weapon parameters. It does not predict wind or terrain obstacles and does not guarantee impact. There is no arbitrary minimum-distance rejection. Use the normal weapon trigger. Released weapons retain their captured coordinates when you select another mark.

Guided cluster deployment requires a **real exposed enemy ground/sea target near the mark**. An empty coordinate is not a target object. Locust and Lawn Chair are unguided CCIP mine dispensers, without GPS support. Laser-guided bombs need illumination for laser terminal acquisition.

GPS release is available in single player or to the multiplayer host. Remote clients retain native targeting. The [compatibility table](GPS-WEAPONS.md) reports adapter eligibility, not completed flight tests for every row.

## Map controls

Wheel or **+ / −** zoom; right-drag pans. **OWN** centres on the aircraft; **TGT** on the selected mark. Drag the header to move the MFD or its lower-right corner to resize it. Camera pan/tilt/zoom commands are suppressed over the MFD, including resize drags. Leaving it restores them; flight controls remain active.

**TOPO** toggles the native map background. I-TGT retains its own relief. New missions regenerate the layer; changing aircraft or missions clears marks and assignments. Terrain changes during the same mission are not tracked continuously.

## Planning multiple assignments

Suppose three stationary targets require different stores of the same guided weapon type.

1. Create **T01, T02 and T03**, manually or from Data Link. Check positions and elevations. Imported HQ information can be stale.
2. Choose TYPE, select T01 and BIND as the default.
3. Choose PYLON, find the desired pylon/store with SLOT, select T02 and BIND. That pylon uses T02 instead of T01.
4. Choose STORE, select one store on that pylon, select T03 and BIND. Only that store uses T03; the others still use T02.
5. Before ARM, check the mark, SCOPE and SLOT. ARM also performs BIND: leave them on the assignment you just saved to avoid overwriting another one.
6. Arm GPS and check NEXT GPS. The game determines the next store; SLOT only chooses which one to program.
7. Check the next assignment after each release. The editor's selected mark is not necessarily the next weapon's destination; NEXT GPS is the relevant readout.

| Available assignments | Used designation |
| --- | --- |
| STORE, PYLON and TYPE | STORE |
| PYLON and TYPE | PYLON |
| TYPE only | TYPE |
| No explicit assignment, marks exist | Next CYCLE mark |
| No marks | No GPS designation |

STORE assignments are consumed at release and do not transfer automatically to replacement stores after rearming. TYPE/PYLON persist within the aircraft/mission. Deleting a mark removes its assignments; changing aircraft or missions clears the plan. Saving plans to files is not implemented.

## Button reference

| Control | Action |
| --- | --- |
| WPN | Next native weapon station/type |
| + / − | GPS map zoom |
| OWN / TGT | Centre on aircraft / selected mark |
| TOPO | Native topographic background ON/OFF |
| SCOPE | CYCLE → TYPE → PYLON → STORE |
| SLOT | Next available store for programming |
| DL→GPS | New mark from the first selected Data Link target |
| PREV / NEXT | Previous / next GPS mark |
| DEL | Delete selected mark and its assignments |
| BIND | Save assignment at the selected scope |
| GPS or ARM / DISARM | Enable GPS (fixed scopes also bind) / disable GPS |
| X or opening key | Close MFD without disarming GPS |

## HUD layers

The target marker and delivery text are **cockpit-only** and are hidden in external, chase and other camera views. The bright outlined diamond and centre point mark the coordinate in the world. Off-screen points appear at the edge with a direction indication. The label shows mark number and distance. Amber is used for preview/advisory cues; green indicates the approximate delivery window.

With GPS off, the HUD shows the selected mark as PREVIEW. With GPS armed, it shows the next store's assignment. If missing, it does not substitute the selected preview: check NEXT GPS and BIND.

Flight symbology hides while I-TGT, the map, chat or another native cursor-driven menu is open. Close these panels to restore it. Thin dark outlines improve contrast without a background panel.

## Troubleshooting

| Problem | Check |
| --- | --- |
| DL→GPS creates no mark | Selected target, known HQ position and the 16-mark limit |
| Unsupported weapon | Seeker/conditions in the table; a guided name alone does not imply adapter support |
| GPS holds release | NEXT GPS, assignments, ammunition, native safeties and host status |
| Cluster arrives but does not dispense | A real eligible enemy near the point and direct visibility from the carrier |
| Moving target left the mark | Import a new position and reassign unreleased stores |
| Aircraft or mission changed | Create marks and assignments again |
| Native targeting/salvos needed | Press DISARM |

The test release verifies compilation and documented tested paths. Modded prefabs differ in range, physics, terminal guidance and deployment; broad mission testing remains necessary.
