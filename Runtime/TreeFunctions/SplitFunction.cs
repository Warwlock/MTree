using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mtree
{
    public class SplitFunction : TreeFunctionAsset
    {
        public int number = 25;
        public float splitAngle = .5f;
        public float splitRadius = .6f;
        public float start = .2f;
        public float spread = 1;

        public override void Init(TreeFunctionAsset parent, bool preserveSettings = false)
        {
            base.Init(parent);
            name = "Split";
        }

        public override void DrawProperties()
        {
#if UNITY_EDITOR

            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("seed"), new GUIContent("Seed"));
            Utils.BoundedIntProperty(obj.FindProperty("number"), new GUIContent("Number"), 0);
            EditorGUILayout.Slider(obj.FindProperty("splitAngle"), 0, 2, new GUIContent("Split angle"));
            EditorGUILayout.Slider(obj.FindProperty("splitRadius"), 0.001f, 1f, new GUIContent("Split Radius"));
            EditorGUILayout.Slider(obj.FindProperty("start"), 0, 1, new GUIContent("Start"));
            EditorGUILayout.Slider(obj.FindProperty("spread"), 0, 1, new GUIContent("Height spread"));

            obj.ApplyModifiedProperties();
#endif
        }

        public override void Execute(MTree tree)
        {
            base.Execute(tree);

            Random.InitState(seed);
            int selection = parent == null ? 0 : parent.id;
            tree.Split(selection, number, splitAngle, id, splitRadius, start, spread, 0f);
        }
    }
}