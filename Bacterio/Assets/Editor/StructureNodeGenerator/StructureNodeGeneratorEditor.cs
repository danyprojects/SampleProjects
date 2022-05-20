using System.Text.RegularExpressions;
using System;
using UnityEngine;
using UnityEditor;

namespace BacterioEditor
{
    [CustomEditor(typeof(StructureNodeGeneratorObject))]
    public class StructureNodeGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        }

        public void Draw(float leftMargin, Action onClearNodes, Action onGenerateNodes)
        {
            var style = EditorStyles.inspectorFullWidthMargins;
            style.margin.left = (int)leftMargin;

            EditorGUILayout.BeginVertical(style);
            DrawDefaultInspector();

            if (GUILayout.Button("Clear nodes"))
                onClearNodes();

            if (GUILayout.Button("Generate"))
                onGenerateNodes();

            EditorGUILayout.EndVertical();
        }
    }
}
