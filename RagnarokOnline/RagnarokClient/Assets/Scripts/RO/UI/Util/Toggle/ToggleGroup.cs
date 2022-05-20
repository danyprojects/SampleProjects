using UnityEngine;
using UnityEngine.Serialization;

namespace RO.UI
{
    public class ToggleGroup : MonoBehaviour
    {
        public ToggleBase SelectedToggle { get; private set; }

        public abstract class ToggleBase : MonoBehaviour
        {
            [SerializeField]
            [FormerlySerializedAs("Toggle Group")]
            protected ToggleGroup _group = default;

            protected void SetAsInitialToogle()
            {
                _group.SelectedToggle = this;
            }

            protected void SetAsSelectedToogle()
            {
                var toggle = _group.SelectedToggle;

                _group.SelectedToggle = this;
                toggle.SetValue(false);
            }

            public abstract void SetValue(bool value);
        }
    }
}
