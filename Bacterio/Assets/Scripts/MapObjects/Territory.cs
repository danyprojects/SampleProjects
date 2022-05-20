using UnityEngine;
using Bacterio.Common;

namespace Bacterio.MapObjects
{
    public class Territory : MonoBehaviour
    {
        public Structure _structure;
        public int _nextDeformMs;
        public int _deformCount;
        public MTRandom _random;

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private Vector3[] _vertices;
        private Vector2[] _uvs;

        public float _deformationDistance = 1;
        public float _affectedRadius = 1;
        public float _deformationValue = 0.5f;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();

            WDebug.Assert(_meshCollider != null, "No collider in territory");
            WDebug.Assert(_meshFilter != null, "No filter in territory");
        }

        public void Configure(Mesh mesh)
        {
            _deformCount = 0;

            //Copy the mesh, otherwise deforming it will change all meshes
            var newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.uv = mesh.uv;
            newMesh.triangles = mesh.triangles;
            newMesh.RecalculateBounds();

            //assign it to the filter and collider
            _meshFilter.mesh = newMesh;
            _meshCollider.sharedMesh = newMesh;

            //copy vertices and uvs to edit later without having to get them
            _vertices = newMesh.vertices;
            _uvs = newMesh.uv;
        }

        public void DeformOutwards()
        {
            var mesh = _meshFilter.sharedMesh;

            //The direction * distance
            Vector3 deformCenter = _random.PointInUnitCircle() * _deformationDistance;
            var radiusPwr = _affectedRadius * _affectedRadius;

            for (int i = 0; i < _vertices.Length; i++)
            {
                if (IsPointInCircle(_vertices[i], ref deformCenter, radiusPwr, out float dist))
                {
                    //The center of the deform is used as a direction. So even if the center is inside the mesh, it will still expand outwards as long as the vertices caught are in the outwards edge.
                    //This also means that if the radius is large enough that it catches all verts, it will just end up moving the circle instead
                    _vertices[i] += deformCenter.normalized * _deformationValue * (radiusPwr - dist);
                    _uvs[i] = new Vector2(_vertices[i].x, _vertices[i].y);
                }
            }

            mesh.vertices = _vertices;
            mesh.uv = _uvs;
            mesh.RecalculateBounds();
            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;

            _deformCount++;
        }

        public void DeformInwards()
        {
            var mesh = _meshFilter.sharedMesh;

            //The direction * distance
            Vector3 deformCenter = _random.PointInUnitCircle() * _deformationDistance;
            var radiusPwr = _affectedRadius * _affectedRadius;

            for (int i = 0; i < _vertices.Length; i++)
            {
                if (IsPointInCircle(_vertices[i], ref deformCenter, radiusPwr, out float dist))
                {
                    _vertices[i] += -deformCenter.normalized * _deformationValue * (radiusPwr - dist);
                    _uvs[i] = new Vector2(_vertices[i].x, _vertices[i].y);
                }
            }

            mesh.vertices = _vertices;
            mesh.uv = _uvs;
            mesh.RecalculateBounds();
            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = mesh;
        }

        private bool IsPointInCircle(Vector3 point, ref Vector3 center, float radiusPwr, out float dist)
        {
            point.x -= center.x;
            point.y -= center.y;

            dist = point.magnitude;

            return dist <= radiusPwr;
        }
    }
}
