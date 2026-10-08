"""Copy editable weapon packs, remap Unity GUIDs, and record provenance."""
from pathlib import Path
import hashlib, json, re, uuid

ROOT = Path(__file__).resolve().parent
PROJECT = Path(r"F:\NCMod\Blueprinter-Editor")
TARGET = PROJECT / "Assets/Blueprinter/Mods/PhantasmsArsenal"
PACKS = {"Poseidon": "R460Poseidon", "Killjoy": "Kh47M2", "Apex": "Apex-6", "CircuitBreaker": "Iron Gate"}
GUID = re.compile(r"(?<=guid: )[0-9a-f]{32}")
TEXT = {".meta", ".asset", ".prefab", ".mat", ".anim", ".controller", ".overridecontroller", ".shader", ".json", ".txt", ".md"}

def category(path):
    name = path.name
    if len(path.parts) > 1: return path
    if name.startswith("Op_"): return Path("Operations") / path
    if name.startswith("WI_"): return Path("WeaponInfo") / path
    if name.startswith("WM_"): return Path("MountInfo") / path
    if name.startswith("Def_"): return Path("Definitions") / path
    if path.suffix == ".prefab":
        folder = "Mounts" if name.startswith("CB_") or any(x in name for x in ("Mount", "Single", "Darkreach", "AirRail")) else "Prefabs"
        return Path(folder) / path
    if path.suffix == ".anim": return Path("Animations") / path
    if path.suffix.lower() in {".png", ".jpg", ".jpeg"}: return Path("Icons") / path
    return Path("Documentation") / path

def main():
    TARGET.mkdir(parents=True, exist_ok=True)
    mapping_path = ROOT / "verification/guid-map.json"
    mapping = json.loads(mapping_path.read_text()) if mapping_path.exists() else {}
    files = []
    for pack, original in PACKS.items():
        source = PROJECT / "Assets/Blueprinter/Mods" / original
        for file in sorted(source.rglob("*")):
            if not file.is_file(): continue
            relative = file.relative_to(source)
            if any(part.endswith("~") or part in {"Editor", "bin", "obj", ".git"} for part in relative.parts): continue
            if file.name in {"modinfo.json", "modinfo.json.meta"} or file.suffix in {".cs", ".csproj", ".dll", ".nobp"}: continue
            base = Path(str(relative)[:-5]) if relative.suffix == ".meta" else relative
            # Folder metadata is recreated by Unity at the new location.
            if relative.suffix == ".meta" and (source / base).is_dir(): continue
            destination = TARGET / "Packs" / pack / category(base)
            if relative.suffix == ".meta": destination = Path(str(destination) + ".meta")
            files.append((file, destination))
            if file.suffix == ".meta":
                match = GUID.search(file.read_text(encoding="utf-8-sig"))
                if match: mapping.setdefault(match.group(), uuid.uuid4().hex)
    mapping_path.write_text(json.dumps(mapping, indent=2))
    inventory = []
    for source, destination in files:
        data = source.read_bytes()
        destination.parent.mkdir(parents=True, exist_ok=True)
        if source.suffix in TEXT:
            text = data.decode("utf-8-sig")
            data = GUID.sub(lambda m: mapping.get(m.group(), m.group()), text).encode("utf-8")
        destination.write_bytes(data)
        inventory.append({"source": str(source), "asset": str(destination.relative_to(PROJECT)), "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(), "copied_sha256": hashlib.sha256(data).hexdigest()})
    (TARGET / "modinfo.json").write_text(json.dumps({"displayName": "Phantasm's Arsenal", "version": "1.0.0"}, indent=2))
    (ROOT / "verification/source-inventory.json").write_text(json.dumps(inventory, indent=2))
    print(f"Imported {len(files)} files, remapped {len(mapping)} GUIDs into {TARGET}")

if __name__ == "__main__": main()
