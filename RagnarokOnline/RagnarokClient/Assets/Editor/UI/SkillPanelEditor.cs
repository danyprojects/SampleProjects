using RO.Databases;
using System;
using UnityEditor;
using UnityEngine;

namespace RO.UI
{
    [CustomEditor(typeof(SkillPanelController))]
    public class SkillPanelEditor : Editor
    {
        SerializedProperty miniSlots;
        SerializedProperty fullSlots;
        SerializedProperty skillSprites;

        private void fillMiniComponent(SerializedProperty slot, GameObject obj)
        {
            slot.objectReferenceValue = obj.GetComponent<SkillMiniSlotController>();
        }

        private void fillFullComponent(SerializedProperty slot, GameObject obj)
        {
            slot.objectReferenceValue = obj.GetComponent<SkillFullSlotController>();
        }

        private void updateSlots<T>(SkillPanelController controller, SerializedProperty slots,
            Action<SerializedProperty, GameObject> fillCb) where T : MonoBehaviour
        {
            var view = controller.gameObject.GetComponentInChildren<T>(true);
            int slotCount = slots.arraySize;
            int copiedCount = 0;

            for (int i = 0; i < view.transform.childCount && i < slotCount; i++)
            {
                var child = view.transform.GetChild(i);
                string name = "Slot (" + (i + 1).ToString() + ")";

                if (child.name.Contains(name))
                {
                    copiedCount++;
                    var slot = slots.GetArrayElementAtIndex(i);
                    fillCb(slot, child.gameObject);
                }
            }

            if (copiedCount != slotCount)
                throw new UnityException("Slot count missmatch expected " + slotCount + " found " + copiedCount);
        }

        void OnEnable()
        {
            miniSlots = serializedObject.FindProperty("_miniSlots");
            fullSlots = serializedObject.FindProperty("_fullSlots");
            skillSprites = serializedObject.FindProperty("_skillSprites");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("update slots"))
            {
                var controller = target as SkillPanelController;

                updateSlots<ListScrollView>(controller, miniSlots, fillMiniComponent);
                updateSlots<MultiColumnListScrollView>(controller, fullSlots, fillFullComponent);

                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("update skill icons"))
            {
                var controller = target as SkillPanelController;

                string pngPath = "Assets/~Resources/GRF/UI/SkillIcons/";

                for (int i = 0; i < (int)SkillIds.Last; i++)
                {
                    var path = pngPath + Enum.GetName(typeof(SkillIds), i) + ".png";

                    var importer = AssetImporter.GetAtPath(path);
                    if (importer != null)
                    {
                        importer.SetAssetBundleNameAndVariant("ui", "");
                        importer.SaveAndReimport();

                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);


                        skillSprites.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}