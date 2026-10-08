# Changelog

## 1.0.3 — 2026-10-08

- Corrected Eclipse dispenser collision isolation to include registered aircraft parts reparented outside the aircraft transform hierarchy. Version 1.0.2 missed these articulated bodies and did not resolve the reported wing reversal.
- Reapply isolation after part initialization and rearming; detached aircraft parts and separately spawned missiles retain normal collision behavior.
- Log registered-part and collision-pair counts for verification. In-mission confirmation remains pending.

## 1.0.2 — 2026-10-08

- Isolated Locust and Lawn Chair mounted-dispenser colliders from their own FS-41 Eclipse carrier to prevent contact forces against articulated wing bodies.
- Kept carrier wing control, mount mass and drag, external collisions and separately spawned dispenser/mine collisions unchanged.
- Added an Eclipse dispenser collision-isolation diagnostic to the BepInEx log. The reported reversed-wing behavior still needs an in-mission retest with this build.
- Reused the verified 1.0.1 weapon bundle; this update changes runtime collision handling only.

## 1.0.1 — 2026-10-08

- Reduced Blackout's prefab scale by 5%, including its collider, for additional ground clearance. In-mission clearance verification remains pending.
- Rebuilt the combined Blueprinter bundle and DLL with the smaller Blackout.
- Expanded the README with each weapon's purpose, targeting requirements, variants and limitations, including Blackout's effects on friendly aircraft.
- Fixed Poseidon gallery face culling and orientation, improved preview materials and removed the duplicate aircraft-variant image.
- Removed repository helper tools while retaining runtime source, editable assets and documentation.

## 1.0.0 — 2026-10-08

- Combined Poseidon, HSM-290 Killjoy, Apex and Circuit Breaker assets in one Blueprinter bundle embedded in one DLL.
- Preserved existing weapon identities, custom names, icons and runtime configuration IDs.
- Added editable Unity packs organized by weapon family and asset function, with unique GUIDs and remapped internal references.
- Added Unity menu commands for bundle builds and studio gallery renders.
- Added 12 Unity renders, including Killjoy with its warhead fins folded and engine lights disabled for clean studio presentation.
- Verified the embedded bundle against its source SHA-256; the combined pack has not yet been tested in a live mission.
