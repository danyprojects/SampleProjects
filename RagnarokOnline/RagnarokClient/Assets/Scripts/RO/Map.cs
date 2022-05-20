using RO.Containers;
using UnityEngine;

namespace RO
{
    public class Map : MonoBehaviour
    {

        [System.Serializable]
        public struct ModelObject
        {
            public ModelData modelData;
            public MeshFilter[] meshFilters;
        }

        [SerializeField] private MeshRenderer _waterRenderer = null;
        private Texture2D[] _waterTextures = null;

        public MapData MapData;
        public ModelObject[] modelObjects;
        private Vector3[] _vertices = null;

        private float _animationSpeed = 0;
        private float _nextUpdate = 0;
        private int _currentFrame = 0;
        private MaterialPropertyBlock _propertyBlock;

#if DEBUG
        GameObject blockGridMesh;
#endif

        private void Awake()
        {
            // make a copy because the getter always returns one and will make it really slow...
            _vertices = GetComponent<MeshCollider>().sharedMesh.vertices;

            _propertyBlock = new MaterialPropertyBlock();

            //Set water shader info
            _waterRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(Media.MediaConstants.SHADER_WAVE_HEIGHT_ID, MapData.WaterInfo.height);
            _propertyBlock.SetFloat(Media.MediaConstants.SHADER_WAVE_PITCH_ID, MapData.WaterInfo.pitch);
            _propertyBlock.SetFloat(Media.MediaConstants.SHADER_WAVE_SPEED_ID, MapData.WaterInfo.speed);
            _waterRenderer.SetPropertyBlock(_propertyBlock);

            //Set lightmap and light global variables
            Shader.SetGlobalTexture(Media.MediaConstants.SHADER_LIGHTMAP_ID, MapData.Lightmap);
            Shader.SetGlobalColor(Media.MediaConstants.SHADER_AMBIENT_LIGHT_COLOR_ID, MapData.LightingInfo.ambient);
            Shader.SetGlobalFloat(Media.MediaConstants.SHADER_AMBIENT_LIGHT_INTENSITY_ID, MapData.LightingInfo.ambientIntensity);
            Shader.SetGlobalColor(Media.MediaConstants.SHADER_DIFFUSE_COLOR_ID, MapData.LightingInfo.diffuse);
            Shader.SetGlobalVector(Media.MediaConstants.SHADER_DIFFUSE_DIRECTION_ID, new Vector2(MapData.LightingInfo.latitude / 100f, MapData.LightingInfo.longitude / 100f)); ;

            //Load the water textures
            AssetBundleProvider.GetWaterBundleTextures(MapData.WaterInfo.type, out _waterTextures);
            _animationSpeed = (1 / 60f) * MapData.WaterInfo.animationSpeed;

            //Load all model data meshes into their filteres. Model data has an array with the mesh filters we need to assign
            for (int i = 0; i < modelObjects.Length; i++)
                for (int k = 0; k < modelObjects[i].meshFilters.Length; k++)
                {
                    Mesh mesh = new Mesh();
                    mesh.vertices = modelObjects[i].modelData.meshDatas[k].vertices;
                    mesh.subMeshCount = modelObjects[i].modelData.meshDatas[k].submeshes.Length;
                    for (int p = 0; p < mesh.subMeshCount; p++)
                        mesh.SetTriangles(modelObjects[i].modelData.meshDatas[k].submeshes[p].triangles, p);
                    mesh.uv = modelObjects[i].modelData.meshDatas[k].uvs;

                    modelObjects[i].meshFilters[k].mesh = mesh;
                }

#if DEBUG
            Mesh gridMesh = new Mesh();
            int blockGridWidth = MapData.Width / 8 + 1, blockGridHeight = MapData.Height / 8 + 1;
            Vector3[] vertices = new Vector3[blockGridWidth * blockGridHeight * 4]; //width * height in blocks * 4 vertices per square
            Vector2[] uvs = new Vector2[blockGridWidth * blockGridHeight * 4]; // 1 uv per vertex
            int[] triangles = new int[blockGridWidth * blockGridHeight * 2 * 3]; //2 triangles per square 3 verts per triangle

            int x = 0, y = 0;
            int cellSize = 8 * Common.Constants.CELL_TO_UNIT_SIZE;
            for (int i = 0; i < blockGridWidth * blockGridHeight; i++)
            {
                vertices[i * 4 + 0] = new Vector3(x * cellSize, 0, y * cellSize);
                vertices[i * 4 + 1] = new Vector3(x * cellSize + cellSize, 0, y * cellSize);
                vertices[i * 4 + 2] = new Vector3(x * cellSize, 0, y * cellSize + cellSize);
                vertices[i * 4 + 3] = new Vector3(x * cellSize + cellSize, 0, y * cellSize + cellSize);

                x++;
                if (x >= blockGridWidth)
                {
                    x = 0;
                    y++;
                }

                uvs[i * 4 + 0] = new Vector2(0, 0);
                uvs[i * 4 + 1] = new Vector2(1, 0);
                uvs[i * 4 + 2] = new Vector2(0, 1);
                uvs[i * 4 + 3] = new Vector2(1, 1);

                triangles[i * 6 + 0] = i * 4 + 0;
                triangles[i * 6 + 1] = i * 4 + 2;
                triangles[i * 6 + 2] = i * 4 + 1;
                triangles[i * 6 + 3] = i * 4 + 1;
                triangles[i * 6 + 4] = i * 4 + 2;
                triangles[i * 6 + 5] = i * 4 + 3;
            }

            gridMesh.vertices = vertices;
            gridMesh.uv = uvs;
            gridMesh.triangles = triangles;

            blockGridMesh = new GameObject("BlockGrid");
            blockGridMesh.AddComponent<MeshFilter>().mesh = gridMesh;
            blockGridMesh.AddComponent<MeshRenderer>();
            blockGridMesh.SetActive(false);
#endif
        }

        private void Start()
        {
            _nextUpdate = Common.Globals.Time;
        }

        public void UpdateMap()
        {
            if (Time.time < _nextUpdate)
                return;

            _nextUpdate += _animationSpeed;

            //Update the texture
            _waterRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(Media.MediaConstants.SHADER_MAIN_TEX_PROPERTY_ID, _waterTextures[_currentFrame]);
            _waterRenderer.SetPropertyBlock(_propertyBlock);

            _currentFrame = (_currentFrame + 1) % Common.Constants.MAX_WATER_TEXTURES;
        }

        /// <summary>
        /// Assigns Vertices vector with 4 vertices on clockwise. Starting at bottom left. Starts inserting at triangles index "startIndex"
        /// </summary>
        public void GetTileVertices(int x, int y, ref Vector3[] triangles, int startIndex = 0)
        {
            triangles[0 + startIndex] = _vertices[(x + y * MapData.Width) * 4 + 0];
            triangles[1 + startIndex] = _vertices[(x + y * MapData.Width) * 4 + 1];
            triangles[2 + startIndex] = _vertices[(x + y * MapData.Width) * 4 + 2];
            triangles[3 + startIndex] = _vertices[(x + y * MapData.Width) * 4 + 3];
        }

        /// <summary>
        /// Assigns Vertices vector with 4 vertices on clockwise. Starting at bottom left. Starts inserting at triangles index "startIndex"
        /// </summary>
        public void GetTileVertices(in Vector2Int coordinates, ref Vector3[] triangles, int startIndex = 0)
        {
            triangles[0 + startIndex] = _vertices[(coordinates.x + coordinates.y * MapData.Width) * 4 + 0];
            triangles[1 + startIndex] = _vertices[(coordinates.x + coordinates.y * MapData.Width) * 4 + 1];
            triangles[2 + startIndex] = _vertices[(coordinates.x + coordinates.y * MapData.Width) * 4 + 2];
            triangles[3 + startIndex] = _vertices[(coordinates.x + coordinates.y * MapData.Width) * 4 + 3];
        }

        /// <summary>
        /// Assigns Vertices vector while modifying only the heights. In clockwise starting at bottom left.  Starts inserting at triangles index "startIndex"
        /// </summary>
        public void GetTileVerticesHeights(int x, int y, ref Vector3[] triangles, int startIndex = 0)
        {
            triangles[0 + startIndex].y = _vertices[(x + y * MapData.Width) * 4 + 0].y;
            triangles[1 + startIndex].y = _vertices[(x + y * MapData.Width) * 4 + 1].y;
            triangles[2 + startIndex].y = _vertices[(x + y * MapData.Width) * 4 + 2].y;
            triangles[3 + startIndex].y = _vertices[(x + y * MapData.Width) * 4 + 3].y;
        }

        /// <summary>
        /// Assigns Vertices vector while modifying only the heights. In clockwise starting at bottom left.  Starts inserting at triangles index "startIndex"
        /// </summary>
        public void GetTileVerticesHeights(in Vector2Int coordinates, ref Vector3[] triangles, int startIndex = 0)
        {
            triangles[0 + startIndex].y = _vertices[(coordinates.x + coordinates.y * MapData.Width) * 4 + 0].y;
            triangles[1 + startIndex].y = _vertices[(coordinates.x + coordinates.y * MapData.Width) * 4 + 1].y;
            triangles[2 + startIndex].y = _vertices[(coordinates.x + coordinates.y * MapData.Width) * 4 + 2].y;
            triangles[3 + startIndex].y = _vertices[(coordinates.x + coordinates.y * MapData.Width) * 4 + 3].y;
        }
    }
}