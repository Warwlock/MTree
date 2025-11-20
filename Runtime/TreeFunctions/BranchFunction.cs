using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mtree
{
    public class BranchFunction : TreeFunctionAsset
    {
        public float length = 7f;
        public AnimationCurve lengthCurve = AnimationCurve.Linear(0, 1, 1, .8f);
        public float resolution = 1f;
        public int number = 30;
        public float splitProba = .1f;
        public float angle = .7f;
        public AnimationCurve shape = AnimationCurve.EaseInOut(0, 1, 1, .1f);
        public AnimationCurve splitProbaCurve = AnimationCurve.Linear(0, .5f, 1, 1f);
        public float radius = .6f;
        public float randomness = .25f;
        public float upAttraction = .5f;
        public float start = .2f;
        public float gravityStrength = .1f;
        public bool obstacleAvoidance = false;

        public override void Init(TreeFunctionAsset parent, bool preserveSettings = false)
        {
            base.Init(parent);
            name = "Branch";

            if (parent != null && !(parent is TrunkFunction) && !preserveSettings)
            {
                length = 3;
                start = .3f;
                angle = .4f;
                number = 120;
                resolution = 1;
                splitProba = .3f;
                radius = .8f;
                shape = AnimationCurve.Linear(0, 1, 1, .1f);
                randomness = .4f;
                gravityStrength = 1f;
            }
        }

        public override void DrawProperties()
        {
#if UNITY_EDITOR

            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("seed"), new GUIContent("Seed"));
            Utils.BoundedIntProperty(obj.FindProperty("number"), new GUIContent("Number"), 0);

            EditorGUILayout.BeginHorizontal();
            Utils.BoundedFloatProperty(obj.FindProperty("length"), new GUIContent("Length"), 0.001f);
            EditorGUILayout.PropertyField(obj.FindProperty("lengthCurve"), new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            Utils.BoundedFloatProperty(obj.FindProperty("resolution"), new GUIContent("Resolution"), 0.01f);
            EditorGUILayout.Slider(obj.FindProperty("randomness"), 0, 1, new GUIContent("Randomness"));
            EditorGUILayout.Slider(obj.FindProperty("radius"), 0.001f, 1, new GUIContent("Radius"));
            EditorGUILayout.PropertyField(obj.FindProperty("shape"), new GUIContent("Shape"));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(obj.FindProperty("splitProba"), 0, 1, new GUIContent("Split proba"));
            EditorGUILayout.PropertyField(obj.FindProperty("splitProbaCurve"), new GUIContent(""));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Slider(obj.FindProperty("angle"), 0, 1, new GUIContent("Angle"));
            EditorGUILayout.PropertyField(obj.FindProperty("upAttraction"), new GUIContent("Up attraction"));
            EditorGUILayout.PropertyField(obj.FindProperty("gravityStrength"), new GUIContent("Gravity Strength"));
            EditorGUILayout.Slider(obj.FindProperty("start"), 0, 1, new GUIContent("Start"));
            EditorGUILayout.PropertyField(obj.FindProperty("obstacleAvoidance"), new GUIContent("React to scene colliders"));

            obj.ApplyModifiedProperties();

#endif
        }

        public override void Execute(MTree tree)
        {
            base.Execute(tree);

            Random.InitState(seed);
            int selection = parent == null ? 0 : parent.id;
            tree.AddBranches(selection, length, lengthCurve, resolution, number, splitProba, splitProbaCurve, angle
                            , randomness, shape, radius, upAttraction, id, start, gravityStrength, 0f, 0.001f, obstacleAvoidance);
        }
    }
}