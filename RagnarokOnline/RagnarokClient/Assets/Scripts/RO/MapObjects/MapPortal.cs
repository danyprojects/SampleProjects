using RO.Common;
using UnityEngine;

namespace RO.MapObjects
{
    public class MapPortal : Block
    {
        public override bool IsEnabled
        {
            get
            {
                return _warpRend.gameObject.activeInHierarchy;
            }
            set
            {
                _warpRend.gameObject.SetActive(value);
            }
        }

        private MeshRenderer _warpRend = null;
        private MeshRenderer _backgroundRend = null;
        public int FadeTag { get; private set; } = int.MinValue;

        public MapPortal(int sessionId, Vector2Int position, Transform parent)
            : base(sessionId, BlockTypes.Npc)
        {
            //instantiate the prefab
            GameObject prefab = AssetBundleProvider.LoadMiscBundleAsset<GameObject>("MapPortal");
            prefab = GameObject.Instantiate(prefab, parent, true);

            _warpRend = prefab.GetComponent<MeshRenderer>();
            _backgroundRend = prefab.transform.GetChild(0).GetComponent<MeshRenderer>();

            SetGameObject(prefab);

            //Set it's position
            Utility.GameToWorldCoordinatesCenter(position.x, position.y, out Vector3 pos);
            Physics.Raycast(pos, Vector3.down, out RaycastHit hit, Utility.RAYCAST_DISTANCE, LayerMasks.Map);
            prefab.transform.position = new Vector3(hit.point.x, hit.point.y + 0.05f, hit.point.z);
        }

        public void Move(Vector2Int position)
        {
            //Set it's position
            Utility.GameToWorldCoordinatesCenter(position.x, position.y, out Vector3 pos);
            Physics.Raycast(pos, Vector3.down, out RaycastHit hit, Utility.RAYCAST_DISTANCE, LayerMasks.Map);
            _warpRend.transform.position = new Vector3(hit.point.x, hit.point.y + 0.05f, hit.point.z); ;
        }

        public int Fade(Media.FadeDirection fadeDirection)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

            _warpRend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(Media.MediaConstants.SHADER_UNIT_FADE_START_TIME_ID, Globals.TimeSinceLevelLoad);
            propertyBlock.SetFloat(Media.MediaConstants.SHADER_UNIT_FADE_DIRECTION_ID, (float)fadeDirection);
            _warpRend.SetPropertyBlock(propertyBlock);

            _backgroundRend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(Media.MediaConstants.SHADER_UNIT_FADE_START_TIME_ID, Globals.TimeSinceLevelLoad);
            propertyBlock.SetFloat(Media.MediaConstants.SHADER_UNIT_FADE_DIRECTION_ID, (float)fadeDirection);
            _backgroundRend.SetPropertyBlock(propertyBlock);

            if (FadeTag < int.MaxValue)
                FadeTag++;
            else
                FadeTag = int.MinValue;
            return FadeTag;
        }
    }
}
