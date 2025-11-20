using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mtree
{
    public class TrunkFunction : TreeFunctionAsset
    {
        public float radiusMultiplier = .3f;
        public AnimationCurve radius;
        public float length = 15;
        public int number = 1;
        public float randomness = .1f;
        public float originAttraction = .1f;
        public float resolution = 1.5f;
        public AnimationCurve rootShape;
        public float rootRadius = .25f;
        public float rootInnerRadius = .2f;
        public float rootHeight = 1f;
        public float rootResolution = 3f;
        public int flareNumber = 5;
        public float spinAmount = .1f;
        public float displacementStrength = 1f;
        public float displacementSize = 2.5f;
        public float heightOffset = .5f;

        public override void Init(TreeFunctionAsset parent, bool preserveSettings = false)
        {
            base.Init(parent);
            name = "Trunk";
            if (!preserveSettings)
            {
                Keyframe[] keys = new Keyframe[2] { new Keyframe(0f, 1f, 0f, 0f), new Keyframe(1f, 0f, -1f, -1f) };
                radius = new AnimationCurve(keys);
                Keyframe[] rootKeys = new Keyframe[2] { new Keyframe(0f, 1f, -2f, -2f), new Keyframe(1f, 0f, 0f, 0f) };
                rootShape = new AnimationCurve(rootKeys);
            }

        }

        public override void DrawProperties()
        {
        #if UNITY_EDITOR

            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("seed"), new GUIContent("Seed"));
            Utils.BoundedFloatProperty(obj.FindProperty("length"), "Length", 0.01f);
            Utils.BoundedFloatProperty(obj.FindProperty("radiusMultiplier"), "Radius", 0.01f);
            Utils.BoundedFloatProperty(obj.FindProperty("resolution"), new GUIContent("Resolution", "Number of segments per unit of length"), 0.01f);
            EditorGUILayout.Slider(obj.FindProperty("originAttraction"), 0, 1, new GUIContent("Axis attraction", "How much the tree is drawn to its original axis"));
            EditorGUILayout.PropertyField(obj.FindProperty("radius"), new GUIContent("Shape"));
            EditorGUILayout.Slider(obj.FindProperty("randomness"), 0, .5f, new GUIContent("Randomness", "How irregular the trunk looks"));
            EditorGUILayout.PropertyField(obj.FindProperty("displacementStrength"), new GUIContent("Displacement strength", "Strength of the the noise affecting the geometry of the trunk"));
            EditorGUILayout.PropertyField(obj.FindProperty("displacementSize"), new GUIContent("Displacement size", "Size of the noise affecting the geometry of the trunk"));
            EditorGUILayout.PropertyField(obj.FindProperty("spinAmount"), new GUIContent("Spin amount", "How much the trunk is twisted"));
            EditorGUILayout.PropertyField(obj.FindProperty("heightOffset"), new GUIContent("Height Offset", "How much the trunk goes inside the ground. Helps when a tree is placed on an uneven ground"));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Flares");

            EditorGUILayout.PropertyField(obj.FindProperty("rootShape"), new GUIContent("Shape"));
            Utils.BoundedFloatProperty(obj.FindProperty("rootHeight"), "Start Height", 0.01f);
            EditorGUILayout.Slider(obj.FindProperty("rootRadius"), 0, 2, new GUIContent("Outer Radius"));
            EditorGUILayout.Slider(obj.FindProperty("rootInnerRadius"), 0, 1, new GUIContent("Inner Radius"));
            Utils.BoundedFloatProperty(obj.FindProperty("rootResolution"), new GUIContent("Additional Resolution"), 1);
            EditorGUILayout.IntSlider(obj.FindProperty("flareNumber"), 0, 10, new GUIContent("Number"));

            EditorGUILayout.EndVertical();
            obj.ApplyModifiedProperties();

        #endif
        }

        public override void Execute(MTree tree)
        {
            base.Execute(tree);

            Random.InitState(seed);

            tree.AddTrunk(Vector3.down * heightOffset, Vector3.up, length, radius, radiusMultiplier, resolution, randomness
            , id, rootShape, rootRadius, rootHeight, rootResolution, originAttraction);
        }
    }
}