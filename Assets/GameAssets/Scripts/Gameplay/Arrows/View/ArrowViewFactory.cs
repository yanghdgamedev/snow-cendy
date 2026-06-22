using HDG.Gameplay.Arrows.Controller;
using HDG.Gameplay.Arrows.Model;
using UnityEngine;

namespace HDG.Gameplay.Arrows.View
{
    /// <summary>
    /// Factory for creating arrow views. Either uses an assigned prefab or
    /// builds one programmatically (useful for the auto-setup test scene).
    /// </summary>
    public class ArrowViewFactory : MonoBehaviour
    {
        [SerializeField] private LineRendererArrowView _arrowPrefab;
        [SerializeField] private float _arrowWidth = 0.5f;
        [SerializeField] private LayerMask _arrowLayer = 0;
        [SerializeField] private Sprite _headSprite;
        [SerializeField] private Material _arrowMaterial;

        private int _resolvedLayer = -1;

        private void Awake()
        {
            // Resolve layer once
            if (_arrowLayer.value == 0) _resolvedLayer = LayerMask.NameToLayer("Default");
            else
            {
                // Find first set bit
                int bits = _arrowLayer.value;
                for (int i = 0; i < 32; i++)
                {
                    if ((bits & (1 << i)) != 0) { _resolvedLayer = i; break; }
                }
            }
        }

        public IArrowView CreateView(Transform container, Arrow arrow,
                                     IDotsController dotsController,
                                     IArrowsController arrowsController)
        {
            LineRendererArrowView view;
            if (_arrowPrefab != null)
            {
                view = Instantiate(_arrowPrefab, container);
            }
            else
            {
                view = BuildProgrammaticView(container);
            }

            view.gameObject.layer = _resolvedLayer < 0 ? container.gameObject.layer : _resolvedLayer;
            view.Bind(dotsController, arrowsController);
            view.Setup(arrow);
            return view;
        }

        private LineRendererArrowView BuildProgrammaticView(Transform container)
        {
            // Root with LineRenderer + MeshCollider + ArrowMeshHandler + LineRendererArrowView
            var root = new GameObject("Arrow");
            root.transform.SetParent(container, worldPositionStays: false);

            var line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.startWidth = _arrowWidth;
            line.endWidth = _arrowWidth;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;
            line.alignment = LineAlignment.View;
            line.material = _arrowMaterial != null
                ? _arrowMaterial
                : new Material(Shader.Find("Sprites/Default"));
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            root.AddComponent<MeshCollider>();
            var meshHandler = root.AddComponent<ArrowMeshHandler>();

            // Head child
            var headGo = new GameObject("Head");
            headGo.transform.SetParent(root.transform, worldPositionStays: false);
            var headRenderer = headGo.AddComponent<SpriteRenderer>();
            headRenderer.sprite = _headSprite != null ? _headSprite : BuildTriangleSprite();
            headRenderer.sortingOrder = 10;

            // Rotate head so the sprite "up" aligns with our Direction.Up convention.
            // Sprite is created with point upward (+Y in 2D = +Z in 3D when camera looks down).
            headGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            headGo.transform.localScale = Vector3.one * _arrowWidth * 1.5f;

            var view = root.AddComponent<LineRendererArrowView>();
            // Use reflection-free assignment via SerializedField is impossible at runtime;
            // we expose Bind/Setup which fetch refs from GetComponent in Awake-like fashion.
            // Simplest: assign via [SerializeField] reflection or via SetReferences method.
            view.SetReferences(line, headRenderer, headGo.transform, meshHandler);

            return view;
        }

        private static Sprite _cachedTriangleSprite;
        private static Sprite BuildTriangleSprite()
        {
            if (_cachedTriangleSprite != null) return _cachedTriangleSprite;
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            // Draw an upward-pointing triangle
            for (int y = 0; y < size; y++)
            {
                int half = (size - y) / 2;
                for (int x = 0; x < size; x++)
                {
                    bool inside = x >= half && x < size - half && y < size;
                    tex.SetPixel(x, y, inside ? Color.white : new Color(0, 0, 0, 0));
                }
            }
            tex.Apply();
            _cachedTriangleSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.3f), 32f);
            return _cachedTriangleSprite;
        }
    }
}
