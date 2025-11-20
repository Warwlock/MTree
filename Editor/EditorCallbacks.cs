using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace Mtree
{

    [InitializeOnLoad]
    public class EditorCallbacks
    {
        static EditorCallbacks()
        {
            PrefabUtility.prefabInstanceUpdated += OnCreatePrefab;
        }

        public static void OnCreatePrefab(GameObject instance)
        {
            if (instance.GetComponent<MtreeComponent>() == null)
                return;

            #if UNITY_2018_3_OR_NEWER
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instance);
            #else
            string prefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(instance));
            #endif
                MtreeComponent[] trees = (MtreeComponent[])GameObject.FindObjectsOfType(typeof(MtreeComponent));
            MtreeComponent originTree = null;
            foreach (MtreeComponent tree in trees)
            {
                #if UNITY_2018_3_OR_NEWER
                bool isInstance = PrefabUtility.GetPrefabInstanceStatus(tree) == PrefabInstanceStatus.Connected;
                string parentPrefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(tree));
                #else
                bool isInstance = PrefabUtility.GetPrefabType(tree) == PrefabType.PrefabInstance;
                string parentPrefabPath = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(tree));
                #endif
                if (isInstance && parentPrefabPath == prefabPath)
                {
                    #if UNITY_2018_3_OR_NEWER
                    PrefabUtility.UnpackPrefabInstance(tree.gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    #else
                    PrefabUtility.DisconnectPrefabInstance(tree);
                    #endif
                    originTree = tree;
                }
            }


            #if UNITY_2018_3_OR_NEWER // Unity 2017 crashes when deleting prefab at this stage. This part is therefore also done in the MtreeComponent editor
            AssetDatabase.DeleteAsset(prefabPath);
            AssetDatabase.Refresh();
            #endif

            if (originTree == null)
                return;

            string templatePath = AssetDatabase.GenerateUniqueAssetPath(Path.GetDirectoryName(prefabPath) + "/" + originTree.name + ".asset");
            TreeTemplate template = TreeTemplate.CreateFromFunctions(originTree.treeFunctionsAssets, templatePath);
            originTree.template = template;
        }
    }

}
