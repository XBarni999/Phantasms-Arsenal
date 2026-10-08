# Phantasm's Arsenal 1.1.5 — I-TGT GPS

I-TGT adds GPS planning and coordinate releases to the Arsenal's existing weapon packs.

- Create up to 16 marks on a 2048×2048 topographic map, or import a selected Data Link target's known position with **DL→GPS**.
- **CYCLE** sends successive weapons through the marks in order and wraps automatically. Fixed **TYPE**, **PYLON** and individual **STORE** assignments take priority. Queue advancement happens only after a successful spawned release.
- Cockpit-only GPS markers show the point, range and bearing, with approximate bomb release cues. Symbology yields to maps and menus.
- Rebind the opening key in Settings. Zoom, pan, move and resize I-TGT without changing the flight camera while the pointer is over it.
- Guided cluster deployment uses an actual visible enemy surface target near the GPS coordinate and the native dispenser mechanism.

## Guides

[Quick start](https://github.com/XBarni999/Phantasms-Arsenal/blob/v1.1.5/docs/ITGT-QUICKSTART.md) · [Detailed planning guide](https://github.com/XBarni999/Phantasms-Arsenal/blob/v1.1.5/docs/ITGT-GUIDE.md) · [Bomb/missile compatibility](https://github.com/XBarni999/Phantasms-Arsenal/blob/v1.1.5/docs/GPS-WEAPONS.md)

![I-TGT GPS screen](https://raw.githubusercontent.com/XBarni999/Phantasms-Arsenal/v1.1.5/docs/images/itgt.png)

## Installation

Requires BepInEx and Blueprinter 2.0.1 or newer. Close the game and replace your existing Arsenal DLL with `Phantasms-Arsenal.dll`. Keep only one copy in `BepInEx/plugins` or your existing plugin subfolder. Separate Poseidon, Killjoy, Apex and Circuit Breaker copies must remain outside the plugins folder; the Arsenal contains those packs.

The ZIP includes the same DLL under `BepInEx/plugins`, both English guides, the compatibility table and validation notes. Existing configuration is retained. Default opening key: **F6**.

## Validation and operating limits

Release compilation and queue checks passed. The ten-weapon/six-mark helper test produces 1–2–3–4–5–6–1–2–3–4. Earlier mission checks and user testing covered GPS release, guided cluster target acquisition/impact and datalink import. Full live coverage of the latest camera transitions and ten-release integration is still pending.

GPS release requires single player or the multiplayer host. Delivery cues are estimates, without wind or terrain-path prediction. Guided clusters require a real exposed enemy near the mark, and laser terminal guidance requires illumination. Locust/Lawn Chair remain unguided CCIP dispensers. Imported coordinates are snapshots; marks reset on aircraft or mission changes. Compatibility-table eligibility is not an impact test for every weapon.

DLL SHA-256: `ECFCE75C40B8A156B2B4DFDBD273EDCF610F9744F65824BC3ECBACAC62281232`.
