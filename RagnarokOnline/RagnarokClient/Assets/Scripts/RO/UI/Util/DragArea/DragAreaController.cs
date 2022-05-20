using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    using MaterialType = UIController.Panel.MaterialType;

    public partial class DragAreaController : MonoBehaviour
    {
        [SerializeField]
        private CanvasFilter _dragFilterFlag = CanvasFilter.DragPanel;
        private UIController.Panel _panel;
        private RectTransform _rootRectTransform;
        private CanvasGroup _canvasGroup;
        private List<RendererEntry> _canvasRendererEntries;
        private bool _isDraging = false;

        private struct RendererEntry
        {
            public CanvasRenderer renderer;
            public MaterialType materialType;
        }

        void Start()
        {
            _panel = GetComponentInParent<UIController.Panel>();
            if (_panel == null)
                throw new MissingFieldException(nameof(_panel));

            _canvasGroup = GetComponentInParent<CanvasGroup>();
            if (_canvasGroup == null)
                throw new MissingFieldException(nameof(_canvasGroup));

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                throw new MissingFieldException(nameof(canvas));

            // Could be redundant later if every panel has a canvas
            _rootRectTransform = canvas.isRootCanvas ? transform as RectTransform : canvas.transform as RectTransform;

            var graphics = GetComponentsInChildren<Graphic>(true);
            _canvasRendererEntries = new List<RendererEntry>(graphics.Length);

            foreach (var graphic in graphics)
            {
                RendererEntry entry;
                entry.renderer = graphic.canvasRenderer;
                entry.materialType = GetGraphicMaterialType(graphic);

                _canvasRendererEntries.Add(entry);
            }
        }

        // call these every time a new component enters the panel
        // make sure these are Never called in mid of a drag
        public void AddGraphic(Graphic graphic)
        {
            var renderer = graphic.canvasRenderer;
            if (_canvasRendererEntries.Exists(x => x.renderer == renderer))
                return;

            RendererEntry entry;
            entry.renderer = renderer;
            entry.materialType = GetGraphicMaterialType(graphic);

            _canvasRendererEntries.Add(entry);
        }

        // Make sure these are NEVER called in mid of a drag
        public void RemoveGraphic(Graphic graphic)
        {
            _canvasRendererEntries.RemoveAll(entry => entry.renderer == graphic.canvasRenderer);
        }

        private MaterialType GetGraphicMaterialType(Graphic graphic)
        {
            var mat = graphic.material;
            if (mat == null)
                throw new NullReferenceException(nameof(graphic.material));

            foreach (MaterialType type in Enum.GetValues(typeof(MaterialType)))
            {
                if (mat.name.Equals(_panel.GetMaterial(type).name))
                    return type;
                if (mat.name.Equals(_panel.GetTransparentMaterial(type).name))
                    return type;
            }

            throw new KeyNotFoundException(mat.name);
        }

        private void DragBegin()
        {
            _isDraging = true;
            UIController.Panel.SetCanvasFilterFlags(_dragFilterFlag);

            foreach (var entry in _canvasRendererEntries)
            {
                for (int i = 0; i < entry.renderer.materialCount; i++)
                {
                    entry.renderer.SetMaterial(_panel.GetTransparentMaterial(entry.materialType), i);
                }
            }

            _canvasGroup.alpha = 0.5f;

            // Could be redundant later if every panel has a canvas
            _rootRectTransform.SetAsLastSibling();
        }

        private void DragEnd()
        {
            foreach (var entry in _canvasRendererEntries)
            {
                for (int i = 0; i < entry.renderer.materialCount; i++)
                {
                    entry.renderer.SetMaterial(_panel.GetMaterial(entry.materialType), i);
                }
            }

            _canvasGroup.alpha = 1f;

            _isDraging = false;

            UIController.Panel.ClearCanvasFilterFlags(_dragFilterFlag);
        }
    }
}