using RO.Common;
using RO.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTools
{
    public class Other
    {

        [MenuItem("Assets/Extra/Miscellaneous/Set sorting layer to mesh prefab")]
        private static void SetSortingLayerToRenderer()
        {
            if (Selection.objects.Length != 1)
                throw new Exception("Please select exactly 1 object");
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(Selection.objects[0]));
            MeshRenderer mshRenderer = obj.GetComponent<MeshRenderer>();
            if (mshRenderer == null)
                throw new Exception("Object does not contain a mesh renderer component");
            mshRenderer.sortingLayerID = SortingLayers.BlockInt;
            mshRenderer.sortingLayerName = SortingLayers.BlockStr;
            mshRenderer.sortingOrder = 0;

            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Extra/Miscellaneous/Format effect bmps")]
        private static void FormatEffectBMPs()
        {
            if (Selection.objects.Length <= 0)
            {
                Debug.Log("Please select some bmps");
                return;
            }

            foreach (var obj in Selection.objects)
            {
                string objPath = AssetDatabase.GetAssetPath(obj);

                if (!objPath.Contains(".bmp"))
                    continue;

                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(objPath);
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = SpriteGenerator.PIXELS_PER_UNIT;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Point;

                var settings = importer.GetDefaultPlatformTextureSettings();
                settings.format = TextureImporterFormat.RGB24;
                settings.crunchedCompression = false;
                settings.overridden = false;
                settings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                importer.ClearPlatformTextureSettings("Standalone");
                importer.SetPlatformTextureSettings(settings);
                //importer.SetPlatformTextureSettings("Standalone", 2048, TextureImporterFormat.RGB24);
                importer.SaveAndReimport();

                GeneratePngFromBmp(objPath);
            }
        }


        [MenuItem("Assets/Extra/Miscellaneous/Format Item bmps")]
        private static void FormatItemBMPs()
        {
            if (Selection.objects.Length <= 0)
            {
                Debug.Log("Please select some bmps");
                return;
            }

            foreach (var obj in Selection.objects)
            {
                string objPath = AssetDatabase.GetAssetPath(obj);

                if (!objPath.Contains(".bmp"))
                    continue;

                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(objPath);
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = SpriteGenerator.PIXELS_PER_UNIT;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;

                var settings = importer.GetDefaultPlatformTextureSettings();
                settings.format = TextureImporterFormat.RGB24;
                settings.crunchedCompression = true;
                settings.overridden = true;
                settings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
                importer.ClearPlatformTextureSettings("Standalone");
                importer.SetPlatformTextureSettings(settings);
                importer.SaveAndReimport();

                AssetImporter.GetAtPath(objPath).SetAssetBundleNameAndVariant("sprites/items", "");
            }
        }


        private static void GeneratePngFromBmp(string fileName)
        {
            string pngPath = "Assets/~Resources/GRF/Sprites/Effects/pngs/" + Path.GetFileNameWithoutExtension(fileName) + ".png";

            AssetDatabase.DeleteAsset(pngPath);
            AssetDatabase.Refresh();

            //Make original readable
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(fileName);
            importer.isReadable = true;
            importer.SaveAndReimport();

            //Create png from original
            var original = AssetDatabase.LoadAssetAtPath<Texture2D>(fileName);
            var format = original.format == TextureFormat.RGB24 ? TextureFormat.RGBA32 : original.format;
            var final = new Texture2D(original.width, original.height, format, false);
            Color[] colors = original.GetPixels();
            int next = 0;
            for (int y = 0; y < original.height; y++)
                for (int x = 0; x < original.width; x++, next++)
                    final.SetPixel(x, y, GetColorAlpha(colors[y * original.width + x]));

            //make original non readable again
            importer.isReadable = false;
            importer.SaveAndReimport();

            //Save Png
            File.WriteAllBytes(pngPath, ImageConversion.EncodeToPNG(final));
            AssetDatabase.Refresh();
            importer = (TextureImporter)TextureImporter.GetAtPath(pngPath);
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.spritePixelsPerUnit = Constants.PIXELS_PER_UNIT;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = false;
            importer.SaveAndReimport();

            //Set the act file and prefab to the asset bundle
            AssetImporter.GetAtPath(pngPath).SetAssetBundleNameAndVariant("sprites/effects", "");
        }

        private static Color GetColorAlpha(Color color)
        {
            Color _color = new Color(color.r, color.g, color.b, color.a);
            //Remove pink
            if (color.r >= 253 / 255f && color.b >= 253 / 255f && color.g <= 3 / 255f)
                return new Color(0, 0, 0, 0);
            return _color;
        }

        //[MenuItem("Assets/Extra/Miscellaneous/Resize image")]
        private static void ResizeImage()
        {
            Texture2D source = Selection.objects[0] as Texture2D;

            RenderTexture rt = RenderTexture.GetTemporary((int)(source.width * 0.7f), (int)(source.height * 0.7f));
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);
            Texture2D nTex = new Texture2D((int)(source.width * 0.7f), (int)(source.height * 0.7f));
            nTex.ReadPixels(new Rect(0, 0, (int)(source.width * 0.7f), (int)(source.height * 0.7f)), 0, 0);
            nTex.Apply();
            RenderTexture.active = null;

            string path = AssetDatabase.GetAssetPath(source);
            File.WriteAllBytes(path + "_a.png", nTex.EncodeToPNG());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Extra/Miscellaneous/Generate Projection Meshes")]
        public static void GenerateProjectionMeshes()
        {
            string path = "Assets/~Resources/Misc/ground_meshes/";

            EditorScriptsUtility.ClearFolder(path, null);
            for (int size = 1; size < 12; size++)
            {
                GroundProjectedMesh groundMesh = ScriptableObject.CreateInstance<GroundProjectedMesh>();

                groundMesh.mesh = new Mesh();
                groundMesh.vertices = new Vector3[size * size * 4]; // square, * vertices per square

                if (size % 2 == 0)
                {
                    groundMesh.min = size / 2;
                    groundMesh.max = groundMesh.min - 1;
                }
                else
                {
                    groundMesh.min = size / 2;
                    groundMesh.max = groundMesh.min;
                }

                Vector2[] uvs = new Vector2[size * size * 4];

                //iterate all squares and set vertices and uvs
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        uvs[(y * size + x) * 4 + 0] = new Vector2(x / (float)size, y / (float)size); // bottom left
                        uvs[(y * size + x) * 4 + 1] = new Vector2(x / (float)size, (y + 1) / (float)size); // top left
                        uvs[(y * size + x) * 4 + 2] = new Vector2((x + 1) / (float)size, (y + 1) / (float)size); // top right
                        uvs[(y * size + x) * 4 + 3] = new Vector2((x + 1) / (float)size, y / (float)size); // bottom right

                        groundMesh.vertices[(y * size + x) * 4 + 0] = new Vector3(x * Constants.CELL_TO_UNIT_SIZE, 0, y * Constants.CELL_TO_UNIT_SIZE);
                        groundMesh.vertices[(y * size + x) * 4 + 0].x -= (size * Constants.CELL_TO_UNIT_SIZE) / 2f;
                        groundMesh.vertices[(y * size + x) * 4 + 0].z -= (size * Constants.CELL_TO_UNIT_SIZE) / 2f;
                        groundMesh.vertices[(y * size + x) * 4 + 1] = new Vector3(x * Constants.CELL_TO_UNIT_SIZE, 0, (y + 1) * Constants.CELL_TO_UNIT_SIZE);
                        groundMesh.vertices[(y * size + x) * 4 + 1].x -= (size * Constants.CELL_TO_UNIT_SIZE) / 2f;
                        groundMesh.vertices[(y * size + x) * 4 + 1].z -= (size * Constants.CELL_TO_UNIT_SIZE) / 2f;
                        groundMesh.vertices[(y * size + x) * 4 + 2] = new Vector3((x + 1) * Constants.CELL_TO_UNIT_SIZE, 0, (y + 1) * Constants.CELL_TO_UNIT_SIZE);
                        groundMesh.vertices[(y * size + x) * 4 + 2].x -= (size * Constants.CELL_TO_UNIT_SIZE) / 2f;
                        groundMesh.vertices[(y * size + x) * 4 + 2].z -= (size * Constants.CELL_TO_UNIT_SIZE) / 2f;
                        groundMesh.vertices[(y * size + x) * 4 + 3] = new Vector3((x + 1) * Constants.CELL_TO_UNIT_SIZE, 0, y * Constants.CELL_TO_UNIT_SIZE);
                        groundMesh.vertices[(y * size + x) * 4 + 3].x -= (size * Constants.CELL_TO_UNIT_SIZE) / 2f;
                        groundMesh.vertices[(y * size + x) * 4 + 3].z -= (size * Constants.CELL_TO_UNIT_SIZE) / 2f;
                    }
                }

                int[] triangles = new int[size * size * 6]; // square, * triangle indexes per square

                //iterate all squares and set triangles
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        triangles[(y * size + x) * 6 + 0] = (y * size + x) * 4 + 0;
                        triangles[(y * size + x) * 6 + 1] = (y * size + x) * 4 + 1;
                        triangles[(y * size + x) * 6 + 2] = (y * size + x) * 4 + 3;
                        triangles[(y * size + x) * 6 + 3] = (y * size + x) * 4 + 3;
                        triangles[(y * size + x) * 6 + 4] = (y * size + x) * 4 + 1;
                        triangles[(y * size + x) * 6 + 5] = (y * size + x) * 4 + 2;
                    }
                }

                groundMesh.mesh.vertices = groundMesh.vertices;
                groundMesh.mesh.triangles = triangles;
                groundMesh.mesh.uv = uvs;

                AssetDatabase.CreateAsset(groundMesh.mesh, Path.Combine(path, ConstStrings.GROUND_MESH_NAME + size + ".mesh"));
                AssetDatabase.CreateAsset(groundMesh, Path.Combine(path, ConstStrings.GROUND_MESH_DATA_NAME + size + ".asset"));
                //Set the act file and prefab to the asset bundle
                AssetImporter.GetAtPath(Path.Combine(path, ConstStrings.GROUND_MESH_NAME + size + ".mesh")).SetAssetBundleNameAndVariant("etc", "");
                AssetImporter.GetAtPath(Path.Combine(path, ConstStrings.GROUND_MESH_DATA_NAME + size + ".asset")).SetAssetBundleNameAndVariant("etc", "");

                AssetDatabase.Refresh();
            }
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Assets/Extra/Miscellaneous/Generate base cylinder")]
        private static void GenerateBaseCylinder()
        {
            const string path = "Assets/~Resources/GRF/Sprites/Misc/Cylinder";
            const string name = "cylinder.mesh";

            List<Vector3> botVertices = new List<Vector3>(), topVertices = new List<Vector3>();
            List<Vector2> botUvs = new List<Vector2>(), topUvs = new List<Vector2>(); ;

            int vertsPerSide = 20;

            //generate top and bottom vertices
            for (int i = 0; i <= vertsPerSide; i++)
            {
                float a = (i + 0.0f) / vertsPerSide;
                float b = (i + 0.5f) / vertsPerSide;

                botVertices.Add(new Vector3(Mathf.Sin(a * Mathf.PI * 2), 0, Mathf.Cos(a * Mathf.PI * 2)));
                botUvs.Add(new Vector2(Mathf.Clamp(a, 0, 0.99f), 0));
                topVertices.Add(new Vector3(Mathf.Sin(b * Mathf.PI * 2), 1, Mathf.Cos(b * Mathf.PI * 2)));
                topUvs.Add(new Vector2(Mathf.Clamp(b, 0, 0.99f), 0.99f));
            }

            botVertices.Add(botVertices[0]);
            botUvs.Add(botUvs[0]);
            topVertices.Add(topVertices[0]);
            topUvs.Add(topUvs[0]);

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int i = 0; i <= vertsPerSide; i++)
            {
                triangles.Add(vertices.Count + 0);
                triangles.Add(vertices.Count + 3);
                triangles.Add(vertices.Count + 2);
                triangles.Add(vertices.Count + 0);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 3);

                vertices.Add(botVertices[i + 0]); // 0 - lower left
                vertices.Add(botVertices[i + 1]); // 1 - lower right
                vertices.Add(topVertices[i + 0]); // 2 - upper left
                vertices.Add(topVertices[i + 1]); // 3 - uper right

                uvs.Add(botUvs[i + 0]);
                uvs.Add(botUvs[i + 1]);
                uvs.Add(topUvs[i + 0]);
                uvs.Add(topUvs[i + 1]);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            AssetDatabase.CreateAsset(mesh, Path.Combine(path, name));
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Extra/Miscellaneous/Generate circle")]
        private static void GenerateCircle()
        {
            const string path = "Assets/~Resources/GRF/Sprites/Misc/Cylinder";
            const string name = "circle.mesh";

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            int vertsPerSide = 20;

            //generate top and bottom vertices
            vertices.Add(new Vector3(0, 0, 0f)); // center
            uvs.Add(new Vector2(0.5f, 0.4f)); // center bottom

            for (int i = 0; i <= vertsPerSide; i++)
            {
                float a = i / (float)vertsPerSide;
                vertices.Add(new Vector3(Mathf.Sin(a * Mathf.PI * 2), 0, Mathf.Cos(a * Mathf.PI * 2)));
                uvs.Add(new Vector3(i % 2, 0.99f));
            }

            vertices.Add(vertices[1]);
            uvs.Add(uvs[1]);

            List<int> triangles = new List<int>();

            for (int i = 0; i <= vertsPerSide; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();

            AssetDatabase.CreateAsset(mesh, Path.Combine(path, name));
            AssetDatabase.Refresh();
        }

        private static void loadBmpDirectory(DirectoryInfo directory, string pathPrefixToRemove)
        {
            string pathParent = directory.Parent.ToString();

            string newDir = Application.dataPath + "\\" + directory.FullName.Replace(pathPrefixToRemove, "");
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);

            var files = directory.GetFiles("*.*");
            foreach (var file in files)
            {
                if (file.Extension.ToLower().EndsWith(".bmp"))
                {
                    ROExTool.convertToRGBATexture(file.FullName, newDir + "\\" + Path.GetFileNameWithoutExtension(file.Name) + ".png", false);

                    /* var bitmap = new Bitmap(file.FullName);

                     bitmap.MakeTransparent(System.Drawing.Color.Fuchsia);
                     bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                     bitmap.Save(newDir + "\\" + Path.GetFileNameWithoutExtension(file.Name) + ".png");
                     */
                }
            }

            foreach (var dir in directory.GetDirectories())
            {
                loadBmpDirectory(dir, pathPrefixToRemove);
            }
        }

        [MenuItem("Assets/Extra/Load Bitmap directory as png")]
        private static void LoadBitmapDirectory()
        {
            string path = EditorUtility.OpenFolderPanel("Select the bitmapas folder", "", "");

            DirectoryInfo directory = new DirectoryInfo(path);
            string pathParent = directory.Parent.ToString();

            loadBmpDirectory(directory, pathParent);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        [MenuItem("Assets/Extra/Miscellaneous/Remove pink from textures")]
        private static void RemovePinkFromTextures()
        {
            if (Selection.objects.Length == 0)
                return;

            foreach (var obj in Selection.objects)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(obj));
                if (texture != null)
                    MapGeneratorUtils.RemovePinkFromTexture(texture);
            }

            AssetDatabase.Refresh();
        }

        //[MenuItem("Assets/Extra/Miscellaneous/Dump Cursor Animations")]
        private static void DumpCursorAnimations()
        {
            //var obj = GameObject.Find("CursorAnimator").GetComponent<CursorAnimator>();
            /*var animations = obj.GetAnimations();

            CursorData data = ScriptableObject.CreateInstance<CursorData>();

            data._cursorAnimations = new CursorData.CursorAnimation[animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                data._cursorAnimations[i].textures = new Texture2D[animations[i].textures.Length];
                for (int k = 0; k < animations[i].textures.Length; k++)
                    data._cursorAnimations[i].textures[k] = animations[i].textures[k];

                data._cursorAnimations[i].cursorFrames = new CursorData.CursorFrame[animations[i].cursorFrames.Length];
                for (int k = 0; k < animations[i].cursorFrames.Length; k++)
                {
                    data._cursorAnimations[i].cursorFrames[k].textureId = animations[i].cursorFrames[k].textureId;
                    data._cursorAnimations[i].cursorFrames[k].hotspot = animations[i].cursorFrames[k].hotspot;
                }
            }

            AssetDatabase.CreateAsset(data, "Assets/~Resources/Misc/cursors/cursorData.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();*/
        }
    }
}
