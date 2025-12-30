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
#if UNITY_6000_2_OR_NEWER
    public bool useMeshLOD = true;
    public float slope = 2f, bias = 18.5f;
#endif
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

        if (LODs == null || LODs.Count == 0)
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
        if (treeFunctionsAssets == null || treeFunctionsAssets.Count == 0)
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

    Mesh CreateMesh(float simplifyLeafs, float radialResolution)
    {
        Mesh mesh = new Mesh();

        if (treeFunctionsAssets.Count > 0)
            tree.GenerateMeshData(treeFunctionsAssets[0] as TrunkFunction, simplifyLeafs, radialResolution, VColBarkModifier, VColLeafModifier);

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
        if (filter == null || filter.sharedMesh == null)
        {
            GenerateTree();
        }

        if (aoBakingHandle != null)
            aoBakingHandle.Abort();

        if (async)
            aoBakingHandle = DensityAO.BakeAoAsync(filter, gameObject, 3f, 3f);
        else
            DensityAO.BakeAo(filter, gameObject, 0.5f, 3f);

    }

    public Mesh GenerateTree(bool instantAo = false)
    {
        return GenerateTree(LODs[LodIndex].radialResolution, LODs[LodIndex].simplifyAngleThreshold,
            LODs[LodIndex].simplifyRadiusThreshold, LODs[LodIndex].simplifyLeafs, instantAo);
    }

    public Mesh GenerateTree(float radialResolution, float simplifyAngleThreshold, float simplifyRadiusThreshold, float simplifyLeafs, bool instantAo = false)
    {
        tree = null;
        ExecuteFunctions();
        tree.Simplify(simplifyAngleThreshold, simplifyRadiusThreshold);
        Mesh mesh = CreateMesh(simplifyLeafs, radialResolution);
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
        Material mat = bill.CreateMaterial(billboardTexture, usingHDRP);
        meshRenderer.sharedMaterial = mat;
        AssetDatabase.CreateAsset(mat, path + name + "billboard.mat");
        return billboard;
    }

    public void CreateBillboardMeshLOD(string path, string name, bool usingHDRP, out Mesh bbMesh, out Material bbMaterial)
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

        bbMesh = bill.CreateMesh();

        Texture billboardTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)); // create material
        Material mat = bill.CreateMaterial(billboardTexture, usingHDRP);
        AssetDatabase.CreateAsset(mat, path + name + "billboard.mat"); ;
        bbMaterial = mat;
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
    public void SaveAsPrefab(bool groupedSave = false, bool isHDRP = false)
    {
        StartCoroutine(SaveAsPrefabCoroutine(groupedSave, isHDRP));
    }
    IEnumerator SaveAsPrefabCoroutine(bool groupedSave = false, bool isHDRP = false)
    {
        int layer = gameObject.layer;
        gameObject.layer = 31;

        yield return new WaitForSeconds(1f); // If we don't wait for a while, camera doesnt render the layer

        var oldMesh = filter.sharedMesh;
        string name = gameObject.name;
        string path = saveTreeFolder;
        Debug.Log(path);
        if (string.IsNullOrEmpty(path))
            yield break;

        if (!System.IO.Directory.Exists(path))
        {
            EditorUtility.DisplayDialog("Invalid Path", "The path is not valid, you can chose it with the find folder button", "Ok");
            yield break;
        }
        if (AssetDatabase.LoadAssetAtPath(path + "/" + name + ".prefab", typeof(GameObject))) // Overriding prefab dialog
        {
            if (EditorUtility.DisplayDialog("Are you sure?", "The prefab already exists. Do you want to overwrite it?", "Yes", "No"))
            {
                FileUtil.DeleteFileOrDirectory(Path.Combine(path, name + "_data"));
            }
            else
            {
                name += "_1";
            }
        }

#if UNITY_6000_0_OR_NEWER
        lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
        lights = FindObjectsOfType<Light>();
#endif

        foreach (var dl in lights)
        {
            dl.enabled = false;
        }

        GameObject TreeObject = new GameObject(name); // Tree game object

        if (useMeshLOD)
            MeshLODCreator(isHDRP, path, name, TreeObject); // MeshLOD, Supports GRD
        else
            LegacyCreator(isHDRP, path, name, TreeObject); // LOD Group Creator, Doesn't support GPU Resident Drawer (GRD)


        string prefabPath = path + "/" + name + ".prefab";

        Object prefab;
        prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(TreeObject, prefabPath, InteractionMode.AutomatedAction);

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

        gameObject.layer = layer;
    }

    void MeshLODCreator(bool isHDRP, string path, string name, GameObject TreeObject)
    {
        Mesh newMesh = new Mesh();
        newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        string dataFolder = AssetDatabase.CreateFolder(path, name + "_data");
        dataFolder = AssetDatabase.GUIDToAssetPath(dataFolder) + Path.DirectorySeparatorChar;
        Material[] materials = SaveMaterials(dataFolder);

        GenerateTree(true);
        CreateBillboardMeshLOD(dataFolder, name, isHDRP, out Mesh bbMesh, out Material bbMat);

        var materialsList = new List<Material>();
        materialsList.AddRange(materials);
        materialsList.Add(bbMat);
        TreeObject.AddComponent<MeshFilter>().sharedMesh = newMesh;
        TreeObject.AddComponent<MeshRenderer>().SetSharedMaterials(materialsList);
        newMesh.subMeshCount = 3;

        int lodCount = LODs.Count;
        Mesh[] meshes = new Mesh[lodCount];
        uint[] startIndex0 = new uint[lodCount + 1];
        uint[] startIndex1 = new uint[lodCount + 1];
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();
        List<int> indices0 = new List<int>();
        List<int> indices1 = new List<int>();
        List<int> indices2 = new List<int>();

        for (int i = 0; i < meshes.Length; i++) // MeshLOD, It doesnt use existing vertices, it adds vertices for each LOD
        {
            LodIndex = i;
            meshes[i] = GenerateTree(instantAo: true);

            int vertCount = verts.Count;
            verts.AddRange(meshes[i].vertices);
            normals.AddRange(meshes[i].normals);
            uvs.AddRange(meshes[i].uv);
            colors.AddRange(meshes[i].colors);

            startIndex0[i] = System.Convert.ToUInt32(indices0.Count);
            startIndex1[i] = System.Convert.ToUInt32(indices1.Count);

            indices0.AddRange(OffsetIndices(meshes[i].GetIndices(0), vertCount));
            indices1.AddRange(OffsetIndices(meshes[i].GetIndices(1), vertCount));
        }
        startIndex0[lodCount] = System.Convert.ToUInt32(indices0.Count);
        startIndex1[lodCount] = System.Convert.ToUInt32(indices1.Count);

        indices2.AddRange(OffsetIndices(bbMesh.GetIndices(0), verts.Count)); // Billboard Mesh Data
        verts.AddRange(bbMesh.vertices);
        normals.AddRange(bbMesh.normals);
        uvs.AddRange(bbMesh.uv);
        for (int i = 0; i < bbMesh.vertices.Length; i++)
        {
            colors.Add(new Color(1, 1, 1, 0)); // There is no color data in billboard mesh, but we have to add it to match verts length
        }

        newMesh.SetVertices(verts);
        newMesh.SetNormals(normals);
        newMesh.SetUVs(0, uvs);
        newMesh.SetColors(colors);
        newMesh.SetTriangles(indices0, 0);
        newMesh.SetTriangles(indices1, 1);
        newMesh.SetTriangles(indices2, 2);

        newMesh.lodCount = lodCount + 1;

        for (int i = 0; i < newMesh.lodCount; i++)
        {
            if (i == lodCount) // Billboard LOD
            {
                newMesh.SetLod(0, lodCount, new MeshLodRange(0, 0));
                newMesh.SetLod(1, lodCount, new MeshLodRange(0, 0));
                newMesh.SetLod(2, lodCount, new MeshLodRange(0, (uint)indices2.Count));
                break;
            }
            newMesh.SetLod(0, i, new MeshLodRange(startIndex0[i], startIndex0[i + 1] - startIndex0[i]));
            newMesh.SetLod(1, i, new MeshLodRange(startIndex1[i], startIndex1[i + 1] - startIndex1[i]));
            newMesh.SetLod(2, i, new MeshLodRange(0, 0));
        }
        newMesh.lodSelectionCurve = new Mesh.LodSelectionCurve(slope, bias);
        newMesh.RecalculateBounds();

        string meshPath = dataFolder + name + "_MeshLOD" + ".mesh"; // Save mesh
        AssetDatabase.CreateAsset(newMesh, meshPath);
    }

    int[] OffsetIndices(int[] indices, int offset)
    {
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] += offset;
        }
        return indices;
    }

    void LegacyCreator(bool isHDRP, string path, string name, GameObject TreeObject)
    {
        Mesh[] meshes = new Mesh[LODs.Count];
        string dataFolder = AssetDatabase.CreateFolder(path, name + "_data");
        dataFolder = AssetDatabase.GUIDToAssetPath(dataFolder) + Path.DirectorySeparatorChar;
        Material[] materials = SaveMaterials(dataFolder);
        LODGroup group = TreeObject.AddComponent<LODGroup>(); // LOD Group
        group.fadeMode = LODFadeMode.CrossFade;
        LOD[] lods = new LOD[LODs.Count + 1];

        // Generating Billboard
        LodIndex = LODs.Count - 1;
        GenerateTree(true);
        GameObject billboard = CreateBillboard(dataFolder, name, isHDRP);
        Renderer[] bill_re = new Renderer[1] { billboard.GetComponent<MeshRenderer>() };
        lods[lods.Length - 1] = new LOD(.01f, bill_re);


        for (LodIndex = LODs.Count - 1; LodIndex >= 0; LodIndex--) // create and save all LOD meshes
        {
            string meshPath = dataFolder + name + "_LOD" + LodIndex + ".mesh"; //updating path for each LOD
            Mesh mesh = GenerateTree(instantAo: true);
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
    }

    public void BezierManager()
    {
        if (hasBezier == 1)
        {
            mtreeBezier = gameObject.AddComponent<MtreeBezier>();
            mtreeBezier.MTreeDoBezier = true;
            GenerateTree();
        }
        if (mtreeBezier == null)
            mtreeBezier = GetComponent<MtreeBezier>();

        if (hasBezier == 0 && mtreeBezier != null)
        {
            mtreeBezier.MTreeDoBezier = false;
            mtreeBezier.MTreeLeafDirection = false;
            DestroyImmediate(mtreeBezier);
            GenerateTree();
        }


    }

#endif
}