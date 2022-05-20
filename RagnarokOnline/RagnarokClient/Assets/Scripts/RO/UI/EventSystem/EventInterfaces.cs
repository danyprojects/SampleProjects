using UnityEngine;

namespace RO.UI
{
    public interface IPointerEnterHandler
    {
        void OnPointerEnter(PointerEventData eventData);
    }

    public interface IPointerExitHandler
    {
        void OnPointerExit(PointerEventData eventData);
    }

    public interface IPointerDownHandler
    {
        void OnPointerDown(PointerEventData eventData);
    }

    public interface IPointerUpHandler
    {
        void OnPointerUp(PointerEventData eventData);
    }

    public interface IPointerClickHandler
    {
        void OnPointerClick(PointerEventData eventData);
    }

    public interface IBeginDragHandler
    {
        void OnBeginDrag(PointerEventData eventData);
    }

    public interface IInitializePotentialDragHandler
    {
        void OnInitializePotentialDrag(PointerEventData eventData);
    }

    public interface IDragHandler
    {
        void OnDrag(PointerEventData eventData);
    }

    public interface IEndDragHandler
    {
        void OnEndDrag(PointerEventData eventData);
    }

    public interface IDropHandler
    {
        void OnDrop(PointerEventData eventData);
    }

    public interface IScrollHandler
    {
        void OnScroll(PointerEventData eventData);
    }

    public interface ISelectHandler
    {
        void OnSelect();
    }

    public interface IDeselectHandler
    {
        void OnDeselect();
    }

    public interface IKeyboardHandler
    {
        void OnKeyDown(Event evt);

        void OnKeyboardFocus(bool hasFocus);
    }
}
