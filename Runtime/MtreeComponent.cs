using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mtree;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
#if UNITY_2018_3 || UNITY_2018_4
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering;
#endif


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MtreeComponent : MonoBehaviour
{
#if UNITY_EDITOR
    public List<TreeFunctionAsset> treeFunctionsAssets;
    public List<LODValues> LODs;

    public int selectedFunctionIndex = 0; // Index of selected Function in editor
    public int tabIndex = 0; // Index of selected tab in editor
    public MTree tree;
    public int LodIndex = 0;
    public string saveTreeFolder = "Assets";
    public int polycount = 0;
    private Material leafMaterial;
    private MeshFilter filter;
    private ThreadHandleEditor aoBakingHandle;
    public TreeTemplate template;
    public int[] MtreeVersion;
    public TreeFunctionAsset savedFunction;
    public float VColBarkModifier = 1;
    public float VColLeafModifier = 1;
    public float BB_Brightness = 1;
    public float BB_TopCardPositon = 0.5f;
    public int BB_hasTopCard = 1;
    public int BB_isSingleSided = 1;

	public MtreeBezier mtreeBezier;
	public int hasBezier = 0;
#if UNITY_2018_3_OR_NEWER
    public bool isHDRP = false;
    public RenderPipelineAsset hdrpAsset;
#endif
    void InitializeTree()
    {
        MtreeVersion = MtreeVariables.MtreeVersion;
        filter = GetComponent<MeshFilter>();
        tree = new MTree(transform);

        if(LODs == null || LODs.Count == 0)
            for (int i = 0; i < 4; i++)
            {
                AddLODLevel(false);
            }
    }

    public void AddLODLevel(bool registerUndo = true)
    {
        LODValues previousLOD = null;
        if (LODs == null || LODs.Count == 0)
            LODs = new List<LODValues>();
        else
            previousLOD = LODs[LODs.Count - 1];

        LODValues lod = ScriptableObject.CreateInstance<LODValues>();
        lod.Init(previousLOD);

        if (registerUndo)
        {
            Undo.RecordObject(this, "Added LOD level");
            Undo.RegisterCreatedObjectUndo(lod, "Added LOD level");
        }

        LODs.Add(lod);
    }

    public void UndoCallBack()
    {
        if (this != null)
            GenerateTree();
    }

    public void AddTreeFunction<T>() where T : TreeFunctionAsset
    {
        T function = ScriptableObject.CreateInstance<T>();
        AddTreeFunction(function);
    }

    private void AddTreeFunction(TreeFunctionAsset function)
    {
        Undo.RecordObjects(treeFunctionsAssets.ToArray(), "Added function");
        Undo.RecordObject(this, "Added function");
        Undo.RegisterCreatedObjectUndo(function, "Added function");

        TreeFunctionAsset parent = treeFunctionsAssets.Count == 0 ? null : treeFunctionsAssets[selectedFunctionIndex];
        function.Init(parent);
        selectedFunctionIndex = treeFunctionsAssets.Count;
        treeFunctionsAssets.Add(function);
        GenerateTree();
    }

    public void RandomizeTree()
    {
        Undo.RecordObjects(treeFunctionsAssets.ToArray(), "Randomized tree");

        foreach (TreeFunctionAsset tf in treeFunctionsAssets)
        {
            tf.seed = Random.Range(0, 1000);
        }
    }

    public void UpdateTreeFunctions()
    {
        if (treeFunctionsAssets == null ||treeFunctionsAssets.Count == 0)
        {
            treeFunctionsAssets = new List<TreeFunctionAsset>();
            AddTreeFunction<TrunkFunction>();
        }
    }

    public void RemoveFunction(TreeFunctionAsset function)
    {
        Undo.RecordObjects(treeFunctionsAssets.ToArray(), "Removed function");
        Undo.RecordObject(this, "Removed function");

        treeFunctionsAssets.Remove(function);
        function.parent.chiildren.Remove(function);
        foreach (TreeFunctionAsset child in function.chiildren)
        {
            child.parent = function.parent;
            child.parent.chiildren.Add(child);
        }
        selectedFunctionIndex = function.parent == null ? 0 : treeFunctionsAssets.IndexOf(function.parent);

        Undo.DestroyObjectImmediate(function);
    }

    private void ExecuteFunctions()
    {
        if (tree == null || LODs == null || LODs.Count == 0 || treeFunctionsAssets == null || treeFunctionsAssets.Count == 0)
            InitializeTree();

        if (treeFunctionsAssets == null || treeFunctionsAssets.Count == 0)
            UpdateTreeFunctions();

        foreach (TreeFunctionAsset function in treeFunctionsAssets)
        {
            function.Execute(tree);
        }
    }

    Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();


        if (treeFunctionsAssets.Count > 0)
            tree.GenerateMeshData(treeFunctionsAssets[0] as TrunkFunction, LODs[LodIndex].simplifyLeafs, LODs[LodIndex].radialResolution,VColBarkModifier,VColLeafModifier);

        mesh.vertices = tree.verts;
        mesh.normals = tree.normals;
        mesh.uv = tree.uvs;
        Color[] colors = tree.colors;
        if (tree.triangles.Length >= System.UInt16.MaxValue)
        {
            mesh.indexFormat = IndexFormat.UInt32;
        }
        else
        {
            mesh.indexFormat = IndexFormat.UInt16;
        }
        mesh.triangles = tree.triangles;
        if (tree.leafTriangles.Length > 0)
        {
            mesh.subMeshCount = 2;
            mesh.SetTriangles(tree.leafTriangles, 1);
        }
        mesh.colors = colors;
        polycount = mesh.triangles.Length / 3;
        return mesh;
    }

    public void FromTemplate()
    {
        if (template == null)
            return;

        treeFunctionsAssets.Clear();

        for (int i = 0; i < template.treeFunctions.Count; i++)
        {
            TreeFunctionAsset function = Instantiate(template.treeFunctions[i]);
            treeFunctionsAssets.Add(function);
        }

        for (int i = 0; i < template.treeFunctions.Count; i++)
        {
            int parentIndex = template.treeFunctions.IndexOf(template.treeFunctions[i].parent);
            TreeFunctionAsset parent = parentIndex == -1 ? null : treeFunctionsAssets[parentIndex];
            treeFunctionsAssets[i].Init(parent, true);
        }
    }

    public void BakeAo(bool async = false)
    {
        if (filter == null ||filter.sharedMesh == null)
        {
            GenerateTree();
        }

        if (aoBakingHandle != null)
            aoBakingHandle.Abort();

        if (async)
            aoBakingHandle = DensityAO.BakeAoAsync(filter, gameObject, 2f, 3f);
        else
            DensityAO.BakeAo(filter, gameObject, 2f, 3f);

    }

    public Mesh GenerateTree(bool instantAo = false)
    {

        tree = null;
        ExecuteFunctions();
        tree.Simplify(LODs[LodIndex].simplifyAngleThreshold, LODs[LodIndex].simplifyRadiusThreshold);
        Mesh mesh = CreateMesh();
        filter.mesh = mesh;
        
        BakeAo(!instantAo);
        UpdateMaterials();
        return mesh;
    }

    public void UpdateMaterials()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (tree.leafTriangles.Length == 0)
        {
            switch (renderer.sharedMaterials.Length)
            {
                case 1:
                    if (renderer.sharedMaterial == null)
                    {
                        Material mat = new Material(Utils.GetBarkShader());
                        renderer.sharedMaterial = mat;
                    }
                    break;

                case 2:
                    leafMaterial = renderer.sharedMaterials[1];
                    renderer.sharedMaterials = new Material[] { renderer.sharedMaterials[0] };
                    break;
            }
        }
        else
        {
            switch (renderer.sharedMaterials.Length)
            {
                case 0:
                    Material barkMat = new Material(Utils.GetBarkShader());
                    renderer.sharedMaterial = barkMat;

                    break;

                case 1:
                    if (leafMaterial == null)
                        leafMaterial = new Material(Utils.GetLeafShader());
                    renderer.sharedMaterials = new Material[] { renderer.sharedMaterials[0], leafMaterial };
                    break;
            }
        }
    }

    private void Start()
    {
        Debug.LogWarning("A tree with Mtree component still attached is used, use trees exported as prefabs in order to optimize your scene. To export a tree, select it, go to the 'save as Prefab' tab and click on 'Save as prefab'");
    }


    public GameObject CreateBillboard(string path, string name, bool usingHDRP)
    {
        ResetGlobalWind();
        GameObject camObject = Instantiate(Resources.Load("Mtree/MtreeBillboardCamera") as GameObject); // create billboard and render it
        Camera cam = camObject.GetComponent<Camera>();
        camObject.GetComponentInChildren<Light>().intensity = BB_Brightness;
        Billboard bill = new Billboard(cam, gameObject, 512, 512, BB_TopCardPositon, BB_hasTopCard == 1, BB_isSingleSided == 1);
        bill.SetupCamera();
        string texturePath = path + name + "_billboard.png";
        bill.Render(texturePath);
        DestroyImmediate(camObject);

        Mesh billboardMesh = bill.CreateMesh(); // create billboard mesh
        AssetDatabase.CreateAsset(billboardMesh, path + name + "_LOD4.mesh");

        GameObject billboard = new GameObject(name + "_LOD4"); // create billboard object and assign mesh
        MeshFilter meshFilter = billboard.AddComponent<MeshFilter>();
        meshFilter.mesh = billboardMesh;
        MeshRenderer meshRenderer = billboard.AddComponent<MeshRenderer>();

        Texture billboardTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)); // create material
        Material mat = bill.CreateMaterial(billboardTexture,usingHDRP);
        meshRenderer.sharedMaterial = mat;
        AssetDatabase.CreateAsset(mat, path + name + "billboard.mat");
        return billboard;
    }

    public Material[] SaveMaterials(string folderPath)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Material[] materialsCopy = new Material[renderer.sharedMaterials.Length];
        int matIndex = 0;
        foreach (Material mat in renderer.sharedMaterials)
        {
            if (AssetDatabase.GetAssetPath(mat).Length == 0)
            {
                string matName = Path.GetFileName(mat.name);
                string matPath = Path.Combine(folderPath, matName + ".mat");
                Material matCopy = new Material(mat);
                materialsCopy[matIndex] = matCopy;
                AssetDatabase.CreateAsset(matCopy, matPath);
            }
            else
            {
                materialsCopy[matIndex] = mat;
            }
            matIndex++;
        }

        return materialsCopy;
    }


    Light[] lights = new Light[0];
    public void ResetGlobalWind()
    {
        Shader.SetGlobalFloat("_WindStrength", 0);
        Shader.SetGlobalFloat("_WindDirection", 0);
        Shader.SetGlobalFloat("_WindPulse", 0);
        Shader.SetGlobalFloat("_WindTurbulence", 0);
    }
    public void SaveAsPrefab(bool groupedSave = false,bool isHDRP = false)
    {
        var oldMesh = filter.sharedMesh;
        string name = gameObject.name;
        string path = saveTreeFolder;
        if (string.IsNullOrEmpty(path))
            return;

        #if (UNITY_2017 || UNITY_2018_1 || UNITY_2018_2)
         bool replacePrefab = false; //=> value never Used, taged by dan_wipf => used for unity up to 2018.2
        #endif

        if (!System.IO.Directory.Exists(path))
        {
            EditorUtility.DisplayDialog("Invalid Path", "The path is not valid, you can chose it with the find folder button", "Ok");
            return;
        }
        if (AssetDatabase.LoadAssetAtPath(path + "/" + name + ".prefab", typeof(GameObject))) // Overriding prefab dialog
        {
            if (EditorUtility.DisplayDialog("Are you sure?", "The prefab already exists. Do you want to overwrite it?", "Yes", "No"))
            {
                FileUtil.DeleteFileOrDirectory(Path.Combine(path, name + "_meshes"));
                #if (UNITY_2017 || UNITY_2018_1 || UNITY_2018_2)
                replacePrefab = true; //  => value never Used, taged by dan_wipf => used for unity up to 2018.2
                #endif
            }
            else
            {
                name += "_1";
            }
        }
        lights = FindObjectsOfType<Light>();
        foreach (var dl in lights)
        {
            dl.enabled = false;
        }
        Mesh[] meshes = new Mesh[LODs.Count];
        string meshesFolder = AssetDatabase.CreateFolder(path, name + "_meshes");
        meshesFolder = AssetDatabase.GUIDToAssetPath(meshesFolder) + Path.DirectorySeparatorChar;
        Material[] materials = SaveMaterials(meshesFolder);
        GameObject TreeObject = new GameObject(name); // Tree game object
        LODGroup group = TreeObject.AddComponent<LODGroup>(); // LOD Group
        group.fadeMode = LODFadeMode.CrossFade;
        LOD[] lods = new LOD[LODs.Count + 1];

        // Generating Billboard 
        LodIndex = LODs.Count - 1;
        GenerateTree(true);
        GameObject billboard = CreateBillboard(meshesFolder, name, isHDRP);
        Renderer[] bill_re = new Renderer[1] { billboard.GetComponent<MeshRenderer>() };
        lods[lods.Length - 1] = new LOD(.01f, bill_re);


        for (LodIndex = LODs.Count - 1; LodIndex >= 0; LodIndex--) // create and save all LOD meshes
        {
            string meshPath = meshesFolder + name + "_LOD" + LodIndex + ".mesh"; //updating path for each LOD
            Mesh mesh = GenerateTree(instantAo:true);
            meshes[LodIndex] = mesh;
            AssetDatabase.CreateAsset(mesh, meshPath);
        }

        for (int i = 0; i < LODs.Count; i++) // assigning lod meshes to LOD array
        {
            GameObject go = new GameObject(name + "_LOD" + i.ToString());
            go.transform.parent = TreeObject.transform;
            MeshFilter mf = go.AddComponent<MeshFilter>();
            mf.mesh = meshes[i];
            Renderer[] re = new Renderer[1] { go.AddComponent<MeshRenderer>() }; // the renderer to put in LODs
            re[0].sharedMaterials = materials;
            float t = Mathf.Pow((i + 1) * 1f / (LODs.Count + 1), 1); // float between 0 and 1 following f(x) = pow(x, n)
            lods[i] = new LOD((1 - t) * 0.9f + t * .01f, re); // assigning renderer
            lods[i].fadeTransitionWidth = 0.25f;
        }

        billboard.transform.parent = TreeObject.transform; // making billboard child of tree object


        group.SetLODs(lods); // assigning LODs to lod group
        group.animateCrossFading = true;
        group.RecalculateBounds();

        string prefabPath = path + "/" + name + ".prefab";

        Object prefab;

        #if (UNITY_2017 || UNITY_2018_1 || UNITY_2018_2)
        if (replacePrefab)
        {
            Object targetPrefab = AssetDatabase.LoadAssetAtPath(path + "/" + name + ".prefab", typeof(GameObject));
            prefab = PrefabUtility.ReplacePrefab(TreeObject, targetPrefab, ReplacePrefabOptions.ConnectToPrefab);
        }
        else
            prefab = PrefabUtility.CreatePrefab(prefabPath, TreeObject, ReplacePrefabOptions.ConnectToPrefab);
        #else 
        prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(TreeObject, prefabPath, InteractionMode.AutomatedAction);
#endif
        
        AssetDatabase.SaveAssets();
        DestroyImmediate(TreeObject);
        if (!groupedSave)
        {
            // select newly created prefab in folder
            Selection.activeObject = prefab;
            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(prefab);
            EditorUtility.DisplayDialog("Prefab saved !", "The prefab is saved, you can now delete the tree and use the prefab instead", "Ok");
        }

        LodIndex = 0;
        filter.sharedMesh = oldMesh;
        foreach (var dl in lights)
        {
            dl.enabled = true;
        }
    }
	public void BezierManager(){
		if (hasBezier == 1) {
			mtreeBezier = gameObject.AddComponent<MtreeBezier> ();
			mtreeBezier.MTreeDoBezier = true;
			GenerateTree ();
		}
		if (mtreeBezier == null)
			mtreeBezier = GetComponent<MtreeBezier> ();
		
		if (hasBezier == 0 && mtreeBezier != null) {
			mtreeBezier.MTreeDoBezier = false;
			mtreeBezier.MTreeLeafDirection = false;
			DestroyImmediate (mtreeBezier);
			GenerateTree ();
			}

			
	}
    
    #endif
}