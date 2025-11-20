using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mtree
{
    public class GrowFunction : TreeFunctionAsset
    {
        public float length = 10f;
        public AnimationCurve lengthCurve = AnimationCurve.Linear(0, 1, 1, .8f);
        public float resolution = 1f;
        public float splitProba = .1f;
        public float splitAngle = .5f;
        public int maxSplits = 2;
        public AnimationCurve radius = AnimationCurve.EaseInOut(0, 1, 1, .1f);
        public AnimationCurve splitProbaCurve = AnimationCurve.Linear(0, .5f, 1, 1f);
        public float splitRadius = .8f;
        public float randomness = .25f;
        public float upAttraction = .5f;
        public float gravityStrength = .1f;
        public bool obstacleAvoidance = false;

        public override void Init(TreeFunctionAsset parent, bool preserveSettings = false)
        {
            base.Init(parent);
            name = "Grow";
            if (!preserveSettings)
            {
                Keyframe[] keys = new Keyframe[2] { new Keyframe(0f, 1f, 0f, 0f), new Keyframe(1f, 0f, -.5f, -1f) };
                radius = new AnimationCurve(keys);
            }
        }

        public override void DrawProperties()
        {
#if UNITY_EDITOR

            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("seed"), new GUIContent("Seed"));

            EditorGUILayout.BeginHorizontal();
            Utils.BoundedFloatProperty(obj.FindProperty("length"), new GUIContent("Length"), 0.001f);
            EditorGUILayout.PropertyField(obj.FindProperty("lengthCurve"), new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            Utils.BoundedFloatProperty(obj.FindProperty("resolution"), new GUIContent("Resolution"), 0.01f);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(obj.FindProperty("splitProba"), 0, 1, new GUIContent("Split proba"));
            EditorGUILayout.PropertyField(obj.FindProperty("splitProbaCurve"), new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Slider(obj.FindProperty("splitAngle"), 0, 2, new GUIContent("Split Angle"));
            EditorGUILayout.PropertyField(obj.FindProperty("radius"), new GUIContent("Shape"));
            EditorGUILayout.Slider(obj.FindProperty("splitRadius"), .5f, .999f, new GUIContent("Split Radius"));
            EditorGUILayout.IntSlider(obj.FindProperty("maxSplits"), 2, 4, new GUIContent("Max splits at a time"));
            EditorGUILayout.Slider(obj.FindProperty("randomness"), 0, 1, new GUIContent("Randomness"));
            EditorGUILayout.Slider(obj.FindProperty("upAttraction"), 0, 1, new GUIContent("Up attraction"));
            EditorGUILayout.PropertyField(obj.FindProperty("gravityStrength"), new GUIContent("Gravity Strength"));
            EditorGUILayout.PropertyField(obj.FindProperty("obstacleAvoidance"), new GUIContent("React to scene colliders"));

            obj.ApplyModifiedProperties();

#endif
        }

        public override void Execute(MTree tree)
        {
            base.Execute(tree);

            Random.InitState(seed);
            int selection = parent == null ? 0 : parent.id;
                tree.Grow(length, lengthCurve, resolution, splitProba, splitProbaCurve, splitAngle, maxSplits, selection
                        , id, randomness, radius, splitRadius, upAttraction, gravityStrength, 0f, 0.001f, obstacleAvoidance);
        }
    }
}