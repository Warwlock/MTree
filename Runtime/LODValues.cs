using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mtree
{

    public class LODValues : ScriptableObject
    {
        public float radialResolution = 1f;
        public float simplifyAngleThreshold = 0.01f;
        public float simplifyRadiusThreshold = 0f;
        public float simplifyLeafs = 0f;

        public void Init(LODValues previousLOD = null)
        {
            if (previousLOD == null)
                return;
            this.radialResolution = previousLOD.radialResolution / 2.5f;
            this.simplifyAngleThreshold = Mathf.Lerp(previousLOD.simplifyAngleThreshold, 40, .2f);
            this.simplifyRadiusThreshold = Mathf.Lerp(previousLOD.simplifyRadiusThreshold, .15f, .2f);
            this.simplifyLeafs = Mathf.Clamp(previousLOD.simplifyLeafs + .2f, 0, 1);
        }

#if UNITY_EDITOR
        public void DrawProperties()
        {
            SerializedObject obj = new SerializedObject(this);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Utils.BoundedFloatProperty(obj.FindProperty("radialResolution"), new GUIContent("Radial resolution"), 0.001f);
            EditorGUILayout.Slider(obj.FindProperty("simplifyAngleThreshold"), 0, 90, new GUIContent("Angle threshold"));
            EditorGUILayout.Slider(obj.FindProperty("simplifyRadiusThreshold"), 0, .9f, new GUIContent("Simplify radius"));
            EditorGUILayout.Slider(obj.FindProperty("simplifyLeafs"), 0, .99f, new GUIContent("Simplify leaves"));

            EditorGUILayout.EndVertical();
            obj.ApplyModifiedProperties();
        }
#endif
    }
}