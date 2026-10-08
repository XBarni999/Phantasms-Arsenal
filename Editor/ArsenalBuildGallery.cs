using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ArsenalBuildGallery
{
    const string Output = "F:/NCMod/Phantasm's Arsenal/";
    const string Mods = "Assets/Blueprinter/Mods/";
    static ArsenalBuildGallery() { EditorApplication.update += Poll; }
    static void Poll()
    {
        var request = Output + "verification/unity.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        string action = File.ReadAllText(request).Trim(); File.Delete(request);
        EditorApplication.delayCall += () => { try {
            if (action == "build") Build(); else Gallery();
            File.WriteAllText(Output + "verification/unity-" + action + ".result", "OK " + DateTime.UtcNow.ToString("O"));
        } catch (Exception e) { File.WriteAllText(Output + "verification/unity-" + action + ".result", e.ToString()); Debug.LogException(e); } };
    }
    [MenuItem("Blueprinter/Phantasm's Arsenal/Build weapon bundle")]
    public static void Build()
    {
        Bundle("PhantasmsArsenal", "Phantasm's Arsenal", "1.0.0");
    }
    static void Bundle(string folder, string title, string version)
    {
        string target = Output + "Bundles/" + title + "_" + version + ".nobp";
        if (File.Exists(target)) File.Delete(target);
        Blueprinter.ModBuilder.Build(folder, title, version, Output + "Bundles");
        if (!File.Exists(target)) throw new Exception("Fresh bundle missing: " + target);
    }
    [MenuItem("Blueprinter/Phantasm's Arsenal/Render weapon gallery")]
    public static void Gallery()
    {
        Shot("PhantasmsArsenal/Packs/Poseidon/Prefabs/R_460_Poseidon_TEST", "poseidon", true);
        Shot("PhantasmsArsenal/Packs/Poseidon/Prefabs/R_460_Poseidon_Nuclear", "poseidon-nuclear", true);
        Shot("PhantasmsArsenal/Packs/Poseidon/Prefabs/R_460_Poseidon_TEL", "poseidon-tel", true);
        Shot("PhantasmsArsenal/Packs/Killjoy/Prefabs/Kinzhal_HE", "killjoy-he", false);
        Shot("PhantasmsArsenal/Packs/Killjoy/Prefabs/Kinzhal_Nuclear", "killjoy-nuclear", false);
        Shot("PhantasmsArsenal/Packs/Apex/Prefabs/Apex6_Ground", "apex-6", true);
        Shot("PhantasmsArsenal/Packs/Apex/Prefabs/Apex8_Air", "apex-8", true);
        Shot("PhantasmsArsenal/Packs/CircuitBreaker/Prefabs/Blackout", "blackout", true);
        Shot("PhantasmsArsenal/Packs/CircuitBreaker/Prefabs/Locust", "locust", false);
        Shot("PhantasmsArsenal/Packs/CircuitBreaker/Prefabs/LawnChair", "lawn-chair", false);
        Shot("PhantasmsArsenal/Packs/CircuitBreaker/Prefabs/LocustMine", "locust-mine", true);
        Shot("PhantasmsArsenal/Packs/CircuitBreaker/Prefabs/ZhdanMine", "zhdan-mine", true);
    }
    static void Shot(string prefab, string name, bool deployed)
    {
        var preview = new PreviewRenderUtility();
        try {
            preview.BeginStaticPreview(new Rect(0, 0, 2400, 1350));
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Mods + prefab + ".prefab");
            if (!asset) throw new Exception("Missing prefab " + prefab);
            var model = UnityEngine.Object.Instantiate(asset); preview.AddSingleGO(model);
            // Poseidon's authored mesh rolls the lettering onto its lower side.
            // Roll the preview instance only to present the lettering upright.
            foreach (var camera in model.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
            foreach (var light in model.GetComponentsInChildren<Light>(true)) light.enabled = false;
            foreach (var particles in model.GetComponentsInChildren<ParticleSystem>(true)) particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            foreach (var animation in model.GetComponentsInChildren<Animation>(true)) if (animation.clip) animation.clip.SampleAnimation(animation.gameObject, deployed ? animation.clip.length : 0);
            if (name.StartsWith("killjoy")) {
                var original = asset.GetComponentsInChildren<Transform>(true);
                var copied = model.GetComponentsInChildren<Transform>(true);
                if (original.Length != copied.Length) throw new Exception("Preview hierarchy changed");
                for (int i = 1; i < original.Length; i++) {
                    copied[i].localPosition = original[i].localPosition;
                    copied[i].localRotation = original[i].localRotation;
                    copied[i].localScale = original[i].localScale;
                }
            }
            if (name.StartsWith("poseidon")) model.transform.Rotate(Vector3.forward, 180, Space.World);
            var renderers = model.GetComponentsInChildren<Renderer>().Where(r => r.enabled && !(r is ParticleSystemRenderer) && !(r is TrailRenderer) && !(r is LineRenderer)).ToArray();
            if (renderers.Length == 0) throw new Exception("No visible renderers " + prefab);
            foreach (var renderer in renderers) {
                renderer.SetPropertyBlock(null);
                var materials = renderer.sharedMaterials.Select(original => {
                    if (!original) return original;
                    var copy = new Material(original);
                    copy.DisableKeyword("_EMISSION");
                    if (copy.HasProperty("_EmissionColor")) copy.SetColor("_EmissionColor", Color.black);
                    foreach (var property in new [] {"_Temperature", "_Heat", "_HeatIntensity"}) if (copy.HasProperty(property)) copy.SetFloat(property, 0);
                    return copy;
                }).ToArray();
                renderer.sharedMaterials = materials;
            }
            File.WriteAllLines(Output + "verification/materials-" + name + ".txt", renderers.SelectMany(r => r.sharedMaterials.Where(m => m).Select(m => r.name + " | " + m.name + " | " + m.shader.name + " | " + string.Join(", ", Enumerable.Range(0, ShaderUtil.GetPropertyCount(m.shader)).Select(i => ShaderUtil.GetPropertyName(m.shader, i))))));
            var bounds = renderers[0].bounds; foreach (var r in renderers.Skip(1)) bounds.Encapsulate(r.bounds);
            var cam = preview.camera; cam.orthographic = true; cam.nearClipPlane = .01f; cam.farClipPlane = 1000;
            cam.transform.position = bounds.center + new Vector3(1.9f, .85f, 1).normalized * (bounds.size.magnitude * 3 + 5); cam.transform.LookAt(bounds.center);
            float width = 0, height = 0;
            for (int i = 0; i < 8; i++) {
                var local = cam.transform.InverseTransformPoint(bounds.center + Vector3.Scale(bounds.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1)));
                width = Mathf.Max(width, Mathf.Abs(local.x)); height = Mathf.Max(height, Mathf.Abs(local.y));
            }
            cam.orthographicSize = Mathf.Max(height, width / (2400f / 1350f)) * 1.15f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.035f, .045f, .065f);
            preview.lights[0].intensity = name.StartsWith("killjoy") ? 1.1f : 2.4f; preview.lights[0].color = new Color(1, .91f, .79f); preview.lights[0].transform.rotation = Quaternion.Euler(35, -45, 0);
            preview.lights[1].intensity = name.StartsWith("killjoy") ? .8f : 1.8f; preview.lights[1].color = new Color(.64f, .8f, 1); preview.lights[1].transform.rotation = Quaternion.Euler(145, 135, 0);
            preview.ambientColor = new Color(.36f, .39f, .44f);
            preview.Render(true); var image = preview.EndStaticPreview();
            File.WriteAllBytes(Output + "docs/images/" + name + ".png", image.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(image);
        } finally { preview.Cleanup(); }
    }
}
