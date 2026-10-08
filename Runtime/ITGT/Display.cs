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
        enum BindingScope { Type, Pylon, Store }
        BindingScope bindingScope;
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
        int selected, nextID = 1;
        Vector2 center;
        float span = 40000;
        GUIStyle label, small, button;
        string message = "LMB: mark   RMB: pan   Wheel: zoom";
        Mark Active => marks.Count > 0 ? marks[Mathf.Clamp(selected, 0, marks.Count - 1)] : null;
        public static Aircraft Aircraft => SceneSingleton<CombatHUD>.i ? SceneSingleton<CombatHUD>.i.aircraft : null;
        static Rect Launcher => new Rect(Mathf.Max(8, Screen.width - 112), 80, 96, 30);
        public static bool PointerOverDisplay
        {
            get
            {
                if (!instance || !Aircraft) return false;
                var mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                return Launcher.Contains(mouse) || (instance.visible && instance.window.Contains(mouse));
            }
        }
        internal static void Trace(string value) => Log.LogInfo(value);
        internal static bool GpsArmedFor(Unit owner, WeaponInfo info) => instance && instance.armed && owner == Aircraft && owner && owner.IsServer && Guidance.Supports(info);
        internal static void UnassignedNotice() { if (instance) instance.message = "Next store has no mark: BIND it or disarm GPS"; }
        internal static void ConsumeStore(MountedMissile mount) { if (instance) instance.storeAssignments.Remove(mount); }
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
            return mount.info && assignments.TryGetValue(mount.info.name, out mark) ? mark : null;
        }

        void Awake()
        {
            instance = this;
            var config = GetComponent<ArsenalPlugin>().Config;
            toggle = config.Bind("I-TGT", "ToggleKey", KeyCode.F6, "Open or close the GPS targeting MFD.");
            replaceMap = config.Bind("I-TGT", "TopographicGameMap", true, "Use the generated relief and 100 m contours on the native map too.");
            windowWidth = config.Bind("I-TGT", "WindowWidth", 620f, new ConfigDescription("MFD width in pixels.", new AcceptableValueRange<float>(480, 1200)));
            windowHeight = config.Bind("I-TGT", "WindowHeight", 640f, new ConfigDescription("MFD height in pixels.", new AcceptableValueRange<float>(540, 1200)));
            window = new Rect(Mathf.Max(8, Screen.width - windowWidth.Value - 24), 120, windowWidth.Value, windowHeight.Value);
            harmony = new Harmony("ua.ncmod.arsenal.itgt");
            Guidance.Install(harmony);
        }
        void Update()
        {
            var aircraft = Aircraft;
            if (aircraft != lastAircraft || missionOrigin != Datum.origin)
            {
                SetVisible(false); armed = false; marks.Clear(); assignments.Clear(); pylonAssignments.Clear(); storeAssignments.Clear(); selected = 0; selectedStore = 0; nextID = 1;
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
            if (Input.GetKeyDown(toggle.Value)) SetVisible(!visible);
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
            if (!Aircraft || Aircraft.disabled) return;
            var previousColor = GUI.color;
            var previousBackground = GUI.backgroundColor;
            try
            {
                if (label == null)
                {
                    label = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter };
                    label.normal.textColor = new Color(.77f, .89f, .77f);
                    small = new GUIStyle(label) { fontSize = 12 };
                    button = new GUIStyle(GUI.skin.button) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
                }
                GUI.backgroundColor = new Color(.18f, .21f, .19f);
                if (GUI.Button(Launcher, "I-TGT  " + toggle.Value, button)) SetVisible(!visible);
                if (!visible) return;
                window.width = Mathf.Clamp(window.width, 480, Mathf.Max(480, Screen.width - 16));
                window.height = Mathf.Clamp(window.height, 540, Mathf.Max(540, Screen.height - 16));
                window.x = Mathf.Clamp(window.x, 0, Mathf.Max(0, Screen.width - window.width));
                window.y = Mathf.Clamp(window.y, 0, Mathf.Max(0, Screen.height - window.height));
                window = GUI.Window(0x49544754, window, DrawWindow, "", GUIStyle.none);
            }
            finally { GUI.color = previousColor; GUI.backgroundColor = previousBackground; }
        }
        bool Button(Rect rect, string title) => GUI.Button(rect, title, button);
        void Fill(Rect rect, Color color)
        {
            var saved = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = saved;
        }
        void DrawWindow(int id)
        {
            float w = window.width, h = window.height;
            Fill(new Rect(0, 0, w, h), new Color(.09f, .105f, .10f, .98f));
            Fill(new Rect(9, 9, w - 18, h - 18), new Color(.15f, .17f, .16f));
            GUI.Label(new Rect(95, 14, w - 155, 28), "I-TGT   /   GPS TARGETING", label);
            if (Button(new Rect(39, 12, 48, 28), "WPN")) Aircraft.weaponManager?.NextWeaponStation();
            if (Button(new Rect(w - 45, 12, 30, 28), "X")) SetVisible(false);
            Fill(new Rect(20, 21, 9, 9), armed ? new Color(.3f, .95f, .3f) : new Color(.35f, .39f, .35f));
            mapRect = new Rect(64, 104, w - 128, h - 244);
            var station = Aircraft.weaponManager?.currentWeaponStation;
            var info = station?.WeaponInfo;
            var stores = AvailableStores(station);
            selectedStore = Mathf.Clamp(selectedStore, 0, Mathf.Max(0, stores.Count - 1));
            var selectedMount = stores.Count > 0 ? stores[selectedStore] : null;
            var nextMount = Guidance.NextStore(station);
            bool compatible = Guidance.Supports(info);
            string weapon = info ? info.weaponName : "NO WEAPON";
            var assigned = Assignment(nextMount);
            string assignment = assigned != null ? "T" + assigned.id.ToString("00") : "NONE";
            GUI.Label(new Rect(58, 49, w - 116, 24), weapon + "  /  NEXT GPS " + assignment, small);
            string scope = bindingScope == BindingScope.Type ? "TYPE" : bindingScope == BindingScope.Pylon ? "PYLON" : "STORE";
            string slot = selectedMount ? StoreLabel(selectedMount, station) : "NO STORE";
            GUI.Label(new Rect(58, 74, w - 116, 22), (armed ? "GPS ON" : "GPS OFF") + "  /  BIND: " + scope + "  /  " + slot, small);
            if (Button(new Rect(14, 112, 43, 34), "+")) Zoom(.7f);
            if (Button(new Rect(14, 160, 43, 34), "−")) Zoom(1.4f);
            if (Button(new Rect(14, 208, 43, 34), "OWN")) { var p = Aircraft.GlobalPosition(); center = new Vector2(p.x, p.z); }
            if (Button(new Rect(14, 256, 43, 34), "TGT") && Active != null) center = new Vector2(Active.point.x, Active.point.z);
            if (Button(new Rect(14, 304, 43, 34), "TOPO")) replaceMap.Value = !replaceMap.Value;
            if (Button(new Rect(14, 352, 43, 34), "SCOPE")) bindingScope = (BindingScope)(((int)bindingScope + 1) % 3);
            if (Button(new Rect(w - 57, 352, 43, 34), "SLOT") && stores.Count > 0) selectedStore = (selectedStore + 1) % stores.Count;
            if (Button(new Rect(w - 57, 112, 43, 34), "PREV") && marks.Count > 0) selected = (selected + marks.Count - 1) % marks.Count;
            if (Button(new Rect(w - 57, 160, 43, 34), "NEXT") && marks.Count > 0) selected = (selected + 1) % marks.Count;
            if (Button(new Rect(w - 57, 208, 43, 34), "DEL")) Delete();
            if (Button(new Rect(w - 57, 256, 43, 34), "BIND")) Bind(info, selectedMount);
            if (Button(new Rect(w - 57, 304, 43, 34), "GPS"))
            {
                if (armed) armed = false;
                else if (!Aircraft.IsServer) message = "GPS release: single player / host only";
                else if (compatible && Active != null) { Bind(info, selectedMount); armed = true; }
                else message = "Select a compatible weapon and create a mark";
            }
            DrawMap();
            float bottom = h - 126;
            GUI.Label(new Rect(56, bottom, w - 112, 24), Active == null ? "NO MARK  /  CLICK MAP TO DESIGNATE" :
                $"T{Active.id:00}   E {Active.point.x / 1000:0.000} km   N {Active.point.z / 1000:0.000} km   H {Active.point.y:0} m", small);
            GUI.Label(new Rect(40, bottom + 26, w - 80, 22), compatible ? (Aircraft.IsServer ? message : "GPS RELEASE AVAILABLE TO HOST ONLY") : "Selected weapon does not support coordinate guidance", small);
            GUI.Label(new Rect(56, bottom + 50, w - 112, 22), $"RANGE {span / 1000:0.0} km   /   CONTOURS 100 m   /   NORTH UP", small);
            if (Button(new Rect(w / 2 - 90, h - 42, 180, 28), armed ? "GPS ON  •  DISARM" : "GPS OFF  •  ARM"))
            {
                if (armed) armed = false;
                else if (Aircraft.IsServer && compatible && Active != null) { Bind(info, selectedMount); armed = true; }
                else message = "Select a compatible weapon and create a mark (host)";
            }
            Resize(new Rect(w - 30, h - 30, 24, 24));
            GUI.DragWindow(new Rect(45, 0, w - 100, 45));
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
            foreach (var key in new List<string>(assignments.Keys)) if (assignments[key] == removed) assignments.Remove(key);
            foreach (var key in new List<Hardpoint>(pylonAssignments.Keys)) if (pylonAssignments[key] == removed) pylonAssignments.Remove(key);
            foreach (var key in new List<MountedMissile>(storeAssignments.Keys)) if (storeAssignments[key] == removed) storeAssignments.Remove(key);
            marks.Remove(removed); selected = Mathf.Clamp(selected, 0, Mathf.Max(0, marks.Count - 1));
            if (marks.Count == 0) armed = false;
        }
        void Zoom(float factor) => span = Mathf.Clamp(span * factor, 1000, Mathf.Max(topography.Size.x, 80000));
        void DrawMap()
        {
            Fill(mapRect, new Color(.85f, .88f, .80f));
            GUI.BeginGroup(mapRect);
            float width = mapRect.width, height = mapRect.height;
            float vertical = span * height / width;
            if (topography.Ready)
            {
                var size = topography.Size;
                GUI.DrawTextureWithTexCoords(new Rect(0, 0, width, height), topography.Texture,
                    new Rect((center.x - span / 2) / size.x + .5f, (center.y - vertical / 2) / size.y + .5f, span / size.x, vertical / size.y));
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
            Fill(new Rect(own.x - 2, own.y - 8, 4, 16), new Color(.12f, .35f, .8f));
            Fill(new Rect(own.x - 8, own.y - 2, 16, 3), new Color(.12f, .35f, .8f)); GUI.matrix = matrix;
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
                    message = "Mark created: BIND to weapon, then ARM GPS";
                }
                e.Use();
            }
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
