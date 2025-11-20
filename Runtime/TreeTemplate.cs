
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Mtree
{
    public class TreeTemplate : ScriptableObject
    {
#if UNITY_EDITOR
        public List<TreeFunctionAsset> treeFunctions;

        public static TreeTemplate CreateFromFunctions(List<TreeFunctionAsset> functionsToCopy, string path)
        {
            List<TreeFunctionAsset> treeFunctions = new List<TreeFunctionAsset>();

            for (int i = 0; i < functionsToCopy.Count; i++)
            {
                TreeFunctionAsset function = Instantiate(functionsToCopy[i]);
                treeFunctions.Add(function);
            }

            for (int i = 0; i < functionsToCopy.Count; i++)
            {
                int parentIndex = functionsToCopy.IndexOf(functionsToCopy[i].parent);
                TreeFunctionAsset parent = parentIndex == -1 ? null : treeFunctions[parentIndex];
                treeFunctions[i].Init(parent, true);
            }

            TreeTemplate template = ScriptableObject.CreateInstance<TreeTemplate>();
            template.treeFunctions = treeFunctions;

            AssetDatabase.CreateAsset(template, path);

            template = AssetDatabase.LoadAssetAtPath<TreeTemplate>(path);

            for (int i = 0; i < treeFunctions.Count; i++)
                AssetDatabase.AddObjectToAsset(treeFunctions[i], template);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return template;
        }
#endif
    }
 }