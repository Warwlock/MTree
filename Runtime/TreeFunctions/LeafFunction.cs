using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;
namespace Mtree
{
    public class LeafFunction : TreeFunctionAsset
    {
        public int leafType = 4;
        public static string[] leafTypesNames = { "cross", "diamond cross", "diamond", "long", "plane", "Procedural", "custom" };
        public int number = 250;
        public Mesh customLeafMesh;
        public float maxRadius = .25f;
        public float size = 2f;
        public bool overrideNormals = true;
        public float minWeight = .2f;
        public float maxWeight = .5f;
        public float length = 2f;
        public AnimationCurve lengthCurve = AnimationCurve.Linear(0, 1, 1, 0.5f);
        public int uLoops = 2;
        public float resolution = 1f;
        public float gravityStrength = 1f;

        public override void Init(TreeFunctionAsset parent, bool preserveSettings = false)
        {
            base.Init(parent);
            name = "Leaves";
            customLeafMesh = null;
        }

#if UNITY_EDITOR
        public override void DrawProperties()
        {

            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.PropertyField(obj.FindProperty("seed"), new GUIContent("Seed"));

            SerializedProperty leafTypeProp = obj.FindProperty("leafType");
            leafTypeProp.intValue = EditorGUILayout.Popup(leafTypeProp.intValue, leafTypesNames);

            if (leafTypeProp.intValue == 6)
                EditorGUILayout.PropertyField(obj.FindProperty("customLeafMesh"), new GUIContent("Leaf mesh"));

            Utils.BoundedIntProperty(obj.FindProperty("number"), new GUIContent("Number"), 0);
            EditorGUILayout.PropertyField(obj.FindProperty("size"), new GUIContent("Size"));
            EditorGUILayout.Slider(obj.FindProperty("maxRadius"), 0.001f, 1, "Max branch radius");

            SerializedProperty minWeihtProp = obj.FindProperty("minWeight");
            SerializedProperty maxWeihtProp = obj.FindProperty("maxWeight");
            float minTmp = minWeihtProp.floatValue;
            float maxTmp = maxWeihtProp.floatValue;
            EditorGUILayout.MinMaxSlider("leafs weight", ref minTmp, ref maxTmp, -1, 1);
            minWeihtProp.floatValue = minTmp;
            maxWeihtProp.floatValue = maxTmp;

            EditorGUILayout.PropertyField(obj.FindProperty("overrideNormals"), new GUIContent("Override normals"));
            if (leafTypeProp.intValue == 5)
            {
                EditorGUILayout.PropertyField(obj.FindProperty("length"), new GUIContent("Length multiplier"));
                EditorGUILayout.PropertyField(obj.FindProperty("lengthCurve"), new GUIContent("Length"));
                Utils.BoundedFloatProperty(obj.FindProperty("resolution"), "resolution", 0.001f);
                EditorGUILayout.IntSlider(obj.FindProperty("uLoops"), 2, 5, new GUIContent("Middle segments"));
                EditorGUILayout.PropertyField(obj.FindProperty("gravityStrength"), new GUIContent("Gravity strength"));
            }

            obj.ApplyModifiedProperties();
        }
#endif

        public override void Execute(MTree tree)
        {
            base.Execute(tree);

            Random.InitState(seed);
            int selection = parent == null ? 0 : parent.id;
            bool procedural = leafType == 5;

            Mesh[] leafMeshes = null;
            switch (leafType)
            {
                case 0:
                    leafMeshes = new Mesh[] { Resources.LoadAll<Mesh>("Mtree/branches")[0] };
                    break;
                case 1:
                    leafMeshes = new Mesh[] { Resources.LoadAll<Mesh>("Mtree/branches")[1] };
                    break;
                case 2:
                    leafMeshes = new Mesh[] { Resources.LoadAll<Mesh>("Mtree/branches")[2], Resources.LoadAll<Mesh>("Mtree/branches")[3] };
                    break;
                case 3:
                    leafMeshes = new Mesh[] { Resources.LoadAll<Mesh>("Mtree/branches")[4], Resources.LoadAll<Mesh>("Mtree/branches")[5] };
                    break;
                case 4:
                    leafMeshes = new Mesh[] { Resources.LoadAll<Mesh>("Mtree/branches")[6] };
                    break;
                case 5:
                    leafMeshes = null;
                    break;
                case 6:
                    leafMeshes = new Mesh[] { customLeafMesh };
                    break;
            }
            
            tree.AddLeafs(maxRadius, number, leafMeshes, size, overrideNormals, minWeight, maxWeight, selection, 70, procedural, length, resolution, uLoops, gravityStrength);
        }
    }
}