using UnityEditor;
using UnityEngine;

namespace RO.UI
{
    public class UpdateSlots
    {
        // Only current limitation os that elements Slots are ordered and at begining of the  scrollviews
        public class Implementation : Editor
        {
            private Transform[] getChilds(Transform transform, out int slot1Index)
            {
                slot1Index = -1;

                if (transform.childCount < 2)
                    return null;

                // fetch all childs
                Transform[] childs = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                {
                    childs[i] = (RectTransform)transform.GetChild(i);
                    if (childs[i].name.Contains("Slot (1)"))
                    {
                        if (slot1Index == -1)
                            slot1Index = i;
                        else
                            return null; // more that a slot 1
                    }
                }

                return slot1Index == -1 ? null : childs; // no slot 1
            }

            protected void ImpOnInspectorGUI<T>() where T : MonoBehaviour
            {
                DrawDefaultInspector();

                if (GUILayout.Button("update slots"))
                {
                    var view = target as T;
                    var transform = view.gameObject.transform;
                    int slot1Index;
                    var childs = getChilds(transform, out slot1Index);

                    if (childs == null)
                        return;

                    for (int i = 0; i < childs.Length; i++)
                    {
                        RectTransform slotN = (RectTransform)childs[i];
                        string name = "Slot (" + (i + 1).ToString() + ")";

                        if (slotN.name.Contains(name))
                        {
                            UnityEditorInternal.ComponentUtility.CopyComponent(slotN);

                            GameObject gameobject = Instantiate(childs[slot1Index].gameObject, transform);
                            gameobject.name = slotN.name;
                            UnityEditorInternal.ComponentUtility.PasteComponentValues((RectTransform)gameobject.transform);
                            gameobject.SetActive(childs[slot1Index].gameObject.activeSelf);
                            childs[i] = gameobject.transform;

                            slotN.SetParent(null);
                            DestroyImmediate(slotN.gameObject);
                        }
                    }

                    // restore original order
                    for (int i = 0; i < childs.Length; i++)
                        childs[i].transform.SetSiblingIndex(i);
                }
            }
        }

        [CustomEditor(typeof(MultiColumnListScrollView))]
        public class MultiColumnListScrollViewEditor : Implementation
        {
            public override void OnInspectorGUI()
            {
                ImpOnInspectorGUI<MultiColumnListScrollView>();
            }
        }
    }
}