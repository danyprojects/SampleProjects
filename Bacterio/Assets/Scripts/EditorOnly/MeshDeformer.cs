using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BacterioEditor
{
#if UNITY_EDITOR
    public class MeshDeformer : MonoBehaviour
    {
        public MeshFilter _meshFilter;
        public int totalVertices = 10;
        public float deformationDistance = 1; //How far the circle that will apply the deformation can be from the center of the mesh
        public float deformationValue = 0.3f; //The deformation rate that we apply to each affected vertex.
        public float affectedRadius = 1; //This is how large the circle that will check for affected vertices is

        private Stack<Vector3> _outwardPositions = new Stack<Vector3>();

        public void GenerateMesh()
        {
            var mesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i < totalVertices; i++)
            {
                float a = (float)i / totalVertices;
                vertices.Add(new Vector3(Mathf.Sin(a * Mathf.PI * 2), Mathf.Cos(a * Mathf.PI * 2), 0));
                uvs.Add(new Vector2(vertices[i].x, vertices[i].y));
            }

            List<int> triangles = new List<int>();

            for (int i = 2; i < totalVertices; i++)
            {
                triangles.Add(0); //always start triangle at first vertex
                triangles.Add(i);
                triangles.Add(i - 1);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            _meshFilter.mesh = mesh;
        }

        public void DeformMeshOutwards()
        {
            var mesh = _meshFilter.sharedMesh;

            //The direction * distance
            Vector3 deformCenter = Random.insideUnitCircle * deformationDistance;
            _outwardPositions.Push(deformCenter);
            var radiusPwr = affectedRadius * affectedRadius;

            var verts = new List<Vector3>(mesh.vertices);
            var uvs = new List<Vector2>(mesh.uv);

            for (int i = 0; i < verts.Count; i++) 
            {
                if (IsPointInCircle(verts[i], deformCenter, radiusPwr, out float dist))
                {
                    verts[i] += deformCenter.normalized * deformationValue * (radiusPwr - dist);
                    uvs[i] = new Vector2(verts[i].x, verts[i].y);
                }
            }

            mesh.vertices = verts.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateBounds();
            _meshFilter.mesh = mesh;
        }

        public void DeformMeshInwards()
        {
            var mesh = _meshFilter.sharedMesh;

            //The direction * distance
            Vector3 deformCenter = _outwardPositions.Pop();
            var radiusPwr = affectedRadius * affectedRadius;

            var verts = new List<Vector3>(mesh.vertices);
            var uvs = new List<Vector2>(mesh.uv);

            for (int i = 0; i < verts.Count; i++)
            {
                if (IsPointInCircle(verts[i], deformCenter, radiusPwr, out float dist))
                {
                    verts[i] += -deformCenter.normalized * deformationValue * (radiusPwr - dist);
                    uvs[i] = new Vector2(verts[i].x, verts[i].y);
                }
            }

            mesh.vertices = verts.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateBounds();
            _meshFilter.mesh = mesh;
        }

        private bool IsPointInCircle(Vector3 point, Vector3 center, float radiusPwr, out float dist)
        {
            point.x -= center.x;
            point.y -= center.y;

            dist = point.magnitude;

            return dist <= radiusPwr;
        }
    }
#endif
}