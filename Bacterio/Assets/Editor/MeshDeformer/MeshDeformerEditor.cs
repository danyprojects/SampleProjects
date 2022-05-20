using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Bacterio;

namespace BacterioEditor
{
    [CustomEditor(typeof(MeshDeformer))]
    public class MeshObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var meshObj = (MeshDeformer)target;

            if (GUILayout.Button("Generate Mesh"))
                meshObj.GenerateMesh();

            if (GUILayout.Button("Deform Mesh Outwards"))
                meshObj.DeformMeshOutwards();
            if (GUILayout.Button("Deform Mesh Inwards"))
                meshObj.DeformMeshInwards();

            if (GUILayout.Button("Save Mesh"))
                SaveMesh();
        }

        private void SaveMesh()
        {
            var obj = (MeshDeformer)target;
            var origMesh = obj.GetComponent<MeshFilter>().sharedMesh;

            var structureDb = DatabaseGenerator.GetStructures();

            var path = EditorStrings.MESH_PATH;
            int count = 0;

            while (true)
            {
                if (count < structureDb.Meshes.Count && File.Exists(Path.Combine(path, structureDb.Meshes[count] + ".mesh")))
                {
                    count++;
                    continue;
                }
                //If we get here either then we got our count
                break;
            }

            //update db
            var name = "Deformed_";
            structureDb.Meshes.Add(name + count.ToString());
            DatabaseGenerator.SaveJson(structureDb.JsonName, structureDb);

            //Then create and save mesh
            path = Path.Combine(path, name + count.ToString() + ".mesh");

            var mesh = new Mesh();
            mesh.vertices = origMesh.vertices;
            mesh.triangles = origMesh.triangles;
            mesh.uv = origMesh.uv;
            mesh.RecalculateBounds();

            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetImporter.GetAtPath(path).assetBundleName = "misc";

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}