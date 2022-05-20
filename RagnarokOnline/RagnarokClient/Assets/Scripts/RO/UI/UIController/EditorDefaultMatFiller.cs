using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed partial class UIController : MonoBehaviour
    {
        [ExecuteInEditMode]
        public class EditorDefaultMatFiller : MonoBehaviour
        {
            [SerializeField]
            private UIController _controller = default;

            private void OnEnable()
            {
                _controller._uiMaterials[0] = Graphic.defaultGraphicMaterial;
            }
        }
    }

    [ExecuteInEditMode]
    public class EditorDefaultMatFiller : UIController.EditorDefaultMatFiller { }
}