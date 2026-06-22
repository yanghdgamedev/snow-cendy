using UnityEngine;

namespace HDG.Gameplay.Arrows.View
{
    /// <summary>
    /// Keeps a MeshCollider in sync with a UnityEngine.LineRenderer mesh,
    /// so Physics.Raycast picks the actual arrow shape.
    /// Bake is on demand (call RefreshCollider) — not every frame.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    public class ArrowMeshHandler : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        private MeshCollider _meshCollider;
        private Mesh _bakedMesh;

        private void Awake()
        {
            _meshCollider = GetComponent<MeshCollider>();
            _meshCollider.convex = false;
            _meshCollider.isTrigger = false;
            _bakedMesh = new Mesh { name = "ArrowBakedMesh" };
        }

        public void Bind(LineRenderer lineRenderer)
        {
            _lineRenderer = lineRenderer;
        }

        public void RefreshCollider(Camera cam)
        {
            if (_lineRenderer == null) return;
            _bakedMesh.Clear();
            _lineRenderer.BakeMesh(_bakedMesh, cam, useTransform: true);
            _meshCollider.sharedMesh = null; // force reassign
            _meshCollider.sharedMesh = _bakedMesh;
        }

        public void DisableCollider() => _meshCollider.enabled = false;
        public void EnableCollider() => _meshCollider.enabled = true;

        private void OnDestroy()
        {
            if (_bakedMesh != null) Destroy(_bakedMesh);
        }
    }
}
