using UnityEngine;

namespace RO.UI
{
    public struct RaycastResult
    {
        // Game object hit by the raycast 
        public GameObject gameObject;

        // BaseRaycaster that raised the hit.
        public GraphicRaycaster module;

        // Hit index
        public float index;

        // Used by raycasters where elements may have the same unit distance, but have specific ordering.
        public int depth;

        // The SortingLayer of the hit object.
        // For UI.Graphic elements this will be the values from that graphic's Canvas
        public int sortingLayer;

        // The SortingOrder for the hit object.
        // For Graphic elements this will be the values from that graphics Canvas
        public int sortingOrder;

        // The world position of the where the raycast has hit.
        public Vector3 worldPosition;

        // The normal at the hit location of the raycast.
        public Vector3 worldNormal;

        // The screen position from which the raycast was generated.
        public Vector2 screenPosition;

        // Is there an associated module and a hit GameObject.
        public bool isValid
        {
            get { return module != null && gameObject != null; }
        }

        // Reset the result.
        public void Clear()
        {
            gameObject = null;
            module = null;
            index = 0;
            depth = 0;
            sortingLayer = 0;
            sortingOrder = 0;
            worldNormal = Vector3.up;
            worldPosition = Vector3.zero;
            screenPosition = Vector2.zero;
        }
    }
}