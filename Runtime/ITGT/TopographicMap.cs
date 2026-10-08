using UnityEngine;
using UnityEngine.UI;

namespace PhantasmsArsenal.ITGT
{
    // Coordinates are stored in Datum space. Sampling always converts back to current local space.
    internal sealed class TopographicMap
    {
        const int Resolution = 2048;
        static readonly System.Reflection.FieldInfo MapDimensions = HarmonyLib.AccessTools.Field(typeof(DynamicMap), "mapDimensions");
        readonly float[] heights = new float[Resolution * Resolution];
        readonly bool[] ground = new bool[Resolution * Resolution];
        Image image;
        Sprite original, generated;
        Color originalColor;
        Color32[] basePixels, pixels;
        readonly System.Diagnostics.Stopwatch frameBudget = new System.Diagnostics.Stopwatch();
        int row, shadedRow;
        bool attached;
        public Texture2D Texture { get; private set; }
        public Vector2 Size { get; private set; }
        public bool Ready => Texture;
        public float Progress => (row * .75f + shadedRow * .25f) / Resolution;

        public void Update(DynamicMap map, bool replace)
        {
            var next = map && map.mapImage ? map.mapImage.GetComponent<Image>() : null;
            var dimensions = next ? (Vector2)MapDimensions.GetValue(map) : Vector2.zero;
            if (!next || (original && image != next) || (image == next && next.sprite != original && next.sprite != generated) ||
                (original && Size != dimensions))
                Dispose();
            if (!next || !next.sprite) return;
            if (!image)
            {
                image = next;
                original = image.sprite;
                originalColor = image.color;
                Size = dimensions;
                if (Size.x <= 0 || Size.y <= 0) { Dispose(); return; }
                CopyBaseImage();
                row = 0;
            }
            // Bound both terrain sampling and relief shading; larger maps build across frames.
            if (!Texture)
            {
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
            if (Texture && replace && !attached)
            {
                image.sprite = generated;
                image.color = Color.white;
                attached = true;
            }
            else if (!replace && attached)
            {
                image.sprite = original;
                image.color = originalColor;
                attached = false;
            }
        }

        void CopyBaseImage()
        {
            // The native image is often not CPU-readable. Read it through a render target.
            var rt = RenderTexture.GetTemporary(Resolution, Resolution, 0);
            var previous = RenderTexture.active;
            Texture2D copy = null;
            try
            {
                var source = original.textureRect;
                Graphics.Blit(original.texture, rt, new Vector2(source.width / original.texture.width, source.height / original.texture.height),
                    new Vector2(source.x / original.texture.width, source.y / original.texture.height));
                RenderTexture.active = rt;
                copy = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, Resolution, Resolution), 0, 0);
                copy.Apply();
                basePixels = copy.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                if (copy) Object.Destroy(copy);
            }
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
                    // Retain native cartographic detail as a faint layer beneath the contour lines.
                    if (basePixels != null) color *= Mathf.Lerp(.85f, 1.03f, ((Color)basePixels[i]).grayscale);
                    color.a = 1;
                    pixels[i] = color;
                }
            }
            if (shadedRow < Resolution) return;
            Texture = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, true) { name = "I-TGT Topography", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 2 };
            Texture.SetPixels32(pixels);
            Texture.Apply(true, true);
            generated = Sprite.Create(Texture, new Rect(0, 0, Resolution, Resolution), Vector2.one * .5f, original.pixelsPerUnit);
            pixels = null; basePixels = null;
        }

        public void Dispose()
        {
            if (image && attached && image.sprite == generated) { image.sprite = original; image.color = originalColor; }
            if (generated) Object.Destroy(generated);
            if (Texture) Object.Destroy(Texture);
            image = null; original = null; generated = null; Texture = null; basePixels = null; pixels = null;
            attached = false; row = 0; shadedRow = 0; Size = Vector2.zero;
        }
    }
}
