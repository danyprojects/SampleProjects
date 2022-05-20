using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class InputField : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IKeyboardHandler, IPointerClickHandler, IPointerDownHandler
    {
        public Action<string> OnSubmit;
        public Action OnTab;
        public Action OnUpArrow;
        public Action OnDownArrow;

        public string Text
        {
            get { return _textStr; }
            set
            {
                _selectedStartPosition = 0;
                _selectedEndPosition = _textStr.Length;

                if (value == null || value.Length == 0)
                    Clear();
                else
                    AppendString(value);
            }
        }

        public enum ContentType
        {
            Standard,
            IntegerNumber,
            Alphanumeric,
            Name,
            Password
        }

        [SerializeField]
        private ContentType _contentType = ContentType.Standard;

        [SerializeField]
        private Text _textComp = default;

        [SerializeField]
        private RectTransform _inputTransform = default;

        [SerializeField]
        private Color _selectionColor = new Color(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        [SerializeField]
        private Material _caretMaterial = default;

        private UIVertex[] _inputVerts = new UIVertex[4];

        private const float _caretWidth = 1;
        private const float _scrollSpeed = 0.05f;
        private Mesh _mesh;
        private CanvasRenderer _inputRenderer;
        private int _drawStart = 0;
        private int _drawEnd = 0;
        private int _caretPosition = 0;
        private int _selectedStartPosition = 0;
        private int _selectedEndPosition = 0;
        private string _textStr = "";
        private static VertexHelper _vertexHelper = new VertexHelper();
        private bool _isDragingAllowed = false;
        private Coroutine _shiftTextCoroutine;
        private bool _dragingOutOfBounds = false;
        private WaitForSecondsRealtime _waitForSecondsRealtime;
        private UIController.Panel _panel;

        private static string Clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        private void Awake()
        {
            _panel = GetComponentInParent<UIController.Panel>();

            _mesh = new Mesh();

            // create cursor verts
            for (int i = 0; i < _inputVerts.Length; i++)
            {
                _inputVerts[i] = UIVertex.simpleVert;
                _inputVerts[i].uv0 = Vector2.zero;
                _inputVerts[i].color = new Color(0, 0, 0, 0);
            }

            _inputRenderer = _inputTransform.GetComponent<CanvasRenderer>();
            _inputRenderer.SetMaterial(_caretMaterial, Texture2D.whiteTexture);
        }

        public int PrefferedWidth
        {
            get
            {
                float width = 0;
                if (ReferenceEquals(_textStr, _textComp.text))
                {
                    foreach (var _char in _textComp.cachedTextGenerator.characters)
                        width += _char.charWidth;
                }
                else // generator might not be updated
                {
                    Vector2 extents = _textComp.rectTransform.rect.size;
                    var settings = _textComp.GetGenerationSettings(extents);
                    settings.generateOutOfBounds = true;

                    width = _textComp.cachedTextGenerator.GetPreferredWidth(_textStr, settings);
                }

                return (int)width;
            }
        }

        public void OverridePanel(UIController.Panel panel)
        {
            _panel = panel;
        }

        public void Clear()
        {
            if (_mesh == null) // it hasn't initialized yet
                return;

            _textComp.text = _textStr = "";
            _drawStart = _drawEnd = _caretPosition = 0;
            _selectedStartPosition = _selectedEndPosition = 0;

            UpdateCaret(_drawStart + _caretPosition);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragingAllowed = true;
            SetHightligthColor();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragingAllowed)
                return;

            _dragingOutOfBounds = !RectTransformUtility.RectangleContainsScreenPoint(_textComp.rectTransform,
                eventData.position);

            if (_dragingOutOfBounds)
            {
                if (_shiftTextCoroutine == null)
                    _shiftTextCoroutine = StartCoroutine(ShiftTextCoroutine(eventData));
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_textComp.rectTransform, eventData.position,
                    null, out var localMousePos);

                _selectedEndPosition = GetCharacterIndexFromPosition(localMousePos) + _drawStart;
                UpdateHightlight(_selectedStartPosition - _drawStart, _selectedEndPosition - _drawStart);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragingAllowed)
            {
                _isDragingAllowed = false;
                SetCaretColor();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _panel.BringToFront();

            // for the first pointer down only trigger keyboard selected event
            if (!ReferenceEquals(EventSystem.CurrentKeyboardHandler, this))
            {
                EventSystem.CurrentKeyboardHandler = this;

                if (_textStr.Length > 0)
                    SelectAll();
                return;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(_textComp.rectTransform, eventData.position))
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_textComp.rectTransform, eventData.position,
                null, out var localMousePos);

            if (_selectedStartPosition != _selectedEndPosition)
                SetCaretColor(); // reset color on pointer down

            var pos = GetCharacterIndexFromPosition(localMousePos);
            _selectedStartPosition = _selectedEndPosition = _drawStart + pos; // in case a drag starts

            _caretPosition = pos;

            if (pos == CharacterCountVisible() && _drawEnd == _textStr.Length)
            {
                _caretPosition = RedrawTextFromEnd();
                UpdateCaret(_drawStart + _caretPosition, CalculateCachedTextOffset());
            }
            else
                UpdateCaret(pos);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                SelectAll();
            }
        }

        private void UpdateCaret(int pos, float xOffset = 0)
        {
            TextGenerator textGen = _textComp.cachedTextGenerator;

            var startX = textGen.characters[pos].cursorPos.x + xOffset / _textComp.pixelsPerUnit;
            float height = textGen.lines[0].height / _textComp.pixelsPerUnit;
            float topY = textGen.lines[0].topY - 1 / _textComp.pixelsPerUnit; // fixing height by 1 unit

            Vector2 offset = _textComp.PixelAdjustPoint(Vector2.zero);
            _inputVerts[0].position = new Vector3(startX, topY - height, 0.0f) + (Vector3)offset;
            _inputVerts[1].position = new Vector3(startX + _caretWidth, topY - height, 0.0f) + (Vector3)offset;
            _inputVerts[2].position = new Vector3(startX + _caretWidth, topY, 0.0f) + (Vector3)offset;
            _inputVerts[3].position = new Vector3(startX, topY, 0.0f) + (Vector3)offset;

            _vertexHelper.Clear();
            _vertexHelper.AddUIVertexQuad(_inputVerts);
            _vertexHelper.FillMesh(_mesh);
            _inputRenderer.SetMesh(_mesh);
        }

        private void UpdateHightlight(int startPos, int endPos, float xOffset = 0)
        {
            TextGenerator textGen = _textComp.cachedTextGenerator;

            var startX = textGen.characters[startPos].cursorPos.x + xOffset / _textComp.pixelsPerUnit;
            var endX = textGen.characters[endPos].cursorPos.x + xOffset / _textComp.pixelsPerUnit;

            float height = textGen.lines[0].height / _textComp.pixelsPerUnit;
            float topY = textGen.lines[0].topY - 1 / _textComp.pixelsPerUnit; // fixing height by 1 unit

            Vector2 offset = _textComp.PixelAdjustPoint(Vector2.zero);
            _inputVerts[0].position = new Vector3(startX, topY - height, 0.0f) + (Vector3)offset;
            _inputVerts[1].position = new Vector3(endX, topY - height, 0.0f) + (Vector3)offset;
            _inputVerts[2].position = new Vector3(endX, topY, 0.0f) + (Vector3)offset;
            _inputVerts[3].position = new Vector3(startX, topY, 0.0f) + (Vector3)offset;

            _vertexHelper.Clear();
            _vertexHelper.AddUIVertexQuad(_inputVerts);
            _vertexHelper.FillMesh(_mesh);
            _inputRenderer.SetMesh(_mesh);
        }

        private void SetHightligthColor()
        {
            for (int i = 0; i < _inputVerts.Length; i++)
                _inputVerts[i].color = _selectionColor;
        }

        private void SetCaretColor()
        {
            for (int i = 0; i < _inputVerts.Length; i++)
                _inputVerts[i].color = new Color(0, 0, 0, 0);
        }

        private int GetCharacterIndexFromPosition(Vector2 pos)
        {
            TextGenerator gen = _textComp.cachedTextGenerator;

            for (int i = 0; i < gen.characterCountVisible; i++)
            {
                UICharInfo charInfo = gen.characters[i];
                Vector2 charPos = charInfo.cursorPos / _textComp.pixelsPerUnit;

                float distToCharStart = pos.x - charPos.x;
                float distToCharEnd = charPos.x + (charInfo.charWidth / _textComp.pixelsPerUnit) - pos.x;
                if (distToCharStart < distToCharEnd)
                    return i;
            }

            return gen.characterCountVisible;
        }

        private int CharacterCountVisible()
        {
            return _textComp.cachedTextGenerator.characterCountVisible;
        }

        private float CalculateCachedTextOffset()
        {
            var chars = _textComp.cachedTextGenerator.characters;
            return chars[0].cursorPos.x - chars[_drawStart].cursorPos.x;
        }

        private int ClampSelectedPos()
        {
            return Mathf.Clamp(_selectedStartPosition, _drawStart, _drawEnd);
        }

        private string GetSelectedString()
        {
            if (_selectedEndPosition == _selectedStartPosition)
                return "";

            if (_selectedStartPosition > _selectedEndPosition)
                return _textStr.Substring(_selectedEndPosition, _selectedStartPosition - _selectedEndPosition);
            else
                return _textStr.Substring(_selectedStartPosition, _selectedEndPosition - _selectedStartPosition);
        }

        private IEnumerator ShiftTextCoroutine(PointerEventData eventData)
        {
            while (_isDragingAllowed && _dragingOutOfBounds)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_textComp.rectTransform,
                    eventData.position, null, out var localMousePos);

                Rect rect = _textComp.rectTransform.rect;

                if (localMousePos.x < rect.xMin)
                {
                    if (_drawStart > 0)
                    {
                        _selectedEndPosition = --_drawStart;
                        RedrawTextFromStart();

                        UpdateHightlight(ClampSelectedPos(), _selectedEndPosition, CalculateCachedTextOffset());
                    }
                    else if (_selectedEndPosition - _drawStart != 0)
                    {
                        _selectedEndPosition = _drawStart;
                        UpdateHightlight(ClampSelectedPos() - _drawStart, _selectedEndPosition - _drawStart);
                    }

                }
                else if (localMousePos.x > rect.xMax)
                {
                    var last = CharacterCountVisible();

                    if (_drawStart + last < _textStr.Length)
                    {
                        _selectedEndPosition = ++_drawEnd;
                        RedrawTextFromEnd();

                        UpdateHightlight(ClampSelectedPos(), _selectedEndPosition, CalculateCachedTextOffset());
                    }
                    else if (_selectedEndPosition - _drawStart != last)
                    {
                        _selectedEndPosition = _drawStart + last;
                        UpdateHightlight(ClampSelectedPos() - _drawStart, _selectedEndPosition - _drawStart);
                    }
                }

                if (_waitForSecondsRealtime == null)
                    _waitForSecondsRealtime = new WaitForSecondsRealtime(_scrollSpeed);
                else
                    _waitForSecondsRealtime.waitTime = _scrollSpeed;
                yield return _waitForSecondsRealtime;
            }
            _shiftTextCoroutine = null;
        }

        // Returns new value of visible chars
        private int RedrawTextFromStart()
        {
            Vector2 extents = _textComp.rectTransform.rect.size;

            var settings = _textComp.GetGenerationSettings(extents);
            settings.generateOutOfBounds = true;

            var text = _contentType == ContentType.Password ?
                 new string('\u25CF', _textStr.Length) : _textStr;

            var gen = _textComp.cachedTextGenerator;
            gen.PopulateWithErrors(text, settings, gameObject);

            float width = 0.0f;
            for (_drawEnd = _drawStart; _drawEnd <= text.Length;)
            {
                width += gen.characters[_drawEnd].charWidth;
                _drawEnd++;
                if (width > extents.x)
                    break;
            }
            _drawEnd--;

            var len = _drawEnd - _drawStart;
            _textComp.text = text.Substring(_drawStart, len);

            return len;
        }

        // Returns new value of visible chars
        private int RedrawTextFromEnd()
        {
            Vector2 extents = _textComp.rectTransform.rect.size;

            var settings = _textComp.GetGenerationSettings(extents);
            settings.generateOutOfBounds = true;

            var text = _contentType == ContentType.Password ?
                 new string('\u25CF', _textStr.Length) : _textStr;

            var gen = _textComp.cachedTextGenerator;
            gen.PopulateWithErrors(text, settings, gameObject);

            float width = 0.0f;
            for (_drawStart = _drawEnd; _drawStart >= 0;)
            {
                width += gen.characters[_drawStart].charWidth;
                _drawStart--;
                if (width > extents.x)
                {
                    if (_drawEnd == text.Length)
                        _drawStart++; // for some reason we are off by one here
                    break;
                }
            }
            _drawStart++;

            var len = _drawEnd - _drawStart;
            _textComp.text = text.Substring(_drawStart, len);

            return len;
        }

        private void MoveLeft()
        {
            var visibleCount = CharacterCountVisible();

            if (_isDragingAllowed || _selectedStartPosition != _selectedEndPosition) // cancel it
            {
                var pos = Math.Min(_selectedStartPosition, _selectedEndPosition) - _drawStart;
                _caretPosition = Mathf.Clamp(pos, 0, visibleCount);

                _isDragingAllowed = false;
                _selectedStartPosition = _selectedEndPosition = 0;

                UpdateCaret(_caretPosition);
                return;
            }

            if (_caretPosition > 0)
            {
                _caretPosition--;
                UpdateCaret(_caretPosition);
            }
            else if (_drawStart > 0)
            {
                _drawStart--;
                RedrawTextFromStart();
            }
        }

        private void MoveRight()
        {
            var visibleCount = CharacterCountVisible();

            if (_isDragingAllowed || _selectedStartPosition != _selectedEndPosition) // cancel it
            {
                var pos = Math.Max(_selectedStartPosition, _selectedEndPosition) - _drawStart;
                _caretPosition = Mathf.Clamp(pos, 0, visibleCount);

                _isDragingAllowed = false;
                _selectedStartPosition = _selectedEndPosition = 0;

                UpdateCaret(_caretPosition);
                return;
            }

            if (_caretPosition < visibleCount)
            {
                _caretPosition++;

                if (_caretPosition == visibleCount && _drawEnd == _textStr.Length)
                {
                    _caretPosition = RedrawTextFromEnd();
                    UpdateCaret(_drawStart + _caretPosition, CalculateCachedTextOffset());
                }
                else
                    UpdateCaret(_caretPosition);
            }
            else if (_drawStart + visibleCount < _textStr.Length)
            {
                _drawEnd++;
                _caretPosition = RedrawTextFromEnd();
                UpdateCaret(_drawStart + _caretPosition, CalculateCachedTextOffset());
            }
        }

        // This does NOT redraw
        private void DeleteHighlighted()
        {
            //we don't need them after this so its safe to swap them here
            if (_selectedStartPosition > _selectedEndPosition)
            {
                var tmp = _selectedStartPosition;
                _selectedStartPosition = _selectedEndPosition;
                _selectedEndPosition = tmp;
            }

            // make sure we next start drawing from a correct point
            var caretPosFull = _drawStart + _caretPosition;
            caretPosFull = Math.Min(caretPosFull, _selectedStartPosition);
            _drawStart = Math.Min(_drawStart, _selectedStartPosition);

            _textStr = _textStr.Remove(_selectedStartPosition, _selectedEndPosition - _selectedStartPosition);

            _selectedStartPosition = _selectedEndPosition = 0;
            _caretPosition = Mathf.Max(caretPosFull - _drawStart, 0);
        }

        public void SelectAll()
        {
            _isDragingAllowed = false;

            _caretPosition = _selectedEndPosition = _drawStart = 0;
            _selectedStartPosition = _textStr.Length;

            SetHightligthColor();
            RedrawTextFromStart();
            UpdateHightlight(ClampSelectedPos(), _selectedEndPosition, CalculateCachedTextOffset());
        }

        private void ForwardSpace()
        {
            _isDragingAllowed = false;

            if (_selectedStartPosition != _selectedEndPosition)
            {
                DeleteHighlighted();
                RedrawTextFromStart();
                UpdateCaret(_drawStart + _caretPosition, CalculateCachedTextOffset());
            }
            else
            {
                var visible = CharacterCountVisible();
                if (_caretPosition < visible)
                {
                    _textStr = _textStr.Remove(_caretPosition + _drawStart, 1);
                    if (_caretPosition + 1 == visible)
                    {
                        _drawEnd = _caretPosition + _drawStart; // no +1 since we just deleted 1 char
                        _caretPosition = RedrawTextFromEnd();
                        UpdateCaret(_drawStart + _caretPosition, CalculateCachedTextOffset());
                    }
                    else
                        RedrawTextFromStart();
                }
            }
        }

        private void Backspace()
        {
            _isDragingAllowed = false;

            if (_selectedStartPosition != _selectedEndPosition)
            {
                DeleteHighlighted();
                RedrawTextFromStart();
                UpdateCaret(_drawStart + _caretPosition, CalculateCachedTextOffset());
            }
            else if (_caretPosition > 0)
            {
                _textStr = _textStr.Remove(_caretPosition + _drawStart - 1, 1);

                if (_caretPosition == CharacterCountVisible()) // caret at end
                {
                    _drawEnd--;
                    _caretPosition = RedrawTextFromEnd();
                }
                else
                {
                    RedrawTextFromStart();
                    _caretPosition--;
                }
                UpdateCaret(_caretPosition + _drawStart, CalculateCachedTextOffset());
            }
        }

        private void Home()
        {
            _isDragingAllowed = false;
            _caretPosition = _selectedStartPosition = _selectedEndPosition = 0;

            if (_drawStart > 0)
            {
                _drawStart = 0;
                RedrawTextFromStart();
                UpdateCaret(_caretPosition + _drawStart, CalculateCachedTextOffset());
            }
            else
                UpdateCaret(_caretPosition);
        }

        private void End()
        {
            _isDragingAllowed = false;
            _selectedStartPosition = _selectedEndPosition = 0;

            if (_drawEnd < _textStr.Length - 1)
            {
                _drawEnd = _textStr.Length - 1;
                _caretPosition = RedrawTextFromEnd();
                UpdateCaret(_caretPosition + _drawStart, CalculateCachedTextOffset());
            }
            else
            {
                _caretPosition = CharacterCountVisible();
                UpdateCaret(_caretPosition);
            }
        }

        private void AppendString(string value)
        {
            if (value.Length == 0)
                return;

            _isDragingAllowed = false;
            bool deleted = false;

            if (_selectedStartPosition != _selectedEndPosition)
            {
                DeleteHighlighted();
                deleted = true;
            }

            _textStr = _textStr.Insert(_caretPosition + _drawStart, value);

            if (!deleted && _caretPosition == CharacterCountVisible()) // caret is at end
            {
                _drawEnd += value.Length;
                _caretPosition = RedrawTextFromEnd();
            }
            else
            {
                RedrawTextFromStart();
            }

            UpdateCaret(_caretPosition + _drawStart, CalculateCachedTextOffset());
        }

        private void AppendChar(char c)
        {
            _isDragingAllowed = false;
            bool deleted = false;

            if (_selectedStartPosition != _selectedEndPosition)
            {
                DeleteHighlighted();
                deleted = true;
            }

            _textStr = _textStr.Insert(_caretPosition + _drawStart, char.ToString(c));

            if (!deleted && _caretPosition == CharacterCountVisible()) // caret is at end
            {
                _drawEnd++;
                _caretPosition = RedrawTextFromEnd();
            }
            else
            {
                var len = RedrawTextFromStart();
                _caretPosition = Math.Min(len, _caretPosition + 1);
            }

            UpdateCaret(_caretPosition + _drawStart, CalculateCachedTextOffset());
        }

        private bool ValidateChar(char c)
        {
            // Don't allow return chars or tabulator key to be entered into single line fields.
            if (c == '\t' || c == '\r' || c == '\n' || c == 10 || c == 127 || c == 3)
                return false;

            if (char.IsSurrogate(c))
                return false;

            if (!_textComp.font.HasCharacter(c))
                return false;

            switch (_contentType)
            {
                case ContentType.Alphanumeric:
                    return (c >= 'A' && c <= 'Z') ||
                           (c >= 'a' && c <= 'z') ||
                           (c >= '0' && c <= '9');
                case ContentType.IntegerNumber:
                    return c >= '0' && c <= '9';
                case ContentType.Name:
                    if (c == ' ')
                    {
                        if (_caretPosition + _drawStart == 0)
                            return false;
                        if (_textStr[_caretPosition + _drawStart] == ' ')
                            return false;
                        return true;
                    }
                    return (c >= 'A' && c <= 'Z') ||
                           (c >= 'a' && c <= 'z') ||
                           (c >= '0' && c <= '9');
            }

            return true;
        }

        public void OnKeyDown(Event evt)
        {
            var currentEventModifiers = evt.modifiers;
            bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
            bool shift = (currentEventModifiers & EventModifiers.Shift) != 0;
            bool alt = (currentEventModifiers & EventModifiers.Alt) != 0;
            bool ctrlOnly = ctrl && !alt && !shift;

            SetCaretColor();

            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                    Backspace();
                    return;
                case KeyCode.Delete:
                    ForwardSpace();
                    return;
                case KeyCode.LeftArrow:
                    MoveLeft();
                    return;
                case KeyCode.RightArrow:
                    MoveRight();
                    return;
                case KeyCode.Home:
                    Home();
                    return;
                case KeyCode.End:
                    End();
                    return;
                case KeyCode.A:
                    if (ctrlOnly)
                    {
                        SelectAll();
                        return;
                    }
                    break;
                case KeyCode.C:
                    if (ctrlOnly)
                    {
                        Clipboard = _contentType != ContentType.Password ? GetSelectedString() : "";
                        return;
                    }
                    break;
                case KeyCode.V:
                    if (ctrlOnly)
                    {
                        AppendString(Clipboard);
                        return;
                    }
                    break;
                case KeyCode.X:
                    if (ctrlOnly)
                    {
                        Clipboard = _contentType != ContentType.Password ? GetSelectedString() : "";
                        DeleteHighlighted();
                        RedrawTextFromStart();
                        UpdateCaret(_drawStart + _caretPosition, CalculateCachedTextOffset());
                        return;
                    }
                    break;
                case KeyCode.Tab:
                    OnTab?.Invoke();
                    return;
                case KeyCode.DownArrow:
                    OnDownArrow?.Invoke();
                    return;
                case KeyCode.UpArrow:
                    OnUpArrow?.Invoke();
                    return;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (_contentType == ContentType.Name)
                        _textStr = _textStr.TrimEnd(' ');
                    OnSubmit?.Invoke(_textStr);
                    return;
            }

            if (ValidateChar(evt.character))
                AppendChar(evt.character);
        }

        private IEnumerator DelayedKeyboardFocusCoroutine()
        {
            yield return 0;

            UpdateCaret(_caretPosition);
        }

        private IEnumerator DelayedDeactivate()
        {
            yield return 0;
            gameObject.SetActive(false);
        }

        public void InitAndDeactivate()
        {
            gameObject.SetActive(true);
            StartCoroutine(DelayedDeactivate());
        }

        public void OnKeyboardFocus(bool hasFocus)
        {
            _inputTransform.gameObject.SetActive(hasFocus);

            if (hasFocus)
            {
                // skip 1 frame if it hasn't initialized yet
                if (_textComp.cachedTextGenerator.characterCountVisible >= 0)
                {
                    SetCaretColor(); // reset color
                    UpdateCaret(_caretPosition);
                    _selectedStartPosition = _selectedEndPosition = 0;
                }
                else
                    StartCoroutine(DelayedKeyboardFocusCoroutine());
            }
        }
    }
}