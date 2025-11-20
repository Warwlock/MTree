using UnityEngine;
using UnityEditor;

public class MenuItems
{
    [MenuItem("GameObject/3D Object/Create new Mtree...")]
    private static void CreateNewMtree()
    {
        GameObject tree = new GameObject("New Mtree");
        MtreeComponent mtree = tree.AddComponent<MtreeComponent>();
        mtree.GenerateTree();
        Selection.activeGameObject = tree;
        if (SceneView.lastActiveSceneView != null)
        {
            tree.gameObject.transform.position = SceneView.lastActiveSceneView.pivot;
        }

    }
    [MenuItem("Window/Mtree/Add Mtree Global Wind Controller To Scene...")]
    private static void CreateGlobalMtreeWindController()
    {
        GameObject WindController = new GameObject("Mtree Wind Controller");
        WindController.AddComponent<WindZone>();
        WindController.AddComponent<MtreeWind>();
        if (SceneView.lastActiveSceneView != null)
        {
            WindController.transform.position = Vector3.zero;
        }

    }
}
