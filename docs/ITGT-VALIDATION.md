# I-TGT validation — 2026-10-08

The current `docs/images/itgt.png` is a user-supplied capture of the topographic GPS screen. It replaces the earlier automated Depot Strike screenshot; earlier checks below remain historical evidence, not a claim about the replacement capture's build or mission.

## Live game checks

Game: Nuclear Option 0.34, installed at `F:\Games\Nuclear.Option.v0.34.1`, with the user's existing BepInEx/Blueprinter plugins enabled.

- Arsenal 1.1.0 initialized without an I-TGT startup/patch exception.
- Tutorial 3: the MFD opened, the native map gained its topographic background, and a map click created a mark with terrain elevation of 143 m.
- Depot Strike, SFB-81 Darkreach, GPO-500: the refined MFD opened with F6, displayed the terrain and aircraft marker, created T01, and bound GPO-500 to that mark. The screenshot in `docs/images/itgt.png` is from this mission.
- A normal left-button release with the MFD closed changed GPO-500 ammo from 12 to 11. The initialized seeker logged `I-TGT released GPO-500 at Global(-34715,6,0,0,-18045,1)` (comma-decimal locale): easting -34715.6 m, elevation 0 m, northing -18045.1 m. This verifies coordinate delivery through rail launch, spawning and seeker initialization.
- F8 collided with the installed autopilot panel. The default and installed I-TGT configuration were changed to F6.

The coordinate-release candidate DLL SHA-256 was `FE31731E3D7D268C8A6BD40C3049B4ABF1C78FFC04EA0F9B59651028831C3669`. Subsequent changes add optical distance/time refresh, restore the native half-second cruise waypoint cadence, and add per-pylon/per-store assignments with an unassigned-store guard and one-shot store assignment consumption. These changes require a further release test.

The scoped-assignment build had zero compiler errors/warnings. Its build output, repository `dist` and installed DLL matched SHA-256 `AEE8F1EA952E029B0F27B1594FA4750A38C409F73A9F7EC0A04691D2006D8B48`. The game was closed before installing this build, then restarted. Depot Strike loaded and its cockpit map displayed the generated terrain layer. Interactive scope tests were stopped when manual input was detected in the game window; TYPE/PYLON/STORE controls and precedence are not yet verified in a mission.

After the user's pixelation feedback, terrain resolution was increased from 512×512 to 2048×2048, with trilinear filtering, mipmaps, and incremental terrain sampling/shading. This current build compiled with zero errors/warnings; build output and repository `dist` match SHA-256 `7EB1FF6306E7133FF32948154BF5671FEC492AEBECA17F88D1005E06C49057AF`. Visual quality, generation duration and frame pacing need a restarted-game check. The installed running game still uses the preceding scoped-assignment build. The embedded Blueprinter bundle and serialized weapon assets were not changed.

## Limits of this verification

### 1.1.1 follow-up

Button restyling, empty-list numbering reset and GPS HUD/release cues compiled with zero errors/warnings. Build, `dist` and installed DLL match SHA-256 `099DBF7EE55F4AA784BC1BE347539AFB6A53B983A735E535609827D4B38E9339`. Installation happened while the game was closed. A restarted game logged Arsenal 1.1.1 initialization without an I-TGT startup error. Interactive validation was left pending while the user was operating other foreground applications; these changes have not yet been checked in a live mission.

HUD acceptance: delete T01 and T02, then create a new mark and verify T01; check the diamond from cockpit/external views and with the point behind the aircraft; verify armed HUD follows the next store's assignment rather than the selected preview. Compare bomb cues at different heights/speeds, with a target ahead, behind and laterally displaced. Verify too-far and green-window releases against actual impacts, separately for ordinary guided bombs and glide bombs. The estimate does not predict wind or intervening terrain, and must remain labelled approximate.

The bomb test verifies the launch coordinate, not an accurate impact. It used an arbitrary sea coordinate and did not establish terminal target acquisition. Optical/cruise/ballistic/laser weapon families, Blackout GPS activation, repeated releases, native-target mode restoration, drag/resize, other terrain maps and multiplayer-host behavior still need dedicated mission coverage. Remote-client GPS release is deliberately unavailable in this version.

## Manual acceptance checks

### 1.1.5 follow-up

Cockpit symbology now requires both the native cockpit camera mode and active cockpit state. CYCLE is the default assignment scope; explicit STORE/PYLON/TYPE assignments retain priority. The cursor advances after a matched non-null spawned weapon, not during selection or mounted-release capture. Pending captures for the same rail are replaced to prevent stale failed attempts from matching a subsequent release.

`verification/Verify-ITGTCycle.ps1` passed against the production queue helper: ten commits with six marks produce 1–2–3–4–5–6–1–2–3–4. It also checks type isolation, repeated previews, stale commits, deleted next marks, wrapping, reset and empty/new plans. Compilation passed with zero errors/warnings; build/dist/installed SHA-256 is `ECFCE75C40B8A156B2B4DFDBD273EDCF610F9744F65824BC3ECBACAC62281232`. Installation occurred while the game was closed. Camera transitions and actual ten-weapon mission sequencing still need live confirmation; helper tests do not establish the entire release integration.

### 1.1.4 test build

Dedicated white-base HUD typography fixes the prior multiplication of cue colours by the MFD label colour. Thin text/symbol outlines improve contrast without a background. Flight symbology now yields when the MFD, maximized map or any native cursor/UI flag is active. Target markers gained centre and direction details.

Build/dist/installed SHA-256: `438EDAC4D968AEF75BAC9FC2D7F15DE714563B0A4B3341D672C261C1D54E12E3`; compilation passed with zero errors/warnings, and installation occurred while the game was closed. The new contrast, UI suppression and marker presentation require live visual confirmation. The user reported satisfactory 1.1.3 operation and datalink import; that feedback does not establish the new visual changes. This build is intended as a GitHub prerelease.

### 1.1.3 follow-up

The user reported a vanilla guided cluster bomb acquiring and hitting a real target during their test. This is user-observed flight feedback; no separate automatic casing/submunition deployment trace was captured.

1.1.3 adds datalink-coordinate import, Settings-screen key rebinding, removal of the launcher, camera-axis interception and an aircraft silhouette. The native camera code was inspected: it reads `Rewired.Player.GetAxis("Zoom View"/"Pan View"/"Tilt View")`; only these actions are suppressed over I-TGT, without skipping camera follow/physics or flight input. Generated native-map tint is reapplied as opaque white to counter native mode/theme recolouring.

The Release build passed with zero errors/warnings. Build, `dist` and installed DLL match SHA-256 `57C6696288CB4D249BDFF83FAC3C27B228949765D4383EA4EC062CE95306FDD1`. A restarted game logged Arsenal 1.1.3 and completed its weapon snapshot without an I-TGT startup exception. After plugin loading the raw snapshot contained 281 definitions; the published bomb/missile table has 145 rows (78 adapter-compatible, 67 incompatible). UFO definitions, external fuel tanks, guns, laser installations and non-weapon equipment/cargo were filtered from the published table.

Settings layout/rebinding persistence, DL→GPS interaction, mouse pan/zoom isolation and the transparency correction still need interactive confirmation. A manual input notification interrupted the attempted Settings UI check; no visual pass is claimed for these changes.

### 1.1.2 follow-up

The HUD is now small unboxed text at 32% of screen height. The arbitrary minimum-range warning was removed. Source inspection of the installed game's `SubmunitionDispenser.TargetApproachCheck` established that its native deployment requires a real target ID, accurate HQ tracking, range and line of sight. GPS coordinates alone cannot meet that gate. The new GPS-specific check requires a real enemy surface unit near the captured coordinate, within the native deployment distance, with direct line of sight; it invokes the native `Missile.Damage` path so native casing/submunition behavior remains authoritative. No synthetic target or forced opening over empty coordinates is introduced.

The first 1.1.2 candidate compiled and initialized in the restarted game without an I-TGT error. Its runtime exporter captured 150 loaded weapon definitions with projectile prefabs after Blueprinter loading (79 eligible, 71 ineligible). An expanded exporter now includes definitions without prefabs, including guns. Current build/dist SHA-256 is `ED3288D4BAC7BD7A718EA17C9FC569E6B8E949C24EFC71764551B8426AEF75FF`, with zero compiler errors/warnings. Interactive cluster deployment and HUD placement remain unverified; the user's successful earlier guided-bomb impact is feedback about the preceding build, not verification of this dispenser fix.

1. Create several terrain marks. Bind a TYPE default, override one PYLON, then override one STORE on that pylon. Verify STORE > PYLON > TYPE precedence, native release order, and that an unassigned compatible next store is held while GPS is armed. Fire a weapon, then change the active mark; verify its in-flight aim point stays on the captured designation. Rearm and verify individual store assignments were consumed.
2. Drop guided bombs within their physical delivery envelope onto stationary coordinates; compare impact error with native delivery. Verify ordinary unguided stores cannot be bound.
3. Launch an optical missile toward a mark with an exposed enemy surface unit nearby. Verify terminal acquisition requires range, line of sight and field of view, with no friendly or aerial acquisition.
4. Repeat without any unit near the mark. Verify the coordinate remains the fallback and an INS/optical cruise missile survives its targetless cruise phase.
5. Test a laser-guided bomb with and without terminal illumination, Killjoy's coordinate loft, and Blackout's coordinate HPM activation separately.
6. Disarm GPS and verify native target selection and salvos. Toggle TOPO and verify native contacts/controls remain usable. Change aircraft and verify all assignments reset. Enter a mission on another terrain map and verify the relief is rebuilt and previous marks are cleared.
