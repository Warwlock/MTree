using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Mtree
{
    public class Utils
    {
        public static string GetCurrentPipeline()
        {
            var ActivePipeline = "legacy";
            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                if (GraphicsSettings.defaultRenderPipeline.GetType().ToString().Contains("LightweightRenderPipelineAsset"))
                    ActivePipeline = "lwrp";
                if (GraphicsSettings.defaultRenderPipeline.GetType().ToString().Contains("UniversalRenderPipelineAsset"))
                    ActivePipeline = "urp";
                if (GraphicsSettings.defaultRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset"))
                    ActivePipeline = "hdrp";
            }
            else
            {
                ActivePipeline = "legacy";
            }
            return ActivePipeline;
        }

        public static Shader GetBarkShader()
        {
            Shader shader = null;

            switch(GetCurrentPipeline())
            {
                case "legacy":
                    shader = Shader.Find("Mtree/Bark");
                break;

                case "lwrp":
                    shader = Shader.Find("Mtree/SRP/Bark LWRP");
                break;

                case "hdrp":
                    shader = Shader.Find("Mtree/SRP/Bark HDRP");
                break;

                case "urp":
                    shader = Shader.Find("Mtree/SRP/Bark URP");
                break;
            }
            return shader;
        }

        public static Shader GetLeafShader()
        {
            Shader shader = null;
            
            switch(GetCurrentPipeline()){
                case "legacy":
                    shader = Shader.Find("Mtree/Leafs");
                break;

                case "lwrp":
                    shader = Shader.Find("Mtree/SRP/Leafs LWRP");
                break;

                case "hdrp":
                    shader = Shader.Find("Mtree/SRP/Leafs HDRP");
                break;

                case "urp":
                    shader = Shader.Find("Mtree/SRP/Leafs URP");
                break;
            }
            return shader;
        }

        public static Shader GetBillboardShader()
        {
            Shader shader = null;
            
            switch(GetCurrentPipeline()){
                case "legacy":
                    shader = Shader.Find("Mtree/Billboard");
                break;

                case "lwrp":
                    shader = Shader.Find("Mtree/SRP/Billboard LWRP");
                break;

                case "hdrp":
                    shader = Shader.Find("Mtree/SRP/Billboard HDRP");
                break;

                case "urp":
                    shader = Shader.Find("Universal Render Pipeline/Lit");
                break;
            }
            return shader;
        }
        public static Shader GetVertexColorShader()
        {
            Shader shader = null;
            
            switch(GetCurrentPipeline()){
                case "legacy":
                    shader = Shader.Find("Hidden/Mtree/VertexColorShader");
                break;

                case "lwrp":
                    shader = Shader.Find("Hidden/Mtree/SRP/VertexColorShader LWRP");
                break;

                case "hdrp":
                    shader = Shader.Find("Hidden/Mtree/SRP/VertexColorShader HDRP");
                break;

                case "urp":
                    shader = Shader.Find("Hidden/Mtree/SRP/VertexColorShader URP");
                break;
            }
            return shader;
        }

        public static T[] SampleArray<T>(T[] array, int number)
        {
            number = Mathf.Max(0, Mathf.Min(array.Length, number));
            T[] result = new T[number];
            for (int i = 0; i < number; i++)
            {
                int index = Random.Range(i, array.Length - 1);
                result[i] = array[index];
                array[index] = array[i];
            }

            return result;
        }

        public static void DilateTexture(Texture2D texture, int iterations )//, bool removeBackground = false)
        {
            Color[] cols = texture.GetPixels();

            // [Removed dueto HDRP]
            // if (removeBackground)
            //     RemoveBackground(ref cols);

            Color[] copyCols = new Color[cols.Length];
            System.Array.Copy(cols, copyCols, cols.Length);
            HashSet<int> borderIndices = new HashSet<int>();
            HashSet<int> indexBuffer = new HashSet<int>();
            int w = texture.width;
            int h = texture.height;
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].a < 0.5f)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            int index = i + y * w + x;
                            if (index >= 0 && index < cols.Length && cols[index].a > 0.5f) // if a non transparent pixel is near the transparent one, add the transparent pixel index to border indices
                            {
                                borderIndices.Add(i);
                                goto End;
                            }
                        }
                    }
                    End:;
                }
            }

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                foreach (int i in borderIndices)
                {
                    Color meanCol = Color.black;
                    int opaqueNeighbours = 0;
                    for (int x = -1; x < 2; x++)
                    {
                        for (int y = -1; y < 2; y++)
                        {
                            int index = i + y * w + x;
                            if (index >= 0 && index < cols.Length && index != i)
                            {
                                if (cols[index].a > 0.5f)
                                {
                                    cols[index].a = 1;
                                    meanCol += cols[index];
                                    opaqueNeighbours++;
                                }
                                else
                                {
                                    indexBuffer.Add(index);
                                }
                            }
                        }
                    }
                    cols[i] = meanCol / opaqueNeighbours;
                }

                indexBuffer.ExceptWith(borderIndices);

                borderIndices = indexBuffer;
                indexBuffer = new HashSet<int>();
            }
            for (int i = 0; i < cols.Length; i++)
                cols[i].a = copyCols[i].a;
            texture.SetPixels(cols);
        }
        
        // [Removed dueto HDRP]

        // private static void RemoveBackground(ref Color[] colors)
        // {
        //     Debug.Log("Removing");
        //     Color backgroundColor = new Color(0.173f, 0.294f, 0.471f);
        //     for (int i = 0; i < colors.Length; i++)
        //     {
        //         Color c = colors[i];
        //         if (Mathf.Abs(c.r - backgroundColor.r) + Mathf.Abs(c.g - backgroundColor.g) + Mathf.Abs(c.b - backgroundColor.b) < .05)
        //             colors[i] = new Color(0, 0, 0, 0);
        //     }
        // }

        public static void AddTriangle(Queue<int> triangles ,int i1, int i2, int i3)
        {
            triangles.Enqueue(i1); triangles.Enqueue(i2); triangles.Enqueue(i3);
        }

#if UNITY_EDITOR

        public static void BoundedFloatProperty(SerializedProperty prop, string label = "", float minValue = Mathf.NegativeInfinity, float maxValue = Mathf.Infinity)
        {
            GUIContent labelGui = label == "" ? null : new GUIContent(label);
            BoundedFloatProperty(prop, labelGui, minValue, maxValue);
        }

        public static void BoundedFloatProperty(SerializedProperty prop, GUIContent label = null, float minValue = Mathf.NegativeInfinity, float maxValue = Mathf.Infinity)
        {
            Rect position = EditorGUILayout.GetControlRect(label != null);

            EditorGUI.BeginProperty(position, label, prop);
            EditorGUI.PropertyField(position, prop, label);
            prop.floatValue = Mathf.Clamp(prop.floatValue, minValue, maxValue);
            EditorGUI.EndProperty();

        }

        public static void BoundedIntProperty(SerializedProperty prop, GUIContent label = null, int minValue = int.MinValue, int maxValue = int.MaxValue)
        {
            Rect position = EditorGUILayout.GetControlRect(label != null);

            EditorGUI.BeginProperty(position, label, prop);
            EditorGUI.PropertyField(position, prop, label);
            prop.intValue = Mathf.Clamp(prop.intValue, minValue, maxValue);
            EditorGUI.EndProperty();
        }
#endif
}    

}