using RO.Common;
using UnityEngine;

namespace RO.Media
{
    public class CastLockOnAnimator : MonoBehaviour
    {
        private MeshRenderer _meshRenderer = null;

        public void Animate()
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            _meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetVector(MediaConstants.SHADER_START_TIME_ID, new Vector4(Globals.TimeSinceLevelLoad, 0, 0, 0));
            _meshRenderer.SetPropertyBlock(propertyBlock);
        }

        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
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
