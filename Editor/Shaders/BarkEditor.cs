using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BarkEditor : ShaderGUI
{
    enum DetailSettings
    {
        On,
        Off
    }

    DetailSettings dSets;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;
        //materialEditor.PropertiesDefaultGUI(properties);

        //base.OnGUI(materialEditor, properties);
        for (int i = 0; i < properties.Length; i++)
        {
            var property = properties[i];

            DrawHeaders(i);
            if ((property.propertyFlags & ShaderPropertyFlags.HideInInspector) == 0)
            {
                CheckProperty(materialEditor, property);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
        {
            materialEditor.RenderQueueField();
        }
        materialEditor.EnableInstancingField();
        materialEditor.DoubleSidedGIField();
    }

    void CheckProperty(MaterialEditor materialEditor, MaterialProperty property)
    {
        if (property.name == "_CullMode")
        {
            //mode = (CullMode)property.floatValue;
            //mode = (CullMode)DrawEnum(mode, property.displayName);
            //property.floatValue = (int)mode;
            return;
        }
        if (property.name == "_BaseDetail")
        {
            dSets = (DetailSettings)property.floatValue;
            dSets = (DetailSettings)DrawEnum(dSets, property.displayName);
            property.floatValue = (int)dSets;
            return;
        }
        materialEditor.ShaderProperty(property, property.displayName);
    }

    System.Enum DrawEnum(System.Enum mode, string label)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        mode = EditorGUILayout.EnumPopup(mode);
        EditorGUILayout.EndHorizontal();
        return mode;
    }

    void DrawHeaders(int index)
    {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;

        if (index == 0)
        {
            EditorGUILayout.LabelField("General Settings", headerStyle);
        }

        if (index == 5)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Detail Settings", headerStyle);
        }

        if (index == 12)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Other Settings", headerStyle);
        }

        if (index == 15)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wind Settings", headerStyle);
        }
    }
}
