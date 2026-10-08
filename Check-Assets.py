from pathlib import Path
import json, re
root = Path(r"F:\NCMod\Blueprinter-Editor\Assets\Blueprinter\Mods\PhantasmsArsenal")
mapping = json.loads((Path(__file__).parent / 'verification/guid-map.json').read_text())
guids = {}
errors = []
for meta in root.rglob('*.meta'):
    match = re.search(r'^guid: ([0-9a-f]{32})$', meta.read_text(encoding='utf-8-sig'), re.M)
    if not match: continue
    guid = match.group(1)
    if guid in guids: errors.append('Duplicate GUID: '+guid)
    guids[guid] = str(meta)
for file in root.rglob('*'):
    if file.suffix not in {'.asset','.prefab','.mat','.anim','.controller','.meta'}: continue
    for guid in re.findall(r'guid: ([0-9a-f]{32})',file.read_text(encoding='utf-8-sig')):
        if guid in mapping: errors.append('Reference to original pack: '+str(file)+' '+guid)
        if guid in mapping.values() and guid not in guids: errors.append('Missing copied asset: '+str(file)+' '+guid)
report = {'unique_guids':len(guids),'remapped_asset_guids':len(mapping),'errors':errors,'passed':not errors}
(Path(__file__).parent/'verification/asset-references.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
raise SystemExit(1 if errors else 0)
