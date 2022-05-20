using RO.Common;
using RO.Containers;
using UnityEngine;
using UnityEngine.UI;

namespace RO.Media
{
    public class ItemAnimator : MonoBehaviour
    {
        private const int NORMAL_MATERIAL_INDEX = 0;
        private const int ZWRITE_MATERIAL_INDEX = 1;

        [SerializeField] private Transform _iconTransform = null;
        private MeshRenderer _meshRenderer = null;
        private BoxCollider _collider = null;
        private MaterialPropertyBlock _propertyBlock = null;
        public MapObjects.Item ItemInstance { get; private set; }

        public void AnimateItem(ItemData itemData, MapObjects.Item itemInstance)
        {
            ItemInstance = itemInstance;

            //Apply icon offset
            _iconTransform.localPosition = new Vector3(0, itemData.icon.height / Constants.PIXELS_PER_UNIT / 2, 0.05f);

            //Update the renderer material
            _meshRenderer.GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, itemData.icon);
            _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, itemData.palette);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(itemData.icon.width, itemData.icon.height));
            _propertyBlock.SetVector(MediaConstants.SHADER_START_TIME_ID, new Vector2(Globals.TimeSinceLevelLoad, 0));
            _meshRenderer.SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);

            //Update the zwrite material
            _meshRenderer.GetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, itemData.icon);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(itemData.icon.width, itemData.icon.height));
            _propertyBlock.SetVector(MediaConstants.SHADER_START_TIME_ID, new Vector2(Globals.TimeSinceLevelLoad, 0));
            _meshRenderer.SetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);

            //Update the collider
            Vector2 colliderSize;
            colliderSize.x = Mathf.Abs(itemData.icon.width / Constants.PIXELS_PER_UNIT);
            colliderSize.y = Mathf.Abs(itemData.icon.height / Constants.PIXELS_PER_UNIT);
            _collider.size = colliderSize;
        }

        public void SetPalette(Texture palette)
        {
            _meshRenderer.GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, palette);
            _meshRenderer.SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);

            _meshRenderer.GetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, palette);
            _meshRenderer.SetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);            
        }

        private void Awake()
        {
            _meshRenderer = _iconTransform.GetComponent<MeshRenderer>();
            _collider = _iconTransform.GetComponent<BoxCollider>();
            _propertyBlock = new MaterialPropertyBlock();
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
