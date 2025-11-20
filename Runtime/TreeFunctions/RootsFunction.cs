using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mtree
{
    public class RootsFunction : TreeFunctionAsset
    {
        public float length = 1.5f;
        public float resolution = 3f;
        public int number = 8;
        public float splitProba = .4f;
        public float angle = .3f;
        public AnimationCurve shape = AnimationCurve.EaseInOut(0, 1, 1, .5f);
        public AnimationCurve splitProbaCurve = AnimationCurve.Linear(0, .5f, 1, 1f);
        public float radius = .6f;
        public float randomness = .25f;
        public float start = .1f;
        public float end = .3f;
        public float groundHeight = 0f;
        public float attractionStrength = 1f;


        public override void Init(TreeFunctionAsset parent, bool preserveSettings = false)
        {
            base.Init(parent);
            name = "Roots";
        }

        public override void DrawProperties()
        {
#if UNITY_EDITOR

            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("seed"), new GUIContent("Seed"));
            Utils.BoundedIntProperty(obj.FindProperty("number"), new GUIContent("Number"), 0);

            EditorGUILayout.BeginHorizontal();
            Utils.BoundedFloatProperty(obj.FindProperty("length"), new GUIContent("Length"), 0.001f);
            EditorGUILayout.EndHorizontal();

            Utils.BoundedFloatProperty(obj.FindProperty("resolution"), new GUIContent("Resolution"), 0.01f);
            EditorGUILayout.Slider(obj.FindProperty("randomness"), 0, 1, new GUIContent("Randomness"));
            EditorGUILayout.Slider(obj.FindProperty("radius"), 0.001f, 1, new GUIContent("Radius"));
            EditorGUILayout.PropertyField(obj.FindProperty("shape"), new GUIContent("Shape"));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(obj.FindProperty("splitProba"), 0, 1, new GUIContent("Split proba"));
            EditorGUILayout.PropertyField(obj.FindProperty("splitProbaCurve"), new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Slider(obj.FindProperty("angle"), 0.1f, .95f, new GUIContent("Angle"));
            Utils.BoundedFloatProperty(obj.FindProperty("start"), new GUIContent("Start height"), 0, end);
            Utils.BoundedFloatProperty(obj.FindProperty("end"), new GUIContent("End height"), start);

            EditorGUILayout.PropertyField(obj.FindProperty("groundHeight"), new GUIContent("Ground height"));
            Utils.BoundedFloatProperty(obj.FindProperty("attractionStrength"), new GUIContent("attraction strength"), 0f);

            obj.ApplyModifiedProperties();

#endif
        }

        public override void Execute(MTree tree)
        {
            base.Execute(tree);

            Random.InitState(seed);
            int selection = parent == null ? 0 : parent.id;
            tree.AddRoots(selection, this);
        }
    }
}