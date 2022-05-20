using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public partial class ChatPanelController : UIController.Panel
    {
        public class ChatMessage : Graphic, IPointerClickHandler
        {
            [SerializeField]
            private ChatPanelController _controller = default;

            [SerializeField]
            private FontData _fontData = FontData.defaultFontData;

            private UIVertex[] _tempVerts = new UIVertex[4];
            private TextGenerator _textGenerator = new TextGenerator();
            private static TextGenerator _sharedTextGenerator = new TextGenerator();
            private static TextGenerationSettings _settings;

            private int _chatIndex;
            private int _msgIndex;

            private static readonly Color[] defaultColors;

            static ChatMessage()
            {
                defaultColors = new Color[Enum.GetValues(typeof(MsgType)).Length];

                defaultColors[(int)MsgType.Normal] = Color.green;
            }

            public void Fill(int index)
            {
                _msgIndex = index;
                var msgData = _controller.GetMessageData(_chatIndex, index);
                color = defaultColors[(int)msgData.type];

                UpdateText(msgData.text);
                SetVerticesDirty();
            }

            public float PreCalculateHeight(int index)
            {
                if (_settings.font == null)
                    CreateSettings();

                var msgData = _controller.GetMessageData(_chatIndex, index);
                _settings.generationExtents = rectTransform.rect.size;

                return _sharedTextGenerator.GetPreferredHeight(msgData.text, _settings);
            }

            public int LineCount()
            {
                return _textGenerator.lineCount;
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.button != PointerEventData.InputButton.Right)
                    return;

                // position is being masked
                if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform.parent, eventData.position))
                    return;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position,
                    null, out var localMousePos);

                var charIndex = GetCharacterIndexFromPosition(localMousePos);
                if (charIndex < 0)
                    return;

                // TODO:: hit chatpanel with the index
            }

            public override Texture mainTexture
            {
                get
                {
                    var font = _fontData.font;

                    if (font.material != null && font.material.mainTexture != null)
                        return font.material.mainTexture;

                    if (m_Material != null)
                        return m_Material.mainTexture;

                    return base.mainTexture;
                }
            }

            public UI.ChatMessage Clone(int chatIndex)
            {
                var msg = Instantiate((UI.ChatMessage)this, transform.parent);
                msg._chatIndex = chatIndex;

                return msg;
            }

            protected override void OnPopulateMesh(VertexHelper toFill)
            {
                // Apply the offset to the vertices
                var verts = _textGenerator.verts;
                float unitsPerPixel = 1 / PixelsPerUnit();
                int vertCount = verts.Count;

                Debug.Assert(vertCount > 0);

                Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
                roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
                toFill.Clear();

                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    var vert = _tempVerts[tempVertsIndex] = verts[i];

                    vert.position *= unitsPerPixel;
                    vert.position.x += roundingOffset.x;
                    vert.position.y += roundingOffset.y;

                    if (tempVertsIndex == 3)
                    {
                        var val = _tempVerts[0].uv0.x == _tempVerts[1].uv0.x ? -Vector2.one : Vector2.one;
                        _tempVerts[0].uv1 = _tempVerts[1].uv1 = _tempVerts[2].uv1 = _tempVerts[3].uv1 = val;

                        toFill.AddUIVertexQuad(_tempVerts);
                    }
                }
            }

            private float PixelsPerUnit()
            {
                var localCanvas = canvas;
                if (!localCanvas)
                    return 1;

                // For dynamic fonts, ensure we use one pixel per pixel on the screen.
                if (!_fontData.font || _fontData.font.dynamic)
                    return localCanvas.scaleFactor;

                // For non-dynamic fonts, calculate pixels per unit based on specified font size relative to font object's own font size.
                if (_fontData.fontSize <= 0 || _fontData.font.fontSize <= 0)
                    return 1;

                return _fontData.font.fontSize / (float)_fontData.fontSize;
            }

            private TextGenerationSettings CreateSettings()
            {
                _settings = new TextGenerationSettings();

                if (_fontData != null && _fontData.font.dynamic)
                {
                    _settings.fontSize = _fontData.fontSize;
                    _settings.resizeTextMinSize = _fontData.minSize;
                    _settings.resizeTextMaxSize = _fontData.maxSize;
                }

                // Other settings
                _settings.generationExtents = rectTransform.rect.size;
                _settings.textAnchor = _fontData.alignment;
                _settings.alignByGeometry = _fontData.alignByGeometry;
                _settings.scaleFactor = PixelsPerUnit();
                _settings.color = color;
                _settings.font = _fontData.font;
                _settings.pivot = rectTransform.pivot;
                _settings.richText = _fontData.richText;
                _settings.lineSpacing = _fontData.lineSpacing;
                _settings.fontStyle = _fontData.fontStyle;
                _settings.resizeTextForBestFit = _fontData.bestFit;
                _settings.updateBounds = false;
                _settings.horizontalOverflow = _fontData.horizontalOverflow;
                _settings.verticalOverflow = _fontData.verticalOverflow;

                return _settings;
            }

            private void UpdateText(string text)
            {
                if (_settings.font == null)
                    CreateSettings();

                _settings.generationExtents = rectTransform.rect.size;

                var height = _textGenerator.GetPreferredHeight(text, _settings);

                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
                _settings.generationExtents = new Vector2(rectTransform.rect.width, height);

                _textGenerator.PopulateWithErrors(text, _settings, gameObject);
            }

            private int GetUnclampedCharacterLineFromPosition(Vector2 pos)
            {
                TextGenerator gen = _textGenerator;

                // transform y to local scale
                float y = pos.y * PixelsPerUnit();
                float lastBottomY = 0.0f;

                for (int i = 0; i < gen.lineCount; ++i)
                {
                    float topY = gen.lines[i].topY;
                    float bottomY = topY - gen.lines[i].height;

                    // pos is somewhere in the leading above this line
                    if (y > topY)
                    {
                        // determine which line we're closer to
                        float leading = topY - lastBottomY;
                        if (y > topY - 0.5f * leading)
                            return i - 1;
                        else
                            return i;
                    }

                    if (y > bottomY)
                        return i;

                    lastBottomY = bottomY;
                }

                // Position is after last line.
                return gen.lineCount;
            }

            private static int GetLineEndPosition(TextGenerator gen, int line)
            {
                line = Mathf.Max(line, 0);
                if (line + 1 < gen.lines.Count)
                    return gen.lines[line + 1].startCharIdx - 1;
                return gen.characterCountVisible;
            }

            private int GetCharacterIndexFromPosition(Vector2 pos)
            {
                TextGenerator gen = _textGenerator;

                int line = GetUnclampedCharacterLineFromPosition(pos);
                if (line < 0 || line >= gen.lineCount)
                    return -1;

                int startCharIndex = gen.lines[line].startCharIdx;
                int endCharIndex = GetLineEndPosition(gen, line);

                var pixelsPerUnit = PixelsPerUnit();
                for (int i = startCharIndex; i < endCharIndex; i++)
                {
                    if (i >= gen.characterCountVisible)
                        break;

                    UICharInfo charInfo = gen.characters[i];
                    Vector2 charPos = charInfo.cursorPos / pixelsPerUnit;

                    float distToCharStart = pos.x - charPos.x;
                    float distToCharEnd = charPos.x + (charInfo.charWidth / pixelsPerUnit) - pos.x;
                    if (distToCharStart < distToCharEnd)
                        return i;
                }

                return endCharIndex;
            }
        }
    }

    public class ChatMessage : ChatPanelController.ChatMessage { }
}