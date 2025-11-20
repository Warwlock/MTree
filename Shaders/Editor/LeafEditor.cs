using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class LeafEditor : ShaderGUI
{
    enum CullMode
    {
        Off,
        Front,
        Back
    }

    enum DoubleSidedNormal
    {
        Flip,
        Mirror,
        None
    }

    enum ColorShifting
    {
        On,
        Off
    }

    enum TranslucencyEnum
    {
        Off,
        On
    }

    enum ColorSource
    {
        Global,
        Custom
    }

    enum WindModeLeaves
    {
        Leaves,
        Palm,
        Grass,
        Off
    }

    CullMode mode;
    DoubleSidedNormal dsNorm;
    ColorShifting colShift;
    TranslucencyEnum transEnum;
    ColorSource colSource;
    WindModeLeaves windMode;

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
        if(property.name == "_CullMode")
        {
            mode = (CullMode)property.floatValue;
            mode = (CullMode)DrawEnum(mode, property.displayName);
            property.floatValue = (int)mode;
            return;
        }
        if (property.name == "_DoubleSidedNormalMode")
        {
            dsNorm = (DoubleSidedNormal)property.floatValue;
            dsNorm = (DoubleSidedNormal)DrawEnum(dsNorm, property.displayName);
            property.floatValue = (int)dsNorm;
            return;
        }
        if (property.name == "_ColorShifting")
        {
            colShift = (ColorShifting)property.floatValue;
            colShift = (ColorShifting)DrawEnum(colShift, property.displayName);
            property.floatValue = (int)colShift;
            return;
        }
        if (property.name == "_TranslucencyEnum")
        {
            transEnum = (TranslucencyEnum)property.floatValue;
            transEnum = (TranslucencyEnum)DrawEnum(transEnum, property.displayName);
            property.floatValue = (int)transEnum;
            return;
        }
        if (property.name == "_ColorSource")
        {
            colSource = (ColorSource)property.floatValue;
            colSource = (ColorSource)DrawEnum(colSource, property.displayName);
            property.floatValue = (int)colSource;
            return;
        }
        if (property.name == "_WindModeLeaves")
        {
            windMode = (WindModeLeaves)property.floatValue;
            windMode = (WindModeLeaves)DrawEnum(windMode, property.displayName);
            property.floatValue = (int)windMode;
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

        if (index == 7)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color Settings", headerStyle);
        }

        if (index == 12)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Translucency Settings", headerStyle);
        }

        if (index == 19)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Other Settings", headerStyle);
        }

        if (index == 22)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wind Settings", headerStyle);
        }
    }
}
