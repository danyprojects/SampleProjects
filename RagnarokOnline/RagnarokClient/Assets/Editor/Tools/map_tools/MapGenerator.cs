using RO.Common;
using RO.Containers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTools
{
    public class MapGenerator
    {
        public const string MAP_FOLDER = "Assets/~Resources/GRF/Maps";
        public const string SCENE_FOLDER = "Assets/Scenes/Maps/";
        public const string waterMaterialPath = "Assets/~Resources/Grf/Maps/water_mat.mat";
        public static string MapFileFolder { get; private set; } = "";
        public static string FileNameNoExt { get; private set; } = "";
        public static int MapId = 0;
        private static int MapWidth, MapHeight;
        private static int ObjectIndex;

        //*************** Methods for map manual fixes. Might go away if we can fix the matrix rotation issues
        [MenuItem("Assets/Extra/Maps/Extract transform fixes")]
        private static void ExtractTransformFixes()
        {
            string sceneName = EditorSceneManager.GetActiveScene().name;

            var ground = GameObject.Find("Ground").transform;

            var fixes = ExtractChildFixes(ground);

            TransformFixes transformFixes = ScriptableObject.CreateInstance<TransformFixes>();
            transformFixes.fixes = fixes.ToArray();

            AssetDatabase.CreateAsset(transformFixes, Path.Combine(MAP_FOLDER, sceneName, sceneName + "_fixes.asset"));
        }

        private static List<TransformFixes.Fix> ExtractChildFixes(Transform parent, bool firstLevel = true)
        {
            List<TransformFixes.Fix> fixes = new List<TransformFixes.Fix>();

            //Go through all the ground children and get transform fixes
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i).transform;

                //skip if object doesn't have children
                if (firstLevel && child.childCount == 0)
                    continue;

                TransformFixes.Fix fix = new TransformFixes.Fix();
                fix.objectIndex = i;
                fix.childFixes = ExtractChildFixes(child, false).ToArray();

                //skip if there's no position or rotation changes
                if (!firstLevel && (child.localPosition != Vector3.zero || child.localEulerAngles != Vector3.zero))
                {
                    fix.positionFix = child.localPosition;
                    fix.rotationFix = child.localEulerAngles;
                }

                //if there's at least 1 thing to fix, add it to the list of fixes
                if (fix.childFixes.Length != 0 || fix.positionFix != Vector3.zero || fix.rotationFix != Vector3.zero)
                    fixes.Add(fix);
            }

            return fixes;
        }

        private static void ApplyMapFixes()
        {
            string fixesFile = Path.Combine(MapFileFolder, FileNameNoExt + "_fixes.asset");

            if (!File.Exists(fixesFile))
                return;

            var fixes = AssetDatabase.LoadAssetAtPath<TransformFixes>(fixesFile);

            var ground = GameObject.Find("Ground").transform;

            //First lvl does't change positions
            foreach (var fix in fixes.fixes)
                ApplyChildFixes(ground.GetChild(fix.objectIndex), fix.childFixes);
        }

        private static void ApplyChildFixes(Transform trans, TransformFixes.Fix[] fixes)
        {
            foreach (var fix in fixes)
            {
                var child = trans.GetChild(fix.objectIndex).transform;

                child.localPosition = fix.positionFix;
                child.localEulerAngles = fix.rotationFix;

                //apply fixes to childs too
                ApplyChildFixes(child, fix.childFixes);

            }
        }

        //***********  Methods for map generation
        [MenuItem("Assets/Extra/Maps/Generate map")]
        private static void GenerateMap()
        {
            string rswPath = null, gndPath = null, gatPath = null;

            MapGeneratorUtils.ValidateSelection(ref gndPath, ref rswPath, ref gatPath);

            string fileName = Path.GetFileName(gndPath);
            FileNameNoExt = Path.GetFileNameWithoutExtension(fileName);

            try
            {
                MapId = (int)System.Enum.Parse(typeof(RO.Databases.MapDb.MapIds), FileNameNoExt);
            }
            catch (System.Exception)
            {
                throw new System.Exception("Map doesnt exist in id");
            }

            //Create the path for the folder or delete all contents for new generation
            MapFileFolder = Path.Combine(MAP_FOLDER, FileNameNoExt);

            if (!Directory.Exists(MapFileFolder))
                Directory.CreateDirectory(MapFileFolder);
            else
                foreach (string file in Directory.GetFiles(MapFileFolder))
                    if (!file.Contains(".meta") && !file.Contains(".gnd") && !file.Contains(".gat") && !file.Contains(".rsw") && !file.Contains("_fixes"))
                        AssetDatabase.DeleteAsset(file);

            AssetDatabase.Refresh();

            //Create the new scene
            string scenePath = Path.Combine(SCENE_FOLDER, FileNameNoExt + ".unity");

            if (File.Exists(scenePath))
                File.Delete(scenePath);
            AssetDatabase.Refresh();

            //Save old scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            //Create new scene and save it
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            if (!scene.isLoaded)
                throw new System.Exception("Scene didn't load");

            //parse map files
            Gat gat = GatParser.ParseGat(gatPath);
            Gnd gnd = new Gnd(gndPath.Substring(0, gndPath.LastIndexOf('/') + 1), fileName);
            Rsw rsw = new Rsw(rswPath);

            //Get all rsms from rsw // currently ignoring sounds, lights and effects
            rsw.rsms = GenerateRswObjects(rsw);

            Texture2D[,] waterTex;
            MapGeneratorUtils.LoadWaterTextures(out waterTex);

            Material[] materials = new Material[1];
            Mesh ground = GenerateGroundMesh(gnd, out materials);
            Mesh water = GenerateWaterMesh(gnd, rsw);
            MapData mapData = GenerateMapData(gnd, rsw, gat);
            MapWidth = mapData.Width * Constants.CELL_TO_UNIT_SIZE;
            MapHeight = mapData.Height * Constants.CELL_TO_UNIT_SIZE;

            BuildScene(ref materials, ground, water, mapData, gat, rsw, scenePath);
        }

        private static Mesh GenerateGroundMesh(Gnd gnd, out Material[] materials)
        {
            //Variables for mesh
            int submeshes = gnd.textures.Length;
            List<Vector3> vertices = new List<Vector3>();
            List<int>[] triangles = new List<int>[submeshes]; // Triangles define the submeshes
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector2> colormap = new List<Vector2>();
            List<Vector2> shadowmap = new List<Vector2>();
            List<Color> colors = new List<Color>();

            for (int i = 0; i < submeshes; i++)
                triangles[i] = new List<int>();

            Debug.Log("Version: " + gnd.version);
            Debug.Log("Dimensions: " + gnd.width + ", " + gnd.height);
            Debug.Log("length: " + gnd.cubes.GetLength(0) + ", " + gnd.cubes.GetLength(1));
            Debug.Log("Submeshes: " + submeshes);
            Debug.Log("Tiles: " + gnd.tiles.Length);

            var lightmap = MapGeneratorUtils.GenerateLargeLightmapTexture(gnd, ref gnd.lightmaps);
            int size = Constants.CELL_TO_UNIT_SIZE * 2; //Each tile in this ground has 2x2 cells;

            //Set mesh fields
            for (int y = 0; y < gnd.height; y++) //gnd.height
            {
                for (int x = 0; x < gnd.width; x++) //gnd.width
                {
                    //Get a cube / surface
                    Gnd.Cube cube_a = gnd.cubes[x, y];

                    float[] heights_a = cube_a.height; // heights of cube

                    //Add the mesh info of the top of the cell
                    if (cube_a.tileUp > -1)
                    {
                        Gnd.Tile tile = gnd.tiles[cube_a.tileUp];

                        triangles[tile.textureIndex].AddRange(new int[]
                        {
                            vertices.Count + 0,
                            vertices.Count + 3,
                            vertices.Count + 1,
                            vertices.Count + 0,
                            vertices.Count + 2,
                            vertices.Count + 3
                        });

                        //Add the vertices for previous triangles
                        var verts = new Vector3[]
                        {
                            new Vector3((x + 0) * size, -heights_a[0], (y + 0) * size), //0
                            new Vector3((x + 1) * size, -heights_a[1], (y + 0) * size), //1
                            new Vector3((x + 0) * size, -heights_a[2], (y + 1) * size), //2
                            new Vector3((x + 1) * size, -heights_a[3], (y + 1) * size), //3
                        };
                        vertices.AddRange(verts);

                        normals.AddRange(MapGeneratorUtils.CalcNormals(ref verts));

                        //Get the color from adjacent tiles for right, up and top right
                        colors.AddRange(new Color[]
                        {
                            tile.color, //0
                            (x + 1 < gnd.width && gnd.cubes[x + 1, y].tileUp > -1 ) ? gnd.tiles[gnd.cubes[x + 1, y].tileUp].color : Color.black, // 1
                            (y + 1 < gnd.height && gnd.cubes[x, y + 1].tileUp > -1 ) ? gnd.tiles[gnd.cubes[x, y + 1].tileUp].color : Color.black, // 2
                            (x + 1 < gnd.width && y + 1 < gnd.height && gnd.cubes[x + 1, y + 1].tileUp > -1 ) ? gnd.tiles[gnd.cubes[x + 1, y + 1].tileUp].color : Color.black, // 3
                        });

                        uvs.AddRange(new Vector2[]
                        {
                            new Vector2(tile.uv0.x, 1 - tile.uv0.y), //1 - y because RO inverts y coordinates
                            new Vector2(tile.uv1.x, 1 - tile.uv1.y),
                            new Vector2(tile.uv2.x, 1 - tile.uv2.y),
                            new Vector2(tile.uv3.x, 1 - tile.uv3.y),
                        });

                        Vector2[] uv = MapGeneratorUtils.GetLightmapUvCoordinates(tile.lightmapIndex, lightmap);
                        colormap.AddRange(new Vector2[]
                        {
                            new Vector2(uv[0].x,uv[0].y),
                            new Vector2(uv[1].x,uv[0].y),
                            new Vector2(uv[0].x,uv[1].y),
                            new Vector2(uv[1].x,uv[1].y)
                        });
                    }

                    //Add the mesh info of the front (north)
                    if (cube_a.tileFront > -1 && y + 1 < gnd.height)
                    {
                        Gnd.Tile tile = gnd.tiles[cube_a.tileFront];
                        Gnd.Cube cube_b = gnd.cubes[x, y + 1];
                        float[] heights_b = cube_b.height;

                        triangles[tile.textureIndex].AddRange(new int[]
                        {
                                        vertices.Count + 0,
                                        vertices.Count + 3,
                                        vertices.Count + 1,
                                        vertices.Count + 0,
                                        vertices.Count + 2,
                                        vertices.Count + 3
                        });

                        //Add the vertices for previous triangles
                        vertices.AddRange(new Vector3[]
                        {
                            new Vector3((x + 0) * size, -heights_a[2], (y + 1) * size), //0
                            new Vector3((x + 1) * size, -heights_a[3], (y + 1) * size), //1
                            new Vector3((x + 0) * size, -heights_b[0], (y + 1) * size), //2
                            new Vector3((x + 1) * size, -heights_b[1], (y + 1) * size)  //3
                        });

                        for (int i = 0; i < 4; i++)
                        {
                            normals.Add(new Vector3(0, 0, -1));
                            colors.Add(Color.white); // Add color white for all verts for now
                        }

                        uvs.AddRange(new Vector2[]
                        {
                            new Vector2(tile.uv0.x, 1 - tile.uv0.y),
                            new Vector2(tile.uv1.x, 1 - tile.uv1.y),
                            new Vector2(tile.uv2.x, 1 - tile.uv2.y),
                            new Vector2(tile.uv3.x, 1 - tile.uv3.y),
                        });

                        Vector2[] uv = MapGeneratorUtils.GetLightmapUvCoordinates(tile.lightmapIndex, lightmap);
                        colormap.AddRange(new Vector2[]
                        {
                            new Vector2(uv[0].x,uv[0].y),
                            new Vector2(uv[1].x,uv[0].y),
                            new Vector2(uv[0].x,uv[1].y),
                            new Vector2(uv[1].x,uv[1].y)
                        });
                    }

                    //Add the mesh info of the right side (east)
                    if (cube_a.tileSide > -1 && x + 1 < gnd.width) // side is right in ro browser
                    {
                        Gnd.Tile tile = gnd.tiles[cube_a.tileSide];
                        Gnd.Cube cube_b = gnd.cubes[x + 1, y];
                        float[] heights_b = cube_b.height;

                        triangles[tile.textureIndex].AddRange(new int[]
                        {
                                        vertices.Count + 0,
                                        vertices.Count + 3,
                                        vertices.Count + 1,
                                        vertices.Count + 0,
                                        vertices.Count + 2,
                                        vertices.Count + 3
                        });

                        //Add the vertices for previous triangles
                        vertices.AddRange(new Vector3[]
                        {
                            new Vector3((x + 1) * size, -heights_b[0], (y + 0) * size), //0
                            new Vector3((x + 1) * size, -heights_b[2], (y + 1) * size), //1
                            new Vector3((x + 1) * size, -heights_a[1], (y + 0) * size), //2
                            new Vector3((x + 1) * size, -heights_a[3], (y + 1) * size)  //3
                        });

                        for (int i = 0; i < 4; i++)
                        {
                            normals.Add(new Vector3(1, 0, 0));
                            colors.Add(Color.white); // Add color white for all verts for now
                        }

                        uvs.AddRange(new Vector2[]
                        {
                            new Vector2(tile.uv3.x, 1 - tile.uv3.y),
                            new Vector2(tile.uv2.x, 1 - tile.uv2.y),
                            new Vector2(tile.uv1.x, 1 - tile.uv1.y),
                            new Vector2(tile.uv0.x, 1 - tile.uv0.y)
                        });

                        Vector2[] uv = MapGeneratorUtils.GetLightmapUvCoordinates(tile.lightmapIndex, lightmap);
                        colormap.AddRange(new Vector2[]
                        {
                            new Vector2(uv[1].x,uv[1].y),
                            new Vector2(uv[0].x,uv[1].y),
                            new Vector2(uv[1].x,uv[0].y),
                            new Vector2(uv[0].x,uv[0].y)
                        });
                    }

                }
            }

            //End of setting mesh fields
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.subMeshCount = submeshes;
            mesh.vertices = vertices.ToArray();
            for (int i = 0; i < submeshes; i++)
                mesh.SetTriangles(triangles[i].ToArray(), i);
            mesh.uv = uvs.ToArray();
            mesh.colors = colors.ToArray(); //TODO: check if colors are correct on other maps. It's applying to whole tile
            mesh.uv2 = colormap.ToArray();
            mesh.RecalculateNormals();
            mesh.SetNormals(normals);

            materials = MapGeneratorUtils.GetMapTextureMaterials(ref gnd.textures);

            string meshName = Path.Combine(MapFileFolder, FileNameNoExt + "_mesh.asset");
            AssetDatabase.CreateAsset(mesh, meshName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetImporter.GetAtPath(meshName).SetAssetBundleNameAndVariant("maps/maps", "");

            return mesh;
        }

        private static Mesh GenerateWaterMesh(Gnd gnd, Rsw rsw)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int size = Constants.CELL_TO_UNIT_SIZE * 2; //Each tile in this ground has 2x2 cells;

            for (int y = 0; y < gnd.height; y++) //gnd.height
            {
                for (int x = 0; x < gnd.width; x++) //gnd.width
                {
                    //Get a cube / surface
                    Gnd.Cube cube = gnd.cubes[x, y];
                    float[] heights = cube.height; // heights of cube

                    //Add the mesh info of the top of the cell
                    if (cube.tileUp > -1)
                    {
                        var tile = gnd.tiles[cube.tileUp];

                        if (heights[0] > rsw.water.level - rsw.water.waveHeight ||
                           heights[1] > rsw.water.level - rsw.water.waveHeight ||
                           heights[2] > rsw.water.level - rsw.water.waveHeight ||
                           heights[3] > rsw.water.level - rsw.water.waveHeight)
                        {
                            triangles.AddRange(new int[]
                            {
                                vertices.Count + 0,
                                vertices.Count + 3,
                                vertices.Count + 1,
                                vertices.Count + 0,
                                vertices.Count + 2,
                                vertices.Count + 3
                            });

                            //Add the vertices for previous triangles
                            vertices.AddRange(new Vector3[]
                            {
                            new Vector3((x + 0) * size, -rsw.water.level, (y + 0) * size), //0
                            new Vector3((x + 1) * size, -rsw.water.level, (y + 0) * size), //1
                            new Vector3((x + 0) * size, -rsw.water.level, (y + 1) * size), //2
                            new Vector3((x + 1) * size, -rsw.water.level, (y + 1) * size), //3
                            });

                            float x0 = (x + 0) % 5 / 5f, x1 = (x + 1) % 5 / 5f;
                            float y0 = (y + 0) % 5 / 5f, y1 = (y + 1) % 5 / 5f;
                            x1 = x1 >= 0.0005f ? x1 : 1;
                            y1 = y1 >= 0.0005f ? y1 : 1;

                            uvs.AddRange(new Vector2[]
                            {
                                new Vector2(x0, y0),
                                new Vector2(x1, y0),
                                new Vector2(x0, y1),
                                new Vector2(x1, y1)
                            });
                        }
                    }
                }
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            // mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();

            string meshName = Path.Combine(MapFileFolder, FileNameNoExt + "Water_mesh.asset");
            AssetDatabase.CreateAsset(mesh, meshName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetImporter.GetAtPath(meshName).SetAssetBundleNameAndVariant("maps/maps", "");

            return mesh;
        }

        private static MapData GenerateMapData(Gnd gnd, Rsw rsw, Gat gat)
        {
            System.Type mapDataType = typeof(MapData);

            MapData mapData = ScriptableObject.CreateInstance<MapData>();
            Texture2D lightmap = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(MapGenerator.MapFileFolder, MapGenerator.FileNameNoExt + "_lm.png"));

            //Use reflection to set readonly variables in map data
            mapDataType.GetField("_id", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mapData, MapId);
            mapDataType.GetField("_lightmap", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mapData, lightmap);

            //Fill the tile data
            mapDataType.GetField("_width", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mapData, gat.cells.GetLength(0));
            mapDataType.GetField("_height", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mapData, gat.cells.GetLength(1));
            mapDataType.GetField("_tiles", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mapData, new MapData.Tile[mapData.Width * mapData.Height]);

            //Map right and top borders are always ignored in ro Gats
            for (int x = 0; x < mapData.Width - 1; x++)
                for (int y = 0; y < mapData.Height - 1; y++)
                {
                    mapData.Tiles[x + y * mapData.Width].IsWalkable = gat.cells[x, y].walkable;
                    mapData.Tiles[x + y * mapData.Width].IsSnipable = gat.cells[x, y].snipe;
                    mapData.Tiles[x + y * mapData.Width].HasWater = gat.cells[x, y].water;
                }

            //Fill water info
            MapData.Water waterInfo = new MapData.Water();
            waterInfo.type = rsw.water.type;
            waterInfo.height = rsw.water.waveHeight;
            waterInfo.pitch = rsw.water.wavePitch;
            waterInfo.speed = rsw.water.waveSpeed;
            waterInfo.animationSpeed = rsw.water.textureCycling;
            mapDataType.GetField("_waterInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mapData, waterInfo);

            //Fill light info
            var lightInfo = new MapData.Lighting();
            lightInfo.latitude = rsw.light.latitude;
            lightInfo.longitude = rsw.light.longitude;
            lightInfo.ambient = rsw.light.ambient;
            lightInfo.diffuse = rsw.light.diffuse;
            lightInfo.ambientIntensity = rsw.light.intensity;
            mapDataType.GetField("_lightingInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mapData, lightInfo);

            string mapDataName = Path.Combine(MapFileFolder, FileNameNoExt + "map_data.asset");
            AssetDatabase.CreateAsset(mapData, mapDataName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetImporter.GetAtPath(mapDataName).SetAssetBundleNameAndVariant("maps/misc", "");

            return mapData;
        }

        private static List<Rsm> GenerateRswObjects(Rsw rsw)
        {
            const string modelPath = "Assets/~Resources/GRF/Objects";
            const string modelDataPath = "Assets/~Resources/GRF/Objects/ObjectData";
            List<Rsm> rsms = new List<Rsm>();

            int index = -1;
            foreach (var rswObject in rsw.objects)
            {
                //lets process X only
                index++;
                //if (index != 37)
                //    continue;

                if (rswObject.type == Rsw.Object.Type.Model)
                {
                    var rswModel = (Rsw.Model)rswObject;
                    //Load the rsm
                    var rsm = new Rsm(Path.Combine(modelPath, rswModel.fileName));

                    //In case directory hasn't been created
                    string rsmModelDataFolder = Path.Combine(modelDataPath, Path.GetDirectoryName(rswModel.fileName));
                    if (!Directory.Exists(rsmModelDataFolder))
                    {
                        Directory.CreateDirectory(rsmModelDataFolder);
                        AssetDatabase.Refresh();
                    }

                    //Try to load existing model data
                    string rsmModelDataPath = Path.Combine(rsmModelDataFolder, Path.GetFileNameWithoutExtension(rswModel.fileName) + "_md.asset");
                    rsm.modelData = AssetDatabase.LoadAssetAtPath<ModelData>(rsmModelDataPath);

                    //If model data doesn't exist then generate it                   
                    if (rsm.modelData == null)
                    {
                        List<ModelData.MeshData> meshDatas = new List<ModelData.MeshData>();
                        List<ModelData.ModelAnimation> modelAnims = new List<ModelData.ModelAnimation>();
                        MapGeneratorUtils.GenerateMeshChildren(rsm.rootMesh, meshDatas, modelAnims);

                        rsm.modelData = ScriptableObject.CreateInstance<ModelData>();
                        rsm.modelData.meshDatas = meshDatas.ToArray();
                        rsm.modelData.modelAnimations = modelAnims.ToArray();

                        //Create the asset
                        AssetDatabase.CreateAsset(rsm.modelData, rsmModelDataPath);
                        AssetDatabase.Refresh();
                        AssetImporter.GetAtPath(rsmModelDataPath).SetAssetBundleNameAndVariant("maps/models", "");
                    }

                    //Add the rsm into the list
                    rsms.Add(rsm);
                }
            }

            return rsms;
        }

        private static Mesh GenerateMapDataGrid(Gat gat, GameObject ground, Mesh groundMesh)
        {
            List<int> triangles = new List<int>();
            List<Vector3> vertices = new List<Vector3>();

            //Create the mesh for the grid
            for (int y = 0; y < gat.cells.GetLength(1); y++)
            {
                for (int x = 0; x < gat.cells.GetLength(0); x++)
                {
                    triangles.AddRange(new int[]
                    {
                        vertices.Count + 0,
                        vertices.Count + 3,
                        vertices.Count + 1,
                        vertices.Count + 0,
                        vertices.Count + 2,
                        vertices.Count + 3
                    });

                    //Add the vertices for previous triangles
                    vertices.AddRange(new Vector3[]
                    {
                        new Vector3((x + 0) * Constants.CELL_TO_UNIT_SIZE, -gat.cells[x,y].heights[0] * 1.2f, (y + 0) * Constants.CELL_TO_UNIT_SIZE), //0
                        new Vector3((x + 1) * Constants.CELL_TO_UNIT_SIZE, -gat.cells[x,y].heights[1] * 1.2f, (y + 0) * Constants.CELL_TO_UNIT_SIZE), //1
                        new Vector3((x + 0) * Constants.CELL_TO_UNIT_SIZE, -gat.cells[x,y].heights[2] * 1.2f, (y + 1) * Constants.CELL_TO_UNIT_SIZE), //2
                        new Vector3((x + 1) * Constants.CELL_TO_UNIT_SIZE, -gat.cells[x,y].heights[3] * 1.2f, (y + 1) * Constants.CELL_TO_UNIT_SIZE), //3
                    });
                }
            }

            Mesh mapGrid = new Mesh();
            mapGrid.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mapGrid.vertices = vertices.ToArray();
            mapGrid.triangles = triangles.ToArray();

            //Create a map collider so we can raycast and fix grid heights
            var collider = ground.AddComponent<MeshCollider>();
            collider.sharedMesh = groundMesh;

            //TODO

            GameObject.DestroyImmediate(collider);

            string meshName = Path.Combine(MapFileFolder, FileNameNoExt + "grid_mesh.asset");
            AssetDatabase.CreateAsset(mapGrid, meshName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssetImporter.GetAtPath(meshName).SetAssetBundleNameAndVariant("maps/maps", "");

            return mapGrid;
        }

        private static void BuildScene(ref Material[] materials, Mesh groundMesh, Mesh waterMesh, MapData mapData, Gat gat, Rsw rsw, string scenePath)
        {
            //Don't change original object scale so we don't change the map grid
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>(waterMaterialPath);
            int mapId = (int)System.Enum.Parse(typeof(RO.Databases.MapDb.MapIds), FileNameNoExt, true);
            GameObject mapObj = new GameObject(System.Enum.GetName(typeof(RO.Databases.MapDb.MapIds), mapId));
            mapObj.layer = LayerIndexes.Map;
            RO.Map map = mapObj.AddComponent<RO.Map>();
            map.MapData = mapData;

            //Create and fill the ground object
            GameObject ground = new GameObject("Ground");
            ground.transform.SetParent(mapObj.transform, true);
            ground.AddComponent<MeshFilter>().mesh = groundMesh;
            ground.AddComponent<MeshRenderer>().materials = materials;
            ground.GetComponent<MeshRenderer>().receiveShadows = false;
            ground.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ground.transform.localScale = new Vector3(1, 1.2f, 1);


            //Create and fill the water object
            GameObject water = new GameObject("Water");
            water.transform.SetParent(mapObj.transform);
            water.AddComponent<MeshFilter>().mesh = waterMesh;
            water.AddComponent<MeshRenderer>().material = waterMat;
            water.transform.localScale = new Vector3(1, 1.2f, 1);

            //Use reflection to set private map variable
            System.Type mapType = typeof(RO.Map);
            FieldInfo fieldInfo = mapType.GetField("_waterRenderer", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo.SetValue(map, water.GetComponent<MeshRenderer>());

            //Assign the map grid
            Mesh mapGrid = GenerateMapDataGrid(gat, ground, groundMesh);
            mapObj.AddComponent<MeshCollider>().sharedMesh = mapGrid;

            GameObject lights = new GameObject("Lights");
            lights.transform.SetParent(mapObj.transform, true);
            lights.transform.localScale = new Vector3(1, 1.2f, 1);

            //Pre-build map rsm objects
            BuildSceneRswObjects(ground, lights, rsw, map);

            //TODO: Lights, sounds and effects

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
            RenderSettings.ambientIntensity = 1;

            //Save scene
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //Add scene to build settings
            var sceneList = EditorBuildSettings.scenes;
            var newList = new EditorBuildSettingsScene[sceneList.Length + 1];
            System.Array.Copy(sceneList, newList, sceneList.Length);

            newList[sceneList.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newList;

            EditorScriptsUtility.SortBuildScenes();
        }

        private static void BuildSceneRswObjects(GameObject ground, GameObject lights, Rsw rsw, RO.Map map)
        {
            //This is how many rswObjects of type MODEL there will be in the map
            map.modelObjects = new RO.Map.ModelObject[rsw.rsms.Count];

            //int index = 37;
            //BuildRsmSceneObject(ground, rsw.objects[index], ref map.modelObjects[0], rsw.rsms[0]);

            //return;

            //Go through all rsw objects
            int rsmIndex = 0;
            ObjectIndex = 0;
            foreach (var rswObj in rsw.objects)
            {
                //TODO: light, sound and effect
                switch (rswObj.type)
                {
                    case Rsw.Object.Type.Model: BuildRsmSceneObject(ground, rswObj, ref map.modelObjects[rsmIndex], rsw.rsms[rsmIndex++]); break;
                    case Rsw.Object.Type.Light: BuildRswSceneLight(lights, rswObj as Rsw.Light); break;
                    case Rsw.Object.Type.Sound:
                    case Rsw.Object.Type.Effect:
                    default: Debug.Log("Todo"); break;
                }
                ObjectIndex++;
            }

            //In case we had map fixes from other iterations
            ApplyMapFixes();
        }

        private static void BuildRsmSceneObject(GameObject ground, Rsw.Object rswObject, ref RO.Map.ModelObject modelObject, Rsm rsm)
        {
            //There's only 1 model data per object. It contains all meshes
            modelObject.modelData = rsm.modelData;

            //Create all the game objects for this model
            GameObject[] models = new GameObject[rsm.modelData.meshDatas.Length];
            MeshRenderer[] meshRenderers = new MeshRenderer[models.Length];
            modelObject.meshFilters = new MeshFilter[models.Length];

            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new GameObject(ObjectIndex + "_" + rsm.rootMesh.name + "_" + i);
                //All of them need a mesh filter and mesh renderer
                meshRenderers[i] = models[i].AddComponent<MeshRenderer>(); // store for using below
                modelObject.meshFilters[i] = models[i].AddComponent<MeshFilter>(); //store the mesh filter for runtime
            }

            //Go through all meshes in an rsm model data to set the parents
            for (int i = 0; i < rsm.modelData.meshDatas.Length; i++)
            {
                var meshData = rsm.modelData.meshDatas[i];
                //if it's root mesh assign it to the ground. If it's not root mesh then assign it to an already created game object
                if (meshData.parent == -1)
                    models[i].transform.SetParent(ground.transform);
                else
                    models[i].transform.SetParent(models[meshData.parent].transform);
            }

            //Get the materials to be used by the mesh renderers
            var materials = MapGeneratorUtils.GetMeshTextureMaterials(ref rsm.textures);

            Material[] rootMaterials = new Material[rsm.rootMesh.textures.Length];
            for (int i = 0; i < rootMaterials.Length; i++)
                rootMaterials[i] = materials[rsm.rootMesh.textures[i]];
            models[0].GetComponent<MeshRenderer>().materials = rootMaterials; //root object has all materials
            models[0].GetComponent<MeshRenderer>().receiveShadows = false;
            models[0].GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            //Set the transform components for root object as these are from the rswObject and not the RSM
            var pos = new Vector3(MapWidth / 2f + rswObject.position.x, -rswObject.position.y, MapHeight / 2f + rswObject.position.z);
            models[0].transform.localPosition = pos;

            models[0].transform.Rotate(new Vector3(0, 0, -rswObject.rotation.z));
            models[0].transform.Rotate(new Vector3(-rswObject.rotation.x, 0, 0));
            models[0].transform.Rotate(new Vector3(0, rswObject.rotation.y, 0));

            models[0].transform.localScale = rswObject.scale;

            //lastly, set transform and material components for children objects
            int index = 1;
            foreach (var rsmMesh in rsm.rootMesh.children)
                SetRsmChildMaterials(ref models, rsmMesh, ref materials, ref index);
        }

        private static void BuildRswSceneLight(GameObject lights, Rsw.Light rswLight)
        {
            GameObject lightObj = new GameObject(rswLight.name);
            lightObj.transform.SetParent(lights.transform, true);

            var light = lightObj.AddComponent<Light>();

            light.type = LightType.Point;
            light.color = new Color(rswLight.color.x, rswLight.color.y, rswLight.color.z);
            light.range = rswLight.range;

            light.transform.position = new Vector3(MapWidth / 2f + rswLight.position.x, -rswLight.position.y, MapHeight / 2f + rswLight.position.z);
        }

        private static void SetRsmChildMaterials(ref GameObject[] objs, Rsm.Mesh rsmMesh, ref Material[] materials, ref int index)
        {
            //Get the materials for this mesh
            var texIndex = rsmMesh.textures;
            Material[] childMaterials = new Material[rsmMesh.textures.Length];
            for (int i = 0; i < childMaterials.Length; i++)
                childMaterials[i] = materials[rsmMesh.textures[i]];

            //set them to the render
            objs[index].GetComponent<MeshRenderer>().materials = childMaterials;

            index++; //go to next object

            //Recursively do the same for the children
            foreach (var childMesh in rsmMesh.children)
                SetRsmChildMaterials(ref objs, childMesh, ref materials, ref index);
        }
    }
}
