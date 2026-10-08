using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PhantasmsArsenal.ITGT
{
    internal static class WeaponInventory
    {
        static float next;
        static string signature;
        static string Quote(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        internal static void Update()
        {
            if (Time.realtimeSinceStartup < next) return;
            next = Time.realtimeSinceStartup + 10;
            var weapons = Resources.FindObjectsOfTypeAll<WeaponInfo>().Where(w => w)
                .OrderBy(w => w.weaponName).ThenBy(w => w.name).ToArray();
            if (weapons.Length < 10) return;
            var rows = weapons.Select(w =>
            {
                var seeker = w.weaponPrefab ? w.weaponPrefab.GetComponent<MissileSeeker>() : null;
                bool supported = Guidance.Supports(w);
                string reason = supported ? "Coordinate guidance" : seeker is OpticalSeekerHighDrag ? "High-drag submunition; not a selectable guided carrier" :
                    w.gun ? "Gun" : !seeker || seeker.GetType() == typeof(MissileSeeker) ? "No supported steering seeker" :
                    seeker is LaserSeeker ? "Laser-only missile requires illumination" : "Seeker requires native targeting; GPS adapter unavailable";
                string cluster = w.weaponPrefab && w.weaponPrefab.GetComponent<SubmunitionDispenser>() ? "Native dispenser: real exposed enemy near GPS mark required" :
                    w.name == "WI_Locust" || w.name == "WI_LawnChair" ? "Unguided mine dispenser; native CCIP/altitude deployment" : "";
                string components = w.weaponPrefab ? string.Join(";", w.weaponPrefab.GetComponents<MonoBehaviour>().Where(c => c).Select(c => c.GetType().Name)) : "No projectile prefab";
                return string.Join(",", new[] { w.weaponName, w.name, w.bomb ? "Bomb" : w.glideBomb ? "Glide bomb" : w.missile ? "Missile" : w.gun ? "Gun" : "Other",
                    seeker ? seeker.GetType().Name : "None", supported ? "Yes" : "No", reason, cluster, components }.Select(Quote));
            }).Distinct().ToArray();
            string content = "Name,Asset,Kind,Seeker,GPS,Reason,Cluster,Components\n" + string.Join("\n", rows) + "\n";
            if (content == signature) return;
            try
            {
                string path = Path.Combine(BepInEx.Paths.ConfigPath, "Phantasms-Arsenal-GPS-weapons.csv");
                File.WriteAllText(path, content, new System.Text.UTF8Encoding(true));
                signature = content;
                Display.Trace("I-TGT loaded weapon inventory: " + rows.Length + " rows, " + path);
            }
            catch (Exception error) { Display.Trace("I-TGT inventory export: " + error.Message); }
        }
    }
}
