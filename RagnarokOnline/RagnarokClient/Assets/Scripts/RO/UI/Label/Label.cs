using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class Label : Text
    {
        public const float DefaultHeight = 20;
        public const float MaxWidthBeforeOverflow = 190;

        private static readonly UIVertex[] _tempVerts = new UIVertex[4];

        public void SetText(string text)
        {
            SetText(text, MaxWidthBeforeOverflow);
        }

        public void SetText(string text, float maxWidthBeforeOverflow)
        {
            this.text = text;

            var textTransform = (RectTransform)transform;
            textTransform.sizeDelta = new Vector2(maxWidthBeforeOverflow, textTransform.sizeDelta.y);

            LayoutRebuilder.ForceRebuildLayoutImmediate(textTransform);

            var lineCount = cachedTextGenerator.lineCount;

            var newSize = new Vector2(12, 5); // paddings
            newSize.x += lineCount > 1 ? maxWidthBeforeOverflow : preferredWidth;
            newSize.y += preferredHeight;

            ((RectTransform)transform.parent).sizeDelta = newSize;
        }

        // Overriding instead of the text populate mesh since we want to fix the bug with the w reversed uv's
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (font == null)
                return;

            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

            // Apply the offset to the vertices
            var verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            int vertCount = verts.Count;

            // We have no verts to process just return (case 1037923)
            if (vertCount <= 0)
            {
                toFill.Clear();
                return;
            }

            Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
            roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
            toFill.Clear();
            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    _tempVerts[tempVertsIndex] = verts[i];
                    _tempVerts[tempVertsIndex].position *= unitsPerPixel;
                    _tempVerts[tempVertsIndex].position.x += roundingOffset.x;
                    _tempVerts[tempVertsIndex].position.y += roundingOffset.y;
                    if (tempVertsIndex == 3)
                    {
                        var val = _tempVerts[0].uv0.x == _tempVerts[1].uv0.x ? -Vector2.one : Vector2.one;
                        _tempVerts[0].uv1 = _tempVerts[1].uv1 = _tempVerts[2].uv1 = _tempVerts[3].uv1 = val;
                        toFill.AddUIVertexQuad(_tempVerts);
                    }
                }
            }
            else
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    _tempVerts[tempVertsIndex] = verts[i];
                    _tempVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                    {
                        var val = _tempVerts[0].uv0.x == _tempVerts[1].uv0.x ? -Vector2.one : Vector2.one;
                        _tempVerts[0].uv1 = _tempVerts[1].uv1 = _tempVerts[2].uv1 = _tempVerts[3].uv1 = val;
                        toFill.AddUIVertexQuad(_tempVerts);
                    }
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
        }

        public static Label Instantiate(Transform parent)
        {
            var panel = AssetBundleProvider.LoadUiBundleAsset<GameObject>("Label");
            panel = Instantiate(panel, parent, false);

            return panel.GetComponentInChildren<Label>();
        }
    }
}