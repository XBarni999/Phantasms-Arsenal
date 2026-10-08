# I-TGT validation — 2026-10-08

## Live game checks

Game: Nuclear Option 0.34, installed at `F:\Games\Nuclear.Option.v0.34.1`, with the user's existing BepInEx/Blueprinter plugins enabled.

- Arsenal 1.1.0 initialized without an I-TGT startup/patch exception.
- Tutorial 3: the MFD opened, the native map gained its topographic background, and a map click created a mark with terrain elevation of 143 m.
- Depot Strike, SFB-81 Darkreach, GPO-500: the refined MFD opened with F6, displayed the terrain and aircraft marker, created T01, and bound GPO-500 to that mark. The screenshot in `docs/images/itgt.png` is from this mission.
- A normal left-button release with the MFD closed changed GPO-500 ammo from 12 to 11. The initialized seeker logged `I-TGT released GPO-500 at Global(-34715,6,0,0,-18045,1)` (comma-decimal locale): easting -34715.6 m, elevation 0 m, northing -18045.1 m. This verifies coordinate delivery through rail launch, spawning and seeker initialization.
- F8 collided with the installed autopilot panel. The default and installed I-TGT configuration were changed to F6.

The coordinate-release candidate DLL SHA-256 was `FE31731E3D7D268C8A6BD40C3049B4ABF1C78FFC04EA0F9B59651028831C3669`. Subsequent changes add optical distance/time refresh, restore the native half-second cruise waypoint cadence, and add per-pylon/per-store assignments with an unassigned-store guard and one-shot store assignment consumption. These changes require a further release test.

Current build: zero compiler errors/warnings. Build output, repository `dist` and installed DLL all have SHA-256 `AEE8F1EA952E029B0F27B1594FA4750A38C409F73A9F7EC0A04691D2006D8B48`. The game was closed before installing this build, then restarted. Depot Strike loaded and its cockpit map displayed the generated terrain layer. Interactive scope tests were stopped when manual input was detected in the game window; TYPE/PYLON/STORE controls and precedence are not yet verified in a mission. The embedded Blueprinter bundle and serialized weapon assets were not changed.

## Limits of this verification

The bomb test verifies the launch coordinate, not an accurate impact. It used an arbitrary sea coordinate and did not establish terminal target acquisition. Optical/cruise/ballistic/laser weapon families, Blackout GPS activation, repeated releases, native-target mode restoration, drag/resize, other terrain maps and multiplayer-host behavior still need dedicated mission coverage. Remote-client GPS release is deliberately unavailable in this version.

## Manual acceptance checks

1. Create several terrain marks. Bind a TYPE default, override one PYLON, then override one STORE on that pylon. Verify STORE > PYLON > TYPE precedence, native release order, and that an unassigned compatible next store is held while GPS is armed. Fire a weapon, then change the active mark; verify its in-flight aim point stays on the captured designation. Rearm and verify individual store assignments were consumed.
2. Drop guided bombs within their physical delivery envelope onto stationary coordinates; compare impact error with native delivery. Verify ordinary unguided stores cannot be bound.
3. Launch an optical missile toward a mark with an exposed enemy surface unit nearby. Verify terminal acquisition requires range, line of sight and field of view, with no friendly or aerial acquisition.
4. Repeat without any unit near the mark. Verify the coordinate remains the fallback and an INS/optical cruise missile survives its targetless cruise phase.
5. Test a laser-guided bomb with and without terminal illumination, Killjoy's coordinate loft, and Blackout's coordinate HPM activation separately.
6. Disarm GPS and verify native target selection and salvos. Toggle TOPO and verify native contacts/controls remain usable. Change aircraft and verify all assignments reset. Enter a mission on another terrain map and verify the relief is rebuilt and previous marks are cleared.
