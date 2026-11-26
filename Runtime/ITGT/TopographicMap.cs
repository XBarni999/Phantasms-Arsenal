using UnityEngine;
using UnityEngine.UI;

namespace PhantasmsArsenal.ITGT
{
    // Coordinates are stored in Datum space. Sampling always converts back to current local space.
    internal sealed class TopographicMap
    {
        const int Resolution = 2048;
        static readonly System.Reflection.FieldInfo MapDimensions = HarmonyLib.AccessTools.Field(typeof(DynamicMap), "mapDimensions");
        float[] heights;
        bool[] ground;
        Image image;
        RawImage reliefLayer;
        Sprite original;
        Color32[] pixels;
        readonly System.Diagnostics.Stopwatch frameBudget = new System.Diagnostics.Stopwatch();
        int row, shadedRow;
        public Texture2D Texture { get; private set; }
        public Vector2 Size { get; private set; }
        public bool Ready => Texture;
        public Texture2D StandardTexture => original ? original.texture : null;
        public Rect StandardUV(Rect uv)
        {
            Rect source = original.textureRect;
            var texture = original.texture;
            return new Rect((source.x + uv.x * source.width) / texture.width,
                (source.y + uv.y * source.height) / texture.height,
                uv.width * source.width / texture.width, uv.height * source.height / texture.height);
        }
        public float Progress => (row * .75f + shadedRow * .25f) / Resolution;

        public void DrawSatellite(Rect area, Rect view)
        {
            var satellite = image ? image.transform.Find("NuclearSatelliteMap") : null;
            if (!satellite || !satellite.gameObject.activeInHierarchy) return;
            Color previous = GUI.color;
            try
            {
                for (int i = 0; i < satellite.childCount; i++)
                {
                    var tile = satellite.GetChild(i).GetComponent<RawImage>();
                    if (!tile || !tile.isActiveAndEnabled || !tile.texture) continue;
                    Vector2 min = tile.rectTransform.anchorMin, max = tile.rectTransform.anchorMax;
                    float left = Mathf.Max(view.xMin, min.x), right = Mathf.Min(view.xMax, max.x);
                    float bottom = Mathf.Max(view.yMin, min.y), top = Mathf.Min(view.yMax, max.y);
                    if (left >= right || bottom >= top) continue;
                    Rect uv = tile.uvRect;
                    GUI.color = previous * tile.color;
                    GUI.DrawTextureWithTexCoords(
                        new Rect(area.x + (left - view.xMin) / view.width * area.width,
                            area.y + (view.yMax - top) / view.height * area.height,
                            (right - left) / view.width * area.width, (top - bottom) / view.height * area.height),
                        tile.texture,
                        new Rect(uv.x + (left - min.x) / (max.x - min.x) * uv.width,
                            uv.y + (bottom - min.y) / (max.y - min.y) * uv.height,
                            (right - left) / (max.x - min.x) * uv.width, (top - bottom) / (max.y - min.y) * uv.height));
                }
            }
            finally { GUI.color = previous; }
        }

        public void Update(DynamicMap map, bool replace)
        {
            var next = map && map.mapImage ? map.mapImage.GetComponent<Image>() : null;
            var dimensions = next ? (Vector2)MapDimensions.GetValue(map) : Vector2.zero;
            if (image != next || Size != dimensions)
                Dispose();
            if (!next || !next.sprite) return;
            if (!image)
            {
                image = next;
                Size = dimensions;
                if (Size.x <= 0 || Size.y <= 0) { Dispose(); return; }
            }
            original = image.overrideSprite ? image.overrideSprite : image.sprite;
            // Bound both terrain sampling and relief shading; larger maps build across frames.
            if (!Texture && replace)
            {
                if (heights == null)
                {
                    heights = new float[Resolution * Resolution];
                    ground = new bool[heights.Length];
                }
                frameBudget.Restart();
                int end = Mathf.Min(row + 16, Resolution);
                for (; row < end && frameBudget.ElapsedMilliseconds < 3; row++)
                    for (int x = 0; x < Resolution; x++)
                    {
                        int i = row * Resolution + x;
                        ground[i] = Height(new GlobalPosition((x / (Resolution - 1f) - .5f) * Size.x, 0,
                            (row / (Resolution - 1f) - .5f) * Size.y), out heights[i]);
                    }
                if (row == Resolution) Build();
            }
            if (Texture && replace)
            {
                if (!reliefLayer) CreateLayer();
                var satellite = image.transform.Find("NuclearSatelliteMap");
                int index = satellite ? 1 : 0;
                if (reliefLayer.transform.GetSiblingIndex() != index) reliefLayer.transform.SetSiblingIndex(index);
                reliefLayer.enabled = true;
            }
            else if (reliefLayer) reliefLayer.enabled = false;
        }

        void CreateLayer()
        {
            var layer = new GameObject("I-TGT Topography", typeof(RectTransform), typeof(RawImage));
            layer.layer = image.gameObject.layer;
            var rect = (RectTransform)layer.transform;
            rect.SetParent(image.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            reliefLayer = layer.GetComponent<RawImage>();
            reliefLayer.texture = Texture;
            reliefLayer.raycastTarget = false;
        }

        public static bool Height(GlobalPosition position, out float elevation)
        {
            var local = position.ToLocalPosition();
            local.y = Datum.LocalSeaY + 18000;
            if (Physics.Raycast(local, Vector3.down, out var hit, 24000, PhysicsLayers.StaticsMask, QueryTriggerInteraction.Ignore))
            {
                elevation = Mathf.Max(0, hit.point.GlobalY());
                return true;
            }
            elevation = 0;
            return false;
        }

        void Build()
        {
            if (pixels == null) pixels = new Color32[heights.Length];
            float dx = Size.x / (Resolution - 1), dz = Size.y / (Resolution - 1);
            int end = Mathf.Min(shadedRow + 16, Resolution);
            for (; shadedRow < end && frameBudget.ElapsedMilliseconds < 3; shadedRow++)
            {
                int y = shadedRow;
                for (int x = 0; x < Resolution; x++)
                {
                    int i = y * Resolution + x;
                    float h = heights[i];
                    Color color = !ground[i] || h < 1 ? new Color(.71f, .84f, .88f) :
                        Color.Lerp(new Color(.87f, .90f, .76f), new Color(.89f, .83f, .69f), Mathf.Clamp01(h / 1600));
                    if (ground[i] && h >= 1)
                    {
                        int left = y * Resolution + Mathf.Max(0, x - 1);
                        int below = Mathf.Max(0, y - 1) * Resolution + x;
                        float shade = Mathf.Clamp(((heights[left] - h) / dx + (h - heights[below]) / dz) * .4f, -.16f, .16f);
                        color *= 1 + shade;
                        bool contour = Mathf.FloorToInt(h / 100) != Mathf.FloorToInt(heights[left] / 100) ||
                            Mathf.FloorToInt(h / 100) != Mathf.FloorToInt(heights[below] / 100);
                        bool major = Mathf.FloorToInt(h / 500) != Mathf.FloorToInt(heights[left] / 500) ||
                            Mathf.FloorToInt(h / 500) != Mathf.FloorToInt(heights[below] / 500);
                        if (contour) color = Color.Lerp(color, new Color(.39f, .32f, .22f), major ? .5f : .25f);
                    }
                    color.a = 1;
                    pixels[i] = color;
                }
            }
            if (shadedRow < Resolution) return;
            Texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, true) { name = "I-TGT Topography", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 2 };
            Texture.SetPixels32(pixels);
            Texture.Apply(true, true);
            pixels = null; heights = null; ground = null;
        }

        public void Dispose()
        {
            if (reliefLayer) { reliefLayer.enabled = false; Object.Destroy(reliefLayer.gameObject); }
            if (Texture) Object.Destroy(Texture);
            image = null; original = null; reliefLayer = null; Texture = null;
            heights = null; ground = null; pixels = null;
            row = 0; shadedRow = 0; Size = Vector2.zero;
        }
    }
}
