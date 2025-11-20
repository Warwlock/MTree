using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Mtree
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TreeTemplate))]
    public class TemplateEditor : Editor
    {
        TreeTemplate template;
        int selectedFuntionIndex = 0;

        private void OnEnable()
        {
            template = (TreeTemplate)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.labelWidth = 135;
            int functionCount = template.treeFunctions.Count;
            int rectHeightMultiplier = TreeFunctionAsset.height + TreeFunctionAsset.margin;
            int rectHeight = functionCount * rectHeightMultiplier; // get the height of the drawing window inside inspector
            Event e = Event.current; // Get current event

            int heighIndex = 0;
            template.treeFunctions[0].UpdateRectRec(ref heighIndex, 0);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Tree functions", EditorStyles.boldLabel);

            Rect rect = GUILayoutUtility.GetRect(10, 1000, rectHeight, rectHeight); // Create drawing window
            GUI.BeginClip(rect);

            for (int i = 0; i < template.treeFunctions.Count; i++)
                template.treeFunctions[i].DrawFunction(i == selectedFuntionIndex, false);

            GUI.EndClip();
            EditorGUILayout.EndVertical();

            if (e.type == EventType.MouseDown && e.button == 0) // If mouse button is pressed, get button pressed and act accordingly
            {
                for (int i = 0; i < functionCount; i++)
                {
                    TreeFunctionAsset tf = template.treeFunctions[i];

                    if (tf.rect.Contains(e.mousePosition - rect.position))
                    {
                        selectedFuntionIndex = i;
                        GUIUtility.ExitGUI();
                        break;
                    }
                }
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            TreeFunctionAsset selectedFunction = template.treeFunctions[selectedFuntionIndex];
            selectedFunction.DrawProperties();
            EditorGUILayout.EndVertical();

        }
    }
}