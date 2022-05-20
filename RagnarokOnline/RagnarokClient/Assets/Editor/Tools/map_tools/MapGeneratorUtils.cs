using RO.Common;
using RO.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public static class MapGeneratorUtils
    {
        public static void ValidateSelection(ref string gndPath, ref string rswPath, ref string gatPath)
        {
            if (Selection.objects.Length > 3)
                throw new Exception("Too many objects selected");

            bool hasGnd = false, hasRsw = false, hasGat = false;

            foreach (UnityEngine.Object obj in Selection.objects)
                if (AssetDatabase.GetAssetPath(obj).Contains(".gnd"))
                {
                    hasGnd = true;
                    gndPath = AssetDatabase.GetAssetPath(obj);
                }
                else if (AssetDatabase.GetAssetPath(obj).Contains(".rsw"))
                {
                    hasRsw = true;
                    rswPath = AssetDatabase.GetAssetPath(obj);
                }
                else if (AssetDatabase.GetAssetPath(obj).Contains(".gat"))
                {
                    hasGat = true;
                    gatPath = AssetDatabase.GetAssetPath(obj);
                }

            if (!hasGnd || !hasRsw || !hasGat)
                throw new Exception("Select map gnd, rsw and gat");

        }

        public static Texture2D GenerateLargeLightmapTexture(Gnd gnd, ref Gnd.Lightmap[] lightmaps)
        {
            float sqr = (float)Math.Sqrt(lightmaps.Length);
            int height = (int)Mathf.Ceil(sqr) * 8;
            int width = height;

            Texture2D tex = new Texture2D(width, height);

            Debug.Log("Lightmap size: " + lightmaps.Length);
            Debug.Log("Texture dimensions: " + width + ", " + height);

            int x = 0;
            int y = 0;
            for (int i = 0; i < lightmaps.Length; i++)
            {
                var lightmap = lightmaps[i];

                //Start at pixel x, y and write 8x8 colors
                tex.SetPixels32(x, y, 8, 8, lightmap.colors);

                //increment x pixel coordinate
                x += 8;
                //Check if we reached end of x and move up a y
                if (x >= width)
                {
                    x = 0;
                    y += 8;

                    if (y >= height)
                        throw new Exception("Too many lightmaps, shouldn't happen");
                }
            }

            string lightmapPath = Path.Combine(MapGenerator.MapFileFolder, MapGenerator.FileNameNoExt + "_lm.png");

            File.WriteAllBytes(lightmapPath, tex.EncodeToPNG());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(lightmapPath);
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Bilinear;

            var settings = importer.GetDefaultPlatformTextureSettings();
            settings.format = TextureImporterFormat.RGBA32;
            settings.crunchedCompression = false;
            settings.overridden = false;
            settings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
            importer.ClearPlatformTextureSettings("Standalone");
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();

            AssetImporter.GetAtPath(lightmapPath).SetAssetBundleNameAndVariant("maps/misc", "");

            return AssetDatabase.LoadAssetAtPath<Texture2D>(lightmapPath);
        }

        public static void LoadWaterTextures(out Texture2D[,] waterTex)
        {
            const string waterPath = "Assets/~Resources/GRF/Textures/Water";
            const int WATER_TYPES = 10;
            const int TEX_PER_WATER = 32;

            waterTex = new Texture2D[WATER_TYPES, TEX_PER_WATER];

            string[] waterFiles = Directory.GetFiles(waterPath);

            foreach (string waterFile in Directory.GetFiles(waterPath))
            {
                if (waterFile.Contains(".meta"))
                    continue;

                string waterName = Path.GetFileNameWithoutExtension(waterFile);

                string waterNumber = waterName.Substring(waterName.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));

                int waterNum = Int32.Parse(waterNumber.Substring(0, 1));
                int waterPart = Int32.Parse(waterNumber.Substring(1));

                waterTex[waterNum, waterPart] = AssetDatabase.LoadAssetAtPath<Texture2D>(waterFile);
            }
        }

        public static Vector2[] GetLightmapUvCoordinates(int lightmapIndex, Texture2D lightmap)
        {
            Vector2[] uvs = new Vector2[2];

            float texelX = 1f / lightmap.width;
            float texelY = 1f / lightmap.height;

            //Gets the index lightmapIndex in the texture
            int x = lightmapIndex % (lightmap.width / 8); // lightmap width should always be a multiple of 8
            int y = lightmapIndex / (lightmap.width / 8);

            //goes to the right pixel
            x *= 8;
            y *= 8;

            //lower left
            uvs[0].x = x / (float)lightmap.width + texelX; //gets the uv coordinate of the pixel x coord ignoring borders
            uvs[0].y = y / (float)lightmap.height + texelY; // gets the uv coordinate of the pixel y coord ignoring borders

            //top left
            uvs[1].x = (x + 8) / (float)lightmap.width - texelX; //ignores borders
            uvs[1].y = (y + 8) / (float)lightmap.height - texelY; //ignores borders

            return uvs;
        }

        public static Material[] GetMapTextureMaterials(ref Gnd.Texture[] gndTextures)
        {
            string[] files = new string[gndTextures.Length];

            int i = 0;
            foreach (var tex in gndTextures)
                files[i++] = tex.file;

            return GetMeshTextureMaterials(ref files, "Custom/MapShader");

            /* Material[] materials = new Material[gndTextures.Length];
             const string textureFolder = "Assets/~Resources/GRF/Textures";
             const string materialFolder = "Assets/~Resources/GRF/Textures/MapMaterials";
             const string materialShader = "Custom/MapShader";

             //Create texture materials
             for (int i = 0; i < gndTextures.Length; i++)
             {
                 string fileName = Path.GetFileNameWithoutExtension(gndTextures[i].file);
                 string fileExtension = Path.GetExtension(gndTextures[i].file);

                 //In case directory doesnt exist yet
                 Directory.CreateDirectory(Path.Combine(materialFolder, Path.GetDirectoryName(gndTextures[i].file)));
                 string materialPath = Path.Combine(materialFolder, Path.GetDirectoryName(gndTextures[i].file), fileName + ".mat");                

                 //Try to load material
                 materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                 //If material is already there then skip
                 if (materials[i] != null)
                     continue;

                 //otherwise create material
                 //Get texture and set it's bundle in case
                 string texturePath = Path.Combine(textureFolder, gndTextures[i].file);
                 Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                 AssetImporter.GetAtPath(texturePath).SetAssetBundleNameAndVariant("maps/textures", "");

                 //Create material instance
                 materials[i] = new Material(Shader.Find(materialShader));
                 materials[i].SetTexture("_MainTex", texture);

                 //Create material asset and save it
                 AssetDatabase.CreateAsset(materials[i], materialPath);
                 AssetDatabase.SaveAssets();
                 AssetDatabase.Refresh();

                 //get material
                 materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                 AssetImporter.GetAtPath(materialPath).SetAssetBundleNameAndVariant("maps/textures", "");
             }
             return materials;*/
        }

        public static Material[] GetMeshTextureMaterials(ref string[] textures, string materialShader = "Custom/Map Object Shader")
        {
            Material[] materials = new Material[textures.Length];
            const string textureFolder = "Assets/~Resources/GRF/Textures";
            const string materialFolder = "Assets/~Resources/GRF/Textures/MapMaterials";

            //Create texture materials
            for (int i = 0; i < textures.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(textures[i]);
                string fileExtension = Path.GetExtension(textures[i]);

                //In case directory doesnt exist yet
                Directory.CreateDirectory(Path.Combine(materialFolder, Path.GetDirectoryName(textures[i])));
                string materialPath = Path.Combine(materialFolder, Path.GetDirectoryName(textures[i]), fileName + ".mat");

                //Try to load material
                materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                //If material is already there then skip
                if (materials[i] != null)
                {
                    RemovePinkFromTexture((Texture2D)materials[i].mainTexture);
                    continue;
                }

                //otherwise create material
                //Get texture and set it's bundle in case
                string texturePath = Path.Combine(textureFolder, textures[i]);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                RemovePinkFromTexture(texture);
                AssetImporter.GetAtPath(texturePath).SetAssetBundleNameAndVariant("maps/textures", "");

                //Create material instance
                materials[i] = new Material(Shader.Find(materialShader));
                materials[i].SetTexture("_MainTex", texture);

                //Create material asset and save it
                AssetDatabase.CreateAsset(materials[i], materialPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //get material
                materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                AssetImporter.GetAtPath(materialPath).SetAssetBundleNameAndVariant("maps/textures", "");
            }
            return materials;
        }

        public static void RemovePinkFromTexture(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.isReadable = true;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            var pink = new Color(1, 0, 1);
            var alpha = new Color(0, 0, 0, 0);
            bool changed = false;
            for (int y = 0; y < texture.height; y++)
                for (int x = 0; x < texture.width; x++)
                {
                    var color = texture.GetPixel(x, y);
                    if (color.r != pink.r || color.g != pink.g || color.b != pink.b)
                        continue;
                    texture.SetPixel(x, y, alpha);
                    changed = true;
                }

            if (changed)
            {
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            importer = (TextureImporter)TextureImporter.GetAtPath(path);
            importer.isReadable = false;
            importer.textureType = TextureImporterType.Default;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.SaveAndReimport();
        }

        //Used by generate rsm meshes
        private static Bounds Bounds;

        private static Mesh GenerateRsmMesh(Rsm.Mesh rsmMesh)
        {
            List<int>[] triangles = new List<int>[rsmMesh.textures.Length]; // for submeshes
            for (int i = 0; i < triangles.Length; i++)
                triangles[i] = new List<int>();

            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> vertices = new List<Vector3>();

            var rsmVerts = rsmMesh.vertices;

            //object matrix stuff
            var mat = Matrix4x4.identity * Matrix4x4.Translate(new Vector3(-Bounds.center.x, -Bounds.max.y, -Bounds.center.z));
            mat *= rsmMesh.transMatrix; //has position, rotation and scaling
            if (!rsmMesh.isOnly)
                mat *= Matrix4x4.Translate(new Vector3(rsmMesh.offset.x, -rsmMesh.offset.y, rsmMesh.offset.z));
            mat *= rsmMesh.axis;

            //Generate all faces
            foreach (var face in rsmMesh.faces)
            {
                triangles[face.texIndex].AddRange(new int[] { vertices.Count + 2, vertices.Count + 1, vertices.Count + 0 });

                var vert1 = rsmMesh.vertices[face.vertices[0]];
                var vert2 = rsmMesh.vertices[face.vertices[1]];
                var vert3 = rsmMesh.vertices[face.vertices[2]];

                vert1 = mat.MultiplyPoint(vert1);
                vert2 = mat.MultiplyPoint(vert2);
                vert3 = mat.MultiplyPoint(vert3);

                vertices.AddRange(new Vector3[] { vert1, vert2, vert3 });

                var faceUvs = new Vector2[]
                {
                    rsmMesh.texCoords[face.texvertices[0]],
                    rsmMesh.texCoords[face.texvertices[1]],
                    rsmMesh.texCoords[face.texvertices[2]]
                };

                //invert y cuz of RO
                faceUvs[0].y = 1 - faceUvs[0].y;
                faceUvs[1].y = 1 - faceUvs[1].y;
                faceUvs[2].y = 1 - faceUvs[2].y;

                uvs.AddRange(faceUvs);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.subMeshCount = rsmMesh.textures.Length;
            for (int i = 0; i < rsmMesh.textures.Length; i++)
                mesh.SetTriangles(triangles[i].ToArray(), i);
            mesh.uv = uvs.ToArray();

            return mesh;
        }

        public static void GenerateMeshChildren(Rsm.Mesh rsmMesh, List<ModelData.MeshData> meshDatas, List<ModelData.ModelAnimation> modelAnims, int parent = -1)
        {
            if (parent == -1)
                Bounds = rsmMesh.bounds;

            //if this was called, there's at least 1 mesh to generate
            var mesh = GenerateRsmMesh(rsmMesh);
            var meshData = new ModelData.MeshData
            {
                vertices = mesh.vertices,
                uvs = mesh.uv,
                parent = parent
            };

            meshData.submeshes = new ModelData.MeshData.Submesh[mesh.subMeshCount];
            for (int i = 0; i < mesh.subMeshCount; i++)
                meshData.submeshes[i].triangles = mesh.GetTriangles(i);
            meshDatas.Add(meshData);

            //Fill the animation data if it has any
            if (rsmMesh.posKeyFrames != null || rsmMesh.rotKeyFrames != null)
            {
                var modelAnim = new ModelData.ModelAnimation();

                //Only add position keyframes if theres any
                if (rsmMesh.posKeyFrames != null)
                {
                    modelAnim.posAnim = new ModelData.ModelAnimation.PositionAnimation[rsmMesh.posKeyFrames.Length];
                    for (int i = 0; i < rsmMesh.posKeyFrames.Length; i++)
                        modelAnim.posAnim[i] = new ModelData.ModelAnimation.PositionAnimation
                        {
                            position = rsmMesh.posKeyFrames[i].position,
                            updateTime = rsmMesh.posKeyFrames[i].frame * Constants.FPS_TIME_INTERVAL
                        };
                }

                //Only add rotation keyframes if theres any
                if (rsmMesh.rotKeyFrames != null)
                {
                    modelAnim.rotAnim = new ModelData.ModelAnimation.RotationAnimation[rsmMesh.rotKeyFrames.Length];
                    for (int i = 0; i < rsmMesh.rotKeyFrames.Length; i++)
                    {
                        modelAnim.rotAnim[i] = new ModelData.ModelAnimation.RotationAnimation
                        {
                            quaternion = rsmMesh.rotKeyFrames[i].rotation,
                            updateTime = rsmMesh.rotKeyFrames[i].frame * Constants.FPS_TIME_INTERVAL
                        };
                        //modelAnim.rotAnim[i].quaternion.w = -modelAnim.rotAnim[i].quaternion.w; //Quaternion is inverted from RO
                    }
                }

                modelAnim.targetMesh = meshDatas.Count - 1; // current mesh index
                modelAnims.Add(modelAnim);
            }

            //the index of current mesh for its children to use as parent
            int currentIndex = meshDatas.Count - 1;

            //If it has children then generate all of them and their own children too
            for (int i = 0; i < rsmMesh.children.Count; i++)
                GenerateMeshChildren(rsmMesh.children[i], meshDatas, modelAnims, currentIndex);
        }

        public static Vector3[] CalcNormals(ref Vector3[] vertices)
        {
            Vector3[] norms = new Vector3[4];
            Vector3 norm1 = CalcNormal(vertices[0], vertices[3], vertices[1]);
            Vector3 norm2 = CalcNormal(vertices[0], vertices[2], vertices[3]);

            norm1.y *= -1;
            norm2.y *= -1;
            norms[0] = (norm1 + norm2).normalized;
            norms[1] = norm1;
            norms[2] = norm2;
            norms[3] = (norm1 + norm2).normalized;

            return norms;
        }

        private static Vector3 CalcNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            // Find vectors corresponding to two of the sides of the triangle.
            Vector3 side1 = b - a;
            Vector3 side2 = c - a;

            // Cross the vectors to get a perpendicular vector, then normalize it.
            return Vector3.Cross(side2, side1).normalized;
        }
    }
}
