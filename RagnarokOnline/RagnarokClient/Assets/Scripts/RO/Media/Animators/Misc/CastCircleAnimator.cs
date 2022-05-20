using RO.Common;
using RO.Containers;
using UnityEngine;


namespace RO.Media
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class CastCircleAnimator : MonoBehaviour
    {
        private MeshRenderer _meshRenderer = null;

        public void ProjectMesh(GroundProjectedMesh groundMesh, in Vector2Int center, in Map map)
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter.mesh;

            int index = 0;
            //start getting vertices at bottom left
            for (int y = center.y - groundMesh.min; y <= center.y + groundMesh.max; y++)
            {
                for (int x = center.x - groundMesh.min; x <= center.x + groundMesh.max; x++)
                {
                    map.GetTileVerticesHeights(x, y, ref groundMesh.vertices, index);
                    index += 4; // each call to get tile vertices inserts 4 vertices
                }
            }

            mesh.vertices = groundMesh.vertices;
            mesh.uv = groundMesh.mesh.uv;
            mesh.triangles = groundMesh.mesh.triangles;

            meshFilter.mesh = mesh;
            _meshRenderer.material = Materials.castCircleMaterial;
        }

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            gameObject.GetComponent<MeshFilter>().mesh = new Mesh();
        }

        private void OnDisable()
        {
            _meshRenderer.enabled = false;
        }

        private void OnEnable()
        {
            _meshRenderer.enabled = true;
        }
    }
}
