using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace PhantasmsArsenal.ITGT
{
    public sealed class Display : MonoBehaviour
    {
        internal sealed class Mark { public int id; public GlobalPosition point; }
        static Display instance;
        static readonly BepInEx.Logging.ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("I-TGT");
        const CursorFlags DisplayCursor = (CursorFlags)0x20000000;
        readonly List<Mark> marks = new List<Mark>();
        readonly Dictionary<string, Mark> assignments = new Dictionary<string, Mark>();
        readonly Dictionary<Hardpoint, Mark> pylonAssignments = new Dictionary<Hardpoint, Mark>();
        readonly Dictionary<MountedMissile, Mark> storeAssignments = new Dictionary<MountedMissile, Mark>();
        readonly DesignationCycle<Mark> cycle = new DesignationCycle<Mark>();
        enum BindingScope { Cycle, Type, Pylon, Store }
        BindingScope bindingScope;
        bool advancedBindings;
        int selectedStore;
        readonly TopographicMap topography = new TopographicMap();
        ConfigEntry<KeyCode> toggle;
        ConfigEntry<bool> replaceMap;
        ConfigEntry<float> windowWidth, windowHeight;
        Harmony harmony;
        Aircraft lastAircraft;
        Transform missionOrigin;
        Rect window, mapRect;
        bool visible, armed, resizing;
        bool positioned;
        bool capturingKey;
        bool captureArmKey;
        ConfigEntry<KeyCode> armKey;
        int selected, nextID = 1;
        Vector2 center;
        float span = 40000;
        GUIStyle label, small, button, hudText;
        string message = "LMB: mark   RMB: pan   Wheel: zoom";
        Mark Active => marks.Count > 0 ? marks[Mathf.Clamp(selected, 0, marks.Count - 1)] : null;
        public static Aircraft Aircraft => SceneSingleton<CombatHUD>.i ? SceneSingleton<CombatHUD>.i.aircraft : null;
        public static bool PointerOverDisplay
        {
            get
            {
                if (!instance || !Aircraft) return false;
                var mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                return instance.visible && (instance.window.Contains(mouse) || instance.resizing);
            }
        }
        internal static void Trace(string value) => Log.LogInfo(value);
        internal static bool GpsArmedFor(Unit owner, WeaponInfo info) => instance && instance.armed && owner == Aircraft && owner && owner.IsServer && Guidance.Supports(info);
        internal static void UnassignedNotice() { if (instance) instance.message = "Next store has no mark: BIND it or disarm GPS"; }
        internal static Mark CaptureMark(MountedMissile mount, out bool automatic)
        {
            automatic = instance && mount && !instance.storeAssignments.ContainsKey(mount) &&
                (Guidance.Pylon(mount) == null || !instance.pylonAssignments.ContainsKey(Guidance.Pylon(mount))) &&
                !instance.assignments.ContainsKey(mount.info.name);
            return instance ? instance.Assignment(mount) : null;
        }
        internal static void ReleaseCommitted(MountedMissile mount, Mark captured, bool automatic)
        {
            if (!instance || !mount) return;
            if (automatic) instance.cycle.Advance(mount.info.name, instance.marks, captured);
            instance.storeAssignments.Remove(mount);
        }
        internal static bool TryDesignation(Unit owner, WeaponInfo info, out GlobalPosition point)
        {
            var aircraft = owner as Aircraft;
            var station = aircraft?.weaponManager?.currentWeaponStation;
            if (station != null && station.WeaponInfo == info) return TryDesignation(owner, station, out point);
            point = default;
            return false;
        }
        internal static bool TryDesignation(Unit owner, WeaponStation station, out GlobalPosition point) => TryDesignation(owner, Guidance.NextStore(station), out point);
        internal static bool TryDesignation(Unit owner, MountedMissile mount, out GlobalPosition point)
        {
            point = default;
            if (!instance || !instance.armed || owner != Aircraft || !owner.IsServer || !mount || !Guidance.Supports(mount.info)) return false;
            var mark = instance.Assignment(mount);
            if (mark == null) return false;
            point = mark.point;
            return true;
        }
        Mark Assignment(MountedMissile mount)
        {
            if (!mount) return null;
            if (storeAssignments.TryGetValue(mount, out var mark)) return mark;
            var pylon = Guidance.Pylon(mount);
            if (pylon != null && pylonAssignments.TryGetValue(pylon, out mark)) return mark;
            if (!mount.info || !Guidance.Supports(mount.info)) return null;
            return assignments.TryGetValue(mount.info.name, out mark) ? mark : cycle.Current(mount.info.name, marks);
        }

        void Awake()
        {
            instance = this;
            var config = GetComponent<ArsenalPlugin>().Config;
            toggle = config.Bind("I-TGT", "ToggleKey", KeyCode.F6, "Open or close the GPS targeting MFD.");
            armKey = config.Bind("I-TGT", "ArmKey", KeyCode.F7, "Arm or disarm GPS targeting without opening the MFD.");
            replaceMap = config.Bind("I-TGT", "TopographicGameMap", false, "Use the generated relief and 100 m contours on the native map too.");
            windowWidth = config.Bind("I-TGT", "WindowWidth", 620f, new ConfigDescription("MFD width in pixels.", new AcceptableValueRange<float>(480, 1200)));
            windowHeight = config.Bind("I-TGT", "WindowHeight", 640f, new ConfigDescription("MFD height in pixels.", new AcceptableValueRange<float>(540, 1200)));
            window = new Rect(Mathf.Max(8, Screen.width - windowWidth.Value - 24), 120, windowWidth.Value, windowHeight.Value);
            harmony = new Harmony("ua.ncmod.arsenal.itgt");
            Guidance.Install(harmony);
            harmony.Patch(AccessTools.Method(typeof(SettingsMenu), "Start"), postfix: new HarmonyMethod(typeof(Display), nameof(SettingsOpened)));
            harmony.Patch(AccessTools.Method(typeof(Rewired.Player), "GetAxis", new[] { typeof(string) }), prefix: new HarmonyMethod(typeof(Display), nameof(CameraAxis)));
        }
        static bool CameraAxis(string __0, ref float __result)
        {
            if (!PointerOverDisplay || (__0 != "Zoom View" && __0 != "Pan View" && __0 != "Tilt View")) return true;
            __result = 0; return false;
        }
        static void SettingsOpened(SettingsMenu __instance) { __instance.gameObject.AddComponent<GPSSettingsPanel>(); }
        public sealed class GPSSettingsPanel : MonoBehaviour
        {
            void OnGUI() { if (instance && instance.button != null) instance.DrawSettings(); }
            void OnDestroy() { if (instance) instance.capturingKey = false; }
        }
        void DrawSettings()
        {
            var panel = new Rect(Screen.width - 336, Screen.height - 218, 312, 184);
            Fill(panel, new Color(.075f, .10f, .09f, .97f));
            GUI.Label(new Rect(panel.x + 12, panel.y + 8, 288, 24), "I-TGT GPS SETTINGS", label);
            if (Button(new Rect(panel.x + 12, panel.y + 40, 288, 30), capturingKey && !captureArmKey ? "PRESS KEY  /  ESC CANCELS" : "OPEN / CLOSE: " + toggle.Value))
            { capturingKey = true; captureArmKey = false; }
            if (Button(new Rect(panel.x + 12, panel.y + 78, 288, 30), capturingKey && captureArmKey ? "PRESS KEY  /  ESC CANCELS" : "ARM / DISARM GPS: " + armKey.Value))
            { capturingKey = true; captureArmKey = true; }
            if (Button(new Rect(panel.x + 12, panel.y + 116, 288, 26), "MAP: " + MapMode)) ToggleMap();
            GUI.Label(new Rect(panel.x + 12, panel.y + 148, 288, 22), "Bindings save automatically", small);
            var e = Event.current;
            if (!capturingKey || e.type != EventType.KeyDown || e.keyCode == KeyCode.None) return;
            var binding = captureArmKey ? armKey : toggle;
            if (e.keyCode != KeyCode.Escape && e.keyCode != (captureArmKey ? toggle.Value : armKey.Value))
            { binding.Value = e.keyCode; binding.ConfigFile.Save(); }
            capturingKey = false; e.Use();
        }
        void Update()
        {
            WeaponInventory.Update();
            var aircraft = Aircraft;
            if (aircraft != lastAircraft || missionOrigin != Datum.origin)
            {
                SetVisible(false); armed = false; marks.Clear(); assignments.Clear(); pylonAssignments.Clear(); storeAssignments.Clear(); cycle.Clear(); selected = 0; selectedStore = 0; nextID = 1;
                Guidance.Clear(); lastAircraft = aircraft;
                topography.Dispose(); missionOrigin = Datum.origin;
                if (aircraft)
                {
                    var point = aircraft.GlobalPosition(); center = new Vector2(point.x, point.z);
                    if (!positioned) { window.x = Mathf.Max(8, Screen.width - window.width - 24); positioned = true; }
                }
            }
            if (!aircraft || aircraft.disabled)
            {
                SetVisible(false); armed = false;
                if (!SceneSingleton<DynamicMap>.i) topography.Dispose();
                return;
            }
            topography.Update(SceneSingleton<DynamicMap>.i, replaceMap.Value);
            if (!capturingKey && !CursorManager.GetFlag(CursorFlags.GameMenu) && Input.GetKeyDown(toggle.Value)) SetVisible(!visible);
            if (!capturingKey && !DynamicMap.mapMaximized &&
                (CursorManager.GetFlags() & ~DisplayCursor) == CursorFlags.None &&
                armKey.Value != toggle.Value && Input.GetKeyDown(armKey.Value)) ToggleArmed(true);
            if (visible && Input.GetKeyDown(KeyCode.Escape)) SetVisible(false);
        }
        void SetVisible(bool value)
        {
            visible = value;
            CursorManager.SetFlag(DisplayCursor, value);
        }
        void OnDestroy()
        {
            SetVisible(false); topography.Dispose(); Guidance.Clear(); harmony?.UnpatchSelf();
            if (instance == this) instance = null;
        }
        void OnGUI()
        {
            var previousColor = GUI.color;
            var previousBackground = GUI.backgroundColor;
            try
            {
                if (label == null)
                {
                    label = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
                    label.normal.textColor = new Color(.77f, .89f, .77f);
                    small = new GUIStyle(label) { fontSize = 12 };
                    button = new GUIStyle(label) { fontSize = 11, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                    button.hover.textColor = Color.white;
                    button.active.textColor = new Color(.45f, 1f, .78f);
                    hudText = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                    hudText.normal.textColor = Color.white;
                }
                GUI.backgroundColor = new Color(.18f, .21f, .19f);
                if (!Aircraft || Aircraft.disabled) return;
                DrawFlightCue();
                if (!visible) return;
                window.width = Mathf.Clamp(window.width, 480, Mathf.Max(480, Screen.width - 16));
                window.height = Mathf.Clamp(window.height, 540, Mathf.Max(540, Screen.height - 16));
                window.x = Mathf.Clamp(window.x, 0, Mathf.Max(0, Screen.width - window.width));
                window.y = Mathf.Clamp(window.y, 0, Mathf.Max(0, Screen.height - window.height));
                window = GUI.Window(0x49544754, window, DrawWindow, "", GUIStyle.none);
            }
            finally { GUI.color = previousColor; GUI.backgroundColor = previousBackground; }
        }
        bool Button(Rect rect, string title, bool primary = false, bool enabled = true)
        {
            bool hover = rect.Contains(Event.current.mousePosition);
            bool down = hover && Input.GetMouseButton(0);
            bool accent = primary;
            Fill(new Rect(rect.x + 2, rect.y + 3, rect.width, rect.height), new Color(0, 0, 0, .35f));
            Fill(rect, hover ? new Color(.43f, .62f, .55f) : new Color(.27f, .35f, .32f));
            Fill(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), down ? new Color(.10f, .18f, .15f) :
                accent ? new Color(.16f, .32f, .25f) : hover ? new Color(.24f, .32f, .29f) : new Color(.17f, .22f, .20f));
            Fill(new Rect(rect.x + 3, rect.y + 2, rect.width - 6, 1), new Color(.65f, .8f, .71f, down ? .1f : .3f));
            bool wasEnabled = GUI.enabled;
            GUI.enabled = wasEnabled && enabled;
            try { return GUI.Button(rect, title, button); }
            finally { GUI.enabled = wasEnabled; }
        }
        void Fill(Rect rect, Color color)
        {
            var saved = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = saved;
        }
        string MapMode => replaceMap.Value ? "TOPOGRAPHIC" : "STANDARD";

        void ToggleMap()
        {
            replaceMap.Value = !replaceMap.Value;
            replaceMap.ConfigFile.Save();
        }

        void DrawWindow(int id)
        {
            float w = window.width, h = window.height;
            Fill(new Rect(0, 0, w, h), new Color(.055f, .08f, .085f, .99f));
            Fill(new Rect(0, 0, w, 3), new Color(.31f, .75f, .63f));
            GUI.Label(new Rect(16, 9, w - 60, 28), "I-TGT  /  GPS targeting", label);
            if (Button(new Rect(w - 40, 9, 28, 28), "×")) SetVisible(false);
            mapRect = new Rect(12, 116, w - 144, h - 228);
            var station = Aircraft.weaponManager?.currentWeaponStation;
            var info = station?.WeaponInfo;
            var stores = AvailableStores(station);
            selectedStore = Mathf.Clamp(selectedStore, 0, Mathf.Max(0, stores.Count - 1));
            var selectedMount = stores.Count > 0 ? stores[selectedStore] : null;
            var nextMount = Guidance.NextStore(station);
            string weapon = info ? info.weaponName : "No weapon selected";
            var assigned = Assignment(nextMount);
            string assignment = assigned != null ? "T" + assigned.id.ToString("00") : "—";
            Fill(new Rect(12, 44, w - 24, 38), new Color(.10f, .15f, .15f));
            if (Button(new Rect(18, 49, 102, 28), "Change weapon")) Aircraft.weaponManager?.NextWeaponStation();
            GUI.Label(new Rect(123, 47, w - 270, 32), weapon, small);
            GUI.Label(new Rect(w - 145, 47, 127, 32), "Next release  " + assignment, label);
            string scope = bindingScope == BindingScope.Cycle ? "CYCLE" : bindingScope == BindingScope.Type ? "TYPE" : bindingScope == BindingScope.Pylon ? "PYLON" : "STORE";
            string slot = selectedMount ? StoreLabel(selectedMount, station) : "NO STORE";
            GUI.Label(new Rect(12, 87, w - 24, 22), "Click map to mark  →  Arm GPS  →  Release normally", small);
            float x = w - 124;
            if (Button(new Rect(x, 116, 52, 28), "Zoom +")) Zoom(.7f);
            if (Button(new Rect(x + 60, 116, 52, 28), "Zoom −")) Zoom(1.4f);
            if (Button(new Rect(x, 146, 112, 28), "Center aircraft")) { var p = Aircraft.GlobalPosition(); center = new Vector2(p.x, p.z); }
            if (Button(new Rect(x, 176, 112, 28), "Center mark", enabled: Active != null))
                center = new Vector2(Active.point.x, Active.point.z);
            if (Button(new Rect(x, 206, 112, 28), replaceMap.Value ? "Topography on" : "Topography off")) ToggleMap();
            if (Button(new Rect(x, 236, 112, 28), "Import Data Link")) ImportDatalink();
            if (Button(new Rect(x, 266, 52, 28), "Prev", enabled: marks.Count > 1)) selected = (selected + marks.Count - 1) % marks.Count;
            if (Button(new Rect(x + 60, 266, 52, 28), "Next", enabled: marks.Count > 1)) selected = (selected + 1) % marks.Count;
            if (Button(new Rect(x, 296, 112, 28), "Delete mark", enabled: Active != null)) Delete();
            if (Button(new Rect(x, 326, 112, 28), advancedBindings ? "Hide bindings" : "Weapon bindings", advancedBindings))
                advancedBindings = !advancedBindings;
            if (advancedBindings)
            {
                if (Button(new Rect(x, 356, 112, 22), "Mode: " + scope))
                    bindingScope = (BindingScope)(((int)bindingScope + 1) % 4);
                if (Button(new Rect(x, 380, 112, 22), slot, enabled: stores.Count > 0))
                    selectedStore = (selectedStore + 1) % stores.Count;
                if (Button(new Rect(x, 404, 112, 22), bindingScope == BindingScope.Cycle ? "Reset sequence" : "Bind mark", enabled: Active != null))
                    Bind(info, selectedMount);
            }
            DrawMap();
            float bottom = h - 108;
            Fill(new Rect(12, bottom, w - 24, 64), new Color(.10f, .15f, .15f));
            GUI.Label(new Rect(56, bottom, w - 112, 24), Active == null ? "NO MARK  /  CLICK MAP TO DESIGNATE" :
                $"T{Active.id:00}   E {Active.point.x / 1000:0.000} km   N {Active.point.z / 1000:0.000} km   H {Active.point.y:0} m", small);
            string hint = Guidance.WeaponHint(info);
            string help = hint ?? (Active == null ? "Left-click to mark  /  Right-drag to pan  /  Scroll to zoom" : message);
            GUI.Label(new Rect(32, bottom + 26, w - 64, 22), Aircraft.IsServer ? help : "GPS RELEASE AVAILABLE TO HOST ONLY", small);
            GUI.Label(new Rect(12, h - 42, 125, 28), $"{span / 1000:0.#} km  /  {(armed ? "GPS ON" : "GPS OFF")}", small);
            if (Button(new Rect(w / 2 - 105, h - 42, 210, 28), (armed ? "Disarm GPS" : "Arm GPS") + " [" + armKey.Value + "]", true))
                ToggleArmed(false);
            Resize(new Rect(w - 30, h - 30, 24, 24));
            GUI.DragWindow(new Rect(12, 3, w - 60, 36));
        }
        void ToggleArmed(bool notify)
        {
            var aircraft = Aircraft;
            if (!aircraft || aircraft.disabled) return;
            var station = aircraft.weaponManager?.currentWeaponStation;
            var mark = Assignment(Guidance.NextStore(station));
            if (armed) { armed = false; message = "GPS disarmed"; }
            else if (!aircraft.IsServer) message = "GPS release is available to the host";
            else if (!Guidance.Supports(station?.WeaponInfo)) message = Guidance.WeaponHint(station?.WeaponInfo);
            else if (Active == null) message = $"Open I-TGT [{toggle.Value}] and place a mark";
            else if (mark == null) message = "No GPS assignment for the next store";
            else { armed = true; message = $"GPS armed for next release: T{mark.id:00}"; }

            var report = SceneSingleton<AircraftActionsReport>.i;
            if (notify && report) report.ReportText(message, 3);
        }
        static List<MountedMissile> AvailableStores(WeaponStation station)
        {
            var result = new List<MountedMissile>();
            if (station != null) foreach (var weapon in station.Weapons)
                if (weapon is MountedMissile mount && Guidance.Available(mount)) result.Add(mount);
            return result;
        }
        static string StoreLabel(MountedMissile mount, WeaponStation station)
        {
            var pylon = Guidance.Pylon(mount);
            string name = pylon != null && pylon.HardpointIndex >= 0 ? "P" + (pylon.HardpointIndex + 1).ToString("00") : "P?";
            return name + " / STORE " + (station.Weapons.IndexOf(mount) + 1).ToString("00");
        }
        void Bind(WeaponInfo info, MountedMissile mount)
        {
            if (!Guidance.Supports(info) || Active == null) { message = "Cannot bind: compatible weapon and mark required"; return; }
            if (bindingScope == BindingScope.Cycle) { assignments.Remove(info.name); cycle.Reset(info.name); message = "CYCLE: T01 onward, wraps after last mark"; return; }
            if (bindingScope == BindingScope.Type) assignments[info.name] = Active;
            else if (bindingScope == BindingScope.Pylon)
            {
                var pylon = Guidance.Pylon(mount);
                if (pylon == null) { message = "No pylon selected"; return; }
                pylonAssignments[pylon] = Active;
            }
            else
            {
                if (!mount) { message = "No store selected"; return; }
                storeAssignments[mount] = Active;
            }
            message = $"{bindingScope}: bound to T{Active.id:00}";
        }
        void Delete()
        {
            if (Active == null) return;
            var removed = Active;
            cycle.Remove(removed, marks);
            foreach (var key in new List<string>(assignments.Keys)) if (assignments[key] == removed) assignments.Remove(key);
            foreach (var key in new List<Hardpoint>(pylonAssignments.Keys)) if (pylonAssignments[key] == removed) pylonAssignments.Remove(key);
            foreach (var key in new List<MountedMissile>(storeAssignments.Keys)) if (storeAssignments[key] == removed) storeAssignments.Remove(key);
            marks.Remove(removed); selected = Mathf.Clamp(selected, 0, Mathf.Max(0, marks.Count - 1));
            if (marks.Count == 0) { armed = false; cycle.Clear(); nextID = 1; selected = 0; message = "All marks cleared: next mark is T01"; }
        }
        void ImportDatalink()
        {
            var targets = Aircraft.weaponManager?.GetTargetList();
            var target = targets != null && targets.Count > 0 ? targets[0] : null;
            if (!target || !Aircraft.NetworkHQ) { message = "Select a Data Link target first"; return; }
            var known = Aircraft.NetworkHQ.GetKnownPosition(target);
            if (!known.HasValue) { message = "Data Link has no known target position"; return; }
            if (marks.Count >= 16) { message = "16 marks maximum: delete a mark first"; return; }
            marks.Add(new Mark { id = nextID++, point = known.Value }); selected = marks.Count - 1;
            message = "Data Link position copied: BIND then ARM";
        }
        void Zoom(float factor) => span = Mathf.Clamp(span * factor, 1000, Mathf.Max(topography.Size.x, 80000));
        void DrawMap()
        {
            Fill(mapRect, new Color(.85f, .88f, .80f));
            GUI.BeginGroup(mapRect);
            float width = mapRect.width, height = mapRect.height;
            float vertical = span * height / width;
            bool relief = replaceMap.Value && topography.Ready;
            var mapTexture = relief ? topography.Texture : topography.StandardTexture;
            if (mapTexture)
            {
                var size = topography.Size;
                var uv = new Rect((center.x - span / 2) / size.x + .5f, (center.y - vertical / 2) / size.y + .5f, span / size.x, vertical / size.y);
                GUI.DrawTextureWithTexCoords(new Rect(0, 0, width, height), mapTexture, relief ? uv : topography.StandardUV(uv));
                if (!relief) topography.DrawSatellite(new Rect(0, 0, width, height), uv);
            }
            else GUI.Label(new Rect(10, 10, width - 20, 50), $"SAMPLING TERRAIN  {topography.Progress:P0}", GUI.skin.label);
            float grid = span > 30000 ? 10000 : span > 12000 ? 5000 : span > 4000 ? 1000 : 250;
            for (float x = Mathf.Ceil((center.x - span / 2) / grid) * grid; x < center.x + span / 2; x += grid)
            {
                float px = (x - center.x) / span * width + width / 2;
                Fill(new Rect(px, 0, 1, height), new Color(.25f, .29f, .23f, .38f));
                GUI.Label(new Rect(px + 3, height - 20, 60, 18), (x / 1000).ToString("0.##"));
            }
            for (float z = Mathf.Ceil((center.y - vertical / 2) / grid) * grid; z < center.y + vertical / 2; z += grid)
            {
                float py = height / 2 - (z - center.y) / span * width;
                Fill(new Rect(0, py, width, 1), new Color(.25f, .29f, .23f, .38f));
                GUI.Label(new Rect(3, py + 2, 60, 18), (z / 1000).ToString("0.##"));
            }
            foreach (var mark in marks)
            {
                var p = Pixel(mark.point);
                var color = mark == Active ? new Color(.85f, .16f, .08f) : new Color(.45f, .27f, .12f);
                Fill(new Rect(p.x - 9, p.y, 18, 2), color); Fill(new Rect(p.x, p.y - 9, 2, 18), color);
                GUI.Label(new Rect(p.x + 10, p.y - 12, 70, 22), $"T{mark.id:00}");
            }
            var own = Pixel(Aircraft.GlobalPosition());
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(Aircraft.transform.eulerAngles.y, own);
            var aircraftColor = new Color(.08f, .25f, .65f);
            Fill(new Rect(own.x - 2, own.y - 9, 4, 18), aircraftColor);
            Fill(new Rect(own.x - 8, own.y - 1, 16, 3), aircraftColor);
            Fill(new Rect(own.x - 5, own.y + 6, 10, 2), aircraftColor);
            Fill(new Rect(own.x - 1, own.y - 13, 2, 4), Color.white); GUI.matrix = matrix;
            GUI.Label(new Rect(width - 30, 5, 30, 22), "N ↑");
            GUI.EndGroup();
            var e = Event.current;
            if (!mapRect.Contains(e.mousePosition)) return;
            if (e.type == EventType.ScrollWheel) { Zoom(e.delta.y > 0 ? 1.2f : .8f); e.Use(); }
            else if (e.type == EventType.MouseDrag && e.button == 1)
            {
                center += new Vector2(-e.delta.x, e.delta.y) * span / mapRect.width;
                ClampCenter(); e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 0)
            {
                var local = e.mousePosition - mapRect.position;
                var point = new GlobalPosition(center.x + (local.x - width / 2) * span / width, 0,
                    center.y + (height / 2 - local.y) * span / width);
                var size = topography.Size;
                if (size.x <= 0 || Mathf.Abs(point.x) > size.x / 2 || Mathf.Abs(point.z) > size.y / 2) message = "Point is outside the map";
                else if (marks.Count >= 16) message = "16 marks maximum: delete a mark first";
                else
                {
                    TopographicMap.Height(point, out var elevation); point.y = elevation;
                    marks.Add(new Mark { id = nextID++, point = point }); selected = marks.Count - 1;
                    message = bindingScope == BindingScope.Cycle ? "Mark created: check next release, then arm GPS" : "Mark created: use BIND, then arm GPS";
                }
                e.Use();
            }
        }
        void DrawFlightCue()
        {
            // Flight symbology yields to map, MFD, chat and menu layers.
            if (visible || DynamicMap.mapMaximized || CursorManager.GetFlags() != CursorFlags.None) return;
            var aircraft = Aircraft;
            var station = aircraft.weaponManager?.currentWeaponStation;
            var mark = armed ? Assignment(Guidance.NextStore(station)) : Active;
            var cameraManager = SceneSingleton<CameraStateManager>.i;
            if (!cameraManager || CameraStateManager.cameraMode != CameraMode.cockpit || cameraManager.currentState != cameraManager.cockpitState) return;
            var camera = cameraManager ? cameraManager.mainCamera : null;
            if (mark == null || !camera || !aircraft.rb) return;
            var delta = mark.point - aircraft.GlobalPosition();
            var horizontal = new Vector3(delta.x, 0, delta.z);
            float distance = horizontal.magnitude;
            var velocity = aircraft.rb.velocity;
            var flatVelocity = new Vector3(velocity.x, 0, velocity.z);
            float closing = Vector3.Dot(flatVelocity, horizontal.normalized);
            float angle = flatVelocity.sqrMagnitude > 1 ? Vector3.SignedAngle(flatVelocity, horizontal, Vector3.up) : 0;
            string status = armed ? "GPS TARGET" : "PREVIEW / GPS OFF";
            Color color = new Color(1f, .86f, .42f);
            string rangeText = "";
            var info = station?.WeaponInfo;
            if (armed && info && (info.bomb || info.glideBomb))
            {
                float height = -delta.y;
                float fallTime = Kinematics.FallTime(Mathf.Max(0, height), velocity.y);
                float ballisticRange = Mathf.Max(0, closing) * fallTime;
                var missile = info.weaponPrefab.GetComponent<Missile>();
                float maxRange = missile ? missile.CalcRange(flatVelocity.magnitude, aircraft.GlobalPosition().y, mark.point.y, distance, 0, out _) : ballisticRange;
                if (float.IsNaN(maxRange) || float.IsInfinity(maxRange) || maxRange <= 0) maxRange = ballisticRange;
                // Native range estimation plus a conservative delivery window, not an impact guarantee.
                float deliveryRange = info.glideBomb ? maxRange * .85f : Mathf.Min(maxRange, ballisticRange);
                bool aligned = Mathf.Abs(angle) <= 8 && closing > 30;
                if (height < 50) status = "CLIMB / TOO LOW";
                else if (!aligned) status = angle < 0 ? "TURN LEFT" : "TURN RIGHT";
                else if (distance > deliveryRange) status = "TOO FAR / HOLD";
                else { status = "RELEASE WINDOW ~"; color = new Color(.5f, 1f, .72f); }
                rangeText = $"  /  EST RANGE {deliveryRange / 1000:0.0} km";
            }
            var projected = camera.WorldToScreenPoint(mark.point.ToLocalPosition());
            var point = new Vector2(projected.x, Screen.height - projected.y);
            bool onScreen = projected.z > 0 && point.x > 60 && point.x < Screen.width - 60 && point.y > 100 && point.y < Screen.height - 100;
            if (!onScreen)
            {
                var direction = new Vector2(projected.x - Screen.width / 2f, Screen.height / 2f - projected.y);
                if (projected.z < 0) direction = -direction;
                if (direction.sqrMagnitude < 1) direction = Vector2.up;
                float factor = Mathf.Min((Screen.width / 2f - 70) / Mathf.Max(Mathf.Abs(direction.x), 1),
                    (Screen.height / 2f - 110) / Mathf.Max(Mathf.Abs(direction.y), 1));
                point = new Vector2(Screen.width / 2f, Screen.height / 2f) + direction * factor;
            }
            var matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(45, point);
            Fill(new Rect(point.x - 11, point.y - 11, 22, 5), new Color(0, 0, 0, .85f));
            Fill(new Rect(point.x - 11, point.y + 6, 22, 5), new Color(0, 0, 0, .85f));
            Fill(new Rect(point.x - 11, point.y - 11, 5, 22), new Color(0, 0, 0, .85f));
            Fill(new Rect(point.x + 6, point.y - 11, 5, 22), new Color(0, 0, 0, .85f));
            Fill(new Rect(point.x - 9, point.y - 9, 18, 2), color);
            Fill(new Rect(point.x - 9, point.y + 7, 18, 2), color);
            Fill(new Rect(point.x - 9, point.y - 9, 2, 18), color);
            Fill(new Rect(point.x + 7, point.y - 9, 2, 18), color);
            GUI.matrix = matrix;
            Fill(new Rect(point.x - 1, point.y - 1, 2, 2), color);
            if (onScreen)
            {
                Fill(new Rect(point.x, point.y + 15, 1, 7), color);
                Fill(new Rect(point.x - 6, point.y + 22, 13, 1), color);
            }
            else
            {
                float bearing = Mathf.Atan2(point.y - Screen.height / 2f, point.x - Screen.width / 2f) * Mathf.Rad2Deg;
                GUIUtility.RotateAroundPivot(bearing, point);
                Fill(new Rect(point.x + 14, point.y - 1, 8, 2), color);
                GUI.matrix = matrix;
            }
            HudLabel(new Rect(Mathf.Clamp(point.x - 110, 0, Screen.width - 220), point.y + 27, 220, 24), $"T{mark.id:00}  {distance / 1000:0.0} km" + (onScreen ? "" : "  >"), color);
            var panel = new Rect(Screen.width / 2f - 245, Screen.height * .32f, 490, 42);
            HudLabel(new Rect(panel.x + 8, panel.y, panel.width - 16, 20), $"GPS T{mark.id:00}  /  {status}", color);
            HudLabel(new Rect(panel.x + 8, panel.y + 20, panel.width - 16, 20), $"{distance / 1000:0.0} km  /  HDG {(Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg + 360) % 360:000}°{rangeText}", color);
        }
        void HudLabel(Rect rect, string text, Color color)
        {
            var saved = GUI.color;
            GUI.color = new Color(0, 0, 0, .9f);
            GUI.Label(new Rect(rect.x - 1, rect.y, rect.width, rect.height), text, hudText);
            GUI.Label(new Rect(rect.x + 1, rect.y, rect.width, rect.height), text, hudText);
            GUI.Label(new Rect(rect.x, rect.y - 1, rect.width, rect.height), text, hudText);
            GUI.Label(new Rect(rect.x, rect.y + 1, rect.width, rect.height), text, hudText);
            GUI.color = color; GUI.Label(rect, text, hudText); GUI.color = saved;
        }
        void ClampCenter()
        {
            if (topography.Size.x <= 0) return;
            center.x = Mathf.Clamp(center.x, -topography.Size.x / 2, topography.Size.x / 2);
            center.y = Mathf.Clamp(center.y, -topography.Size.y / 2, topography.Size.y / 2);
        }
        Vector2 Pixel(GlobalPosition point) => new Vector2(mapRect.width / 2 + (point.x - center.x) * mapRect.width / span,
            mapRect.height / 2 - (point.z - center.y) * mapRect.width / span);
        void Resize(Rect grip)
        {
            GUI.Label(grip, "◢", small);
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && grip.Contains(e.mousePosition)) { resizing = true; e.Use(); }
            if (resizing && e.type == EventType.MouseDrag)
            {
                window.width += e.delta.x; window.height += e.delta.y; e.Use();
            }
            if (resizing && e.type == EventType.MouseUp)
            {
                resizing = false; windowWidth.Value = Mathf.Clamp(window.width, 480, 1200); windowHeight.Value = Mathf.Clamp(window.height, 540, 1200); e.Use();
            }
        }
    }
}
