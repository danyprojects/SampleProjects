using System.Collections.Generic;
using UnityEngine;

namespace RO.UI
{
    public class PointerEventData
    {
        public enum InputButton
        {
            Left = 0,
            Right = 1,
            Middle = 2
        }

        // The object that received 'OnPointerEnter'.
        public GameObject pointerEnter;

        // The object that received OnPointerDown
        private GameObject m_PointerPress;

        // The raw GameObject for the last press event. 
        // This means that it is the 'pressed' GameObject even if it can not receive the press event itself.
        public GameObject lastPress { get; private set; }

        // The object that the press happened on even if it can not handle the press event.
        public GameObject rawPointerPress { get; set; }

        // The object that is receiving 'OnDrag'.
        public GameObject pointerDrag { get; set; }

        // RaycastResult associated with the current event.
        public RaycastResult pointerCurrentRaycast { get; set; }

        // RaycastResult associated with the pointer press.
        public RaycastResult pointerPressRaycast { get; set; }

        public List<GameObject> hovered = new List<GameObject>();

        // Is it possible to click this frame
        public bool eligibleForClick { get; set; }

        // Current pointer position.
        public Vector2 position { get; set; }

        // Pointer delta since last update.
        public Vector2 delta { get; set; }

        // Position of the press.
        public Vector2 pressPosition { get; set; }

        // The last time a click event was sent. Used for double click
        public float clickTime { get; set; }

        // Number of clicks in a row.  
        public int clickCount { get; set; }

        // The amount of scroll since the last update.
        public Vector2 scrollDelta { get; set; }

        // Should a drag threshold be used?
        // If you do not want a drag threshold set this to false in IInitializePotentialDragHandler.OnInitializePotentialDrag.
        public bool useDragThreshold { get; set; }

        // Is a drag operation currently occuring.
        public bool dragging { get; set; }

        // The EventSystem.PointerEventData.InputButton for this event.
        public InputButton button { get; set; }

        public PointerEventData()
        {
            eligibleForClick = false;

            position = Vector2.zero; // Current position of the mouse or touch event
            delta = Vector2.zero; // Delta since last update
            pressPosition = Vector2.zero; // Delta since the event started being tracked
            clickTime = 0.0f; // The last time a click event was sent out (used for double-clicks)
            clickCount = 0; // Number of clicks in a row. 2 for a double-click for example.

            scrollDelta = Vector2.zero;
            useDragThreshold = true;
            dragging = false;
            button = InputButton.Left;
        }

        // Is the pointer moving.
        public bool IsPointerMoving()
        {
            return delta.sqrMagnitude > 0.0f;
        }

        // Is scroll being used on the input device.
        public bool IsScrolling()
        {
            return scrollDelta.sqrMagnitude > 0.0f;
        }

        // The GameObject that received the OnPointerDown.
        public GameObject pointerPress
        {
            get { return m_PointerPress; }
            set
            {
                if (m_PointerPress == value)
                    return;

                lastPress = m_PointerPress;
                m_PointerPress = value;
            }
        }
    }
}