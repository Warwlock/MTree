using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
#endif
public enum EnumTextureSize
{
    _512, _1024, _2048, _4096
}

namespace Mtree
{
    public class BranchEditor : EditorWindow
    {

        #region Exposed variables
        public Texture2D leafTexture, normalLeafTexture;
        public Texture2D barkTexture, normalBarkTexture;
        bool switchShader = false;
        int NormalMode = 0;
        Vector3 NormalRotation = new Vector3(0, 0, 0);
        Color barkColor = Color.white;
        int branchNumber = 5;
        float stemLength = 1.5f;
        float length = 0.5f;
        float radius = .01f;
        float angle = .4f;
        float randomness = .2f;
        float splitProba = .05f;
        int textureSize = 1024;
        EnumTextureSize texSize = EnumTextureSize._1024;
        int leafNumber = 30;
        float leafCovering = .7f;
        float leafLength = 1f;
        float leafSize = .7f;
        float leafAngle = 45f;
        float cutOff = .5f;
        Vector3 hsv = new Vector3(0, 1, 1);
        float leafColorVariation = .1f;
        float fakeShading = .3f;
        bool deadLeafs,deadLeafsRendering;
        int leafRotation = 0;

        #endregion

        #region Editor variables
        GameObject branchObject;
        GameObject colliderObject;
        GameObject cameraObject;
        Camera cam;
        Texture2D texture;
        Mesh leafMesh;
        int seed = 21;
        bool showColorOptions = true;
        Vector2 scrollPos;
        #endregion

        #region Editor cosmetic variables
        Color backgroundColor = new Color(.25f, .255f, .26f);
        Color inspectorColor = new Color(.8f, .8f, .8f, .8f);
        int parametersWidth = 250;
        int texturePadding = 10;
        #endregion

#if UNITY_2018_3_OR_NEWER
        #region Pipeline Switch
        static RenderPipelineAsset renderPipelineAsset;
        #endregion
#endif




        [MenuItem("Window/Mtree/Open Branch Editor...")]
        static void Start()
        {
            BranchEditor editor = GetWindow(typeof(BranchEditor)) as BranchEditor;
            editor.Init();
            editor.Show();
#if UNITY_2018_3_OR_NEWER
            renderPipelineAsset = GraphicsSettings.defaultRenderPipeline;
            GraphicsSettings.defaultRenderPipeline = null;
#endif
        }

        void Init()
        {
            seed = Random.Range(0, 100);
            UpdateBranch();
        }


        void OnGUI()
        {
            GUI.color = backgroundColor;
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), EditorGUIUtility.whiteTexture);

            if (texture != null)
            {
                float sizeReduction = Mathf.Min((position.width - parametersWidth - texturePadding * 2) / texture.width, (position.height - texturePadding * 2) / texture.height);
                float posX = ((position.width - parametersWidth) - texture.width * sizeReduction) / 2;
                float posY = position.height - texturePadding - texture.height * sizeReduction;
                GUI.color = backgroundColor * .8f;
                GUI.DrawTexture(new Rect(posX, posY, texture.width * sizeReduction, texture.height * sizeReduction), EditorGUIUtility.whiteTexture);
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(posX, posY, texture.width * sizeReduction, texture.height * sizeReduction), texture);
            }


            BeginWindows();
            GUI.color = inspectorColor;
            Rect parametersRect = new Rect(position.width - parametersWidth,15, parametersWidth, position.height-15);
#if UNITY_2018_3_OR_NEWER
            Rect pipelinesRect = new Rect(position.width - parametersWidth,0, parametersWidth, position.height);
            var gs = new GUIStyle();
            gs.fontStyle = FontStyle.Italic;
            gs.normal.textColor = Color.red;
            gs.alignment = TextAnchor.UpperCenter;
            if(renderPipelineAsset != null){
                GUI.Label(pipelinesRect,"Active SRP: "+ renderPipelineAsset.name,gs);
            }else{
                gs.normal.textColor = Color.green;
                GUI.Label(pipelinesRect,"Active SRP: Legacy Pipeline",gs);
            }
#endif

            GUI.Window(0, parametersRect, DrawInspector, "Parameters");
            GUI.BringWindowToFront(0);
            EndWindows();
        }

        private void OnDestroy()
        {
            DestoyObjects();

#if UNITY_2018_3_OR_NEWER
            GraphicsSettings.defaultRenderPipeline = renderPipelineAsset;
            renderPipelineAsset = null;
#endif

        }


        void DrawInspector(int id)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Label("Branch Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 80;
            if (GUILayout.Button("Randomize"))
                seed = Random.Range(int.MinValue, int.MaxValue);
            barkTexture = EditorGUILayout.ObjectField("Bark Texture", barkTexture, typeof(Texture2D), false) as Texture2D;
            normalBarkTexture = EditorGUILayout.ObjectField("Bark Normal", normalBarkTexture, typeof(Texture2D), false) as Texture2D;
            barkColor = EditorGUILayout.ColorField("Bark Color", barkColor);
            fakeShading = EditorGUILayout.Slider("Fake Shading Intensity", fakeShading, 0f, 1f);
            stemLength = EditorGUILayout.FloatField("Stem Length", stemLength);
            stemLength = Mathf.Max(stemLength, 0.01f);
            length = EditorGUILayout.FloatField("Branch Length", length);
            length = Mathf.Max(length, 0.01f);
            radius = EditorGUILayout.Slider("Radius", radius, .001f, .1f);
            angle = EditorGUILayout.Slider("Split angle", angle, 0f, 1f);
            randomness = EditorGUILayout.FloatField("Randomness", randomness);
            branchNumber = EditorGUILayout.IntSlider("Branch Count", branchNumber, 0, 50);
            splitProba = EditorGUILayout.Slider("Split Proba", splitProba, 0f, .2f);

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label("Leaf Settings", EditorStyles.boldLabel);
            leafTexture = EditorGUILayout.ObjectField("Leaf Texture", leafTexture, typeof(Texture2D), false) as Texture2D;
            normalLeafTexture = EditorGUILayout.ObjectField("Leaf Normal", normalLeafTexture, typeof(Texture2D), false) as Texture2D;
            //leafRotation = EditorGUILayout.Slider("Texture Rotation", leafRotation, -180, 180);

            GUILayout.Label("Texture alignment");
            leafRotation = GUILayout.SelectionGrid(leafRotation, new string[] { "Y", "X", "-Y", "-X" }, 4);
            showColorOptions = EditorGUILayout.Foldout(showColorOptions, "Shader Parameters");
            if (showColorOptions)
            {
                cutOff = EditorGUILayout.Slider("Cut Off", cutOff, 0f, 1f);
                leafColorVariation = EditorGUILayout.Slider("Color Variation", leafColorVariation, 0f, 1f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                hsv.x = EditorGUILayout.Slider("Hue", hsv.x, 0f, 1f);
                hsv.y = EditorGUILayout.Slider("Saturation", hsv.y, 0f, 2f);
                hsv.z = EditorGUILayout.Slider("Value", hsv.z, 0f, 2f);
                EditorGUILayout.EndVertical();
            }
            leafNumber = EditorGUILayout.IntField("Leaf Count", leafNumber);
            leafNumber = Mathf.Max(0, leafNumber);
            leafCovering = EditorGUILayout.Slider("Leaf Covering", leafCovering, 0f, 1f);
            leafLength = EditorGUILayout.FloatField("Leaf Length", leafLength);
            leafLength = Mathf.Max(0, leafLength);
            leafSize = EditorGUILayout.FloatField("Leaf Size", leafSize);
            leafSize = Mathf.Max(0, leafSize);
            leafAngle = EditorGUILayout.Slider("Leaf Angle", leafAngle, 0f, 90f);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Normal Map Mode", Guistyle("b", 10));

            if (normalBarkTexture != null && normalLeafTexture != null)
            {
                NormalMode = GUILayout.Toolbar(NormalMode, new string[] { "Default", "Enhanced" });
            }
            else
            {
                NormalMode = GUILayout.Toolbar(NormalMode, new string[] { "Default" });
                NormalMode = 0;
            }
            NormalRotation = EditorGUILayout.Vector3Field("Normal Rotation", NormalRotation);

            string s_swtichShader = "";
            if (switchShader)
            {
                s_swtichShader = "Show Textures";
            }
            else
            {
                s_swtichShader = "Show Normals";
            }
            if (GUILayout.Button(s_swtichShader))
            {
                switchShader = !switchShader;
            }

            EditorGUILayout.Space();
            texSize = (EnumTextureSize)EditorGUILayout.EnumPopup("Texture Size:", texSize);
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                CheckTextureType();
                UpdateBranch(switchShader);
            }
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Texture Output",Guistyle("b"));
            deadLeafs = GUILayout.Toggle(deadLeafs, new GUIContent("Dead Leafs"));
            EditorGUILayout.EndVertical();


            if (GUILayout.Button("Save Texture"))
            {
                AssetDatabase.StartAssetEditing();
                ExportTextures();
                AssetDatabase.StopAssetEditing();
                //SaveTexture();
                
            }

            EditorGUILayout.EndScrollView();
        }
        /// <summary>
        /// Fontstyle: "n" = Normal | "i" = Italic | "b" = Bold | "bi" = Bold and Italic
        /// </summary>
        GUIStyle Guistyle(string style = "n", int size = 12)
        {
            var gs = new GUIStyle();
            switch (style)
            {
                case "n":
                    gs.fontStyle = FontStyle.Normal;
                    break;
                case "i":
                    gs.fontStyle = FontStyle.Italic;
                    break;
                case "b":
                    gs.fontStyle = FontStyle.Bold;
                    break;
                case "bi":
                    gs.fontStyle = FontStyle.BoldAndItalic;
                    break;
            }
            gs.fontSize = size;
            return gs;
        }
        void UpdateBranch(bool normalShader = false)
        {
            switch (texSize)
            {
                case EnumTextureSize._512:
                    textureSize = 512;
                    break;
                case EnumTextureSize._1024:
                    textureSize = 1024;
                    break;
                case EnumTextureSize._2048:
                    textureSize = 2048;
                    break;
                case EnumTextureSize._4096:
                    textureSize = 4096;
                    break;
            }

            if (branchObject == null)
                CreateBranchObject(normalShader);
            if(!deadLeafsRendering)
                CreateLeafMesh();
            MTree branch = new MTree(branchObject.transform);
            TrunkFunction trunkFunction = ScriptableObject.CreateInstance<TrunkFunction>();
            trunkFunction.Init(null);

            float resolution = 20;

            Random.InitState(seed);


            branch.AddTrunk(Vector3.up, Vector3.forward, stemLength, AnimationCurve.Linear(0, 1, 1, .3f), radius, resolution, randomness / 3, 0, AnimationCurve.Linear(0, 1, 0, 0), 0, .01f, 1, 0);
            branch.TwigSplit(branchNumber, angle*90f, .9f, 0f);
            branch.Grow(length, AnimationCurve.Linear(0, 1, 1, 1), resolution, splitProba, AnimationCurve.Linear(0, 1, 1, 1), angle, 2, 1, 1, randomness, AnimationCurve.Linear(0, 1, 1, .4f), .9f, 0f, 0f, 1f, 0.00001f, obstacleAvoidance:false);
            if(!deadLeafsRendering)
            branch.AddLeafs(leafCovering, leafNumber, new Mesh[] { leafMesh }, leafSize, false, 0, 0, 1, leafAngle, false, 0, 0, 0, 0);
            Mesh mesh = CreateBranchMesh(branch, trunkFunction);

            branchObject.GetComponent<MeshFilter>().mesh = mesh;

            if (cameraObject == null)
                CreateCameraObject();
            RenderCamera();

            DestoyObjects();
        }


        void CreateBranchObject(bool normalShader)
        {
            branchObject = new GameObject("Branch Editor");
            branchObject.transform.position = Vector3.down * 200;
            branchObject.AddComponent<MeshFilter>();
            MeshRenderer rend = branchObject.AddComponent<MeshRenderer>();
            Material barkMaterial = new Material(Shader.Find("Hidden/BranchEditor/BranchEditor_Bark"));
            Material leafMaterial = new Material(Shader.Find("Hidden/BranchEditor/BranchEditor_Leafs"));

            if (normalShader)
            {
                if (NormalMode == 0)
                {
                    barkMaterial = new Material(Shader.Find("Hidden/BranchEditor/BranchEditor_Normal"));
                    leafMaterial = new Material(Shader.Find("Hidden/BranchEditor/BranchEditor_Normal"));
                }
                if (NormalMode == 1)
                {
                    barkMaterial = new Material(Shader.Find("Hidden/BranchEditor/BranchEditor_EnhancedNormal"));
                    leafMaterial = new Material(Shader.Find("Hidden/BranchEditor/BranchEditor_EnhancedNormal"));
                    barkMaterial.SetVector("_Rotation", NormalRotation + new Vector3(90, 0, 0));
                    leafMaterial.SetVector("_Rotation", NormalRotation + new Vector3(90, 0, 0));
                }
                barkMaterial.SetTexture("_BumpMap", normalBarkTexture);
                leafMaterial.SetTexture("_BumpMap", normalLeafTexture);
            }
            else
            {
                barkMaterial = new Material(Shader.Find("Hidden/BranchEditor/BranchEditor_Bark"));
                leafMaterial = new Material(Shader.Find("Hidden/BranchEditor/BranchEditor_Leafs")); barkMaterial.SetTexture("_MainTex", barkTexture);
                barkMaterial.SetFloat("_FakeShading", fakeShading);
                barkMaterial.SetColor("_Color", barkColor);
                leafMaterial.SetFloat("_HueShift", hsv.x);
                leafMaterial.SetFloat("_Saturation", hsv.y);
                leafMaterial.SetFloat("_Value", hsv.z);
                leafMaterial.SetFloat("_ColorVariation", leafColorVariation);
            }
            leafMaterial.SetTexture("_MainTex", leafTexture);
            leafMaterial.SetFloat("_Cutoff", cutOff);
            rend.sharedMaterials = new Material[] { barkMaterial, leafMaterial };

            colliderObject = new GameObject("collider");
            colliderObject.transform.position = branchObject.transform.position + Vector3.back;
            BoxCollider col = colliderObject.AddComponent<BoxCollider>();
            col.size = new Vector3(5, 5, .01f);

        }


        void CreateCameraObject()
        {
            cameraObject = Instantiate(Resources.Load("Mtree/MtreeBillboardCamera") as GameObject);
            cameraObject.transform.position = branchObject.transform.position;
            cameraObject.transform.rotation = Quaternion.Euler(90, 0, 0);

            cam = cameraObject.GetComponent<Camera>();
            cam.orthographic = true;

            Bounds bb = branchObject.GetComponent<Renderer>().bounds;
            Vector3 position = bb.center;
            position.x = branchObject.transform.position.x; ;
            float width = Mathf.Abs(position.x - bb.center.x) + bb.extents.x;
            cam.transform.position = position;
            cam.nearClipPlane = -bb.extents.y;
            cam.farClipPlane = bb.extents.y;
            cam.orthographicSize = bb.extents.z;
            cam.aspect = width / bb.extents.z;
            Color color = backgroundColor;
            color.a = 0f;
            cam.backgroundColor = color;
            cam.clearFlags = CameraClearFlags.SolidColor;
        }


        void RenderCamera()
        {
            RenderTexture currentRT = RenderTexture.active;
            int sizeY = textureSize;
            int sizeX = (int)(textureSize * cam.aspect);
            RenderTexture camText = new RenderTexture(sizeX, sizeY, 16, RenderTextureFormat.ARGB32);
            cam.targetTexture = camText;
            RenderTexture.active = cam.targetTexture;
            cam.Render();
            Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height, TextureFormat.RGBA32, false);
            image.name = "Branch";
            image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
            image.Apply();
            RenderTexture.active = currentRT;
            texture = image;
        }


        Mesh CreateBranchMesh(MTree branch, TrunkFunction trunkFunction)
        {
            Mesh mesh = new Mesh();
            branch.GenerateMeshData(trunkFunction, 0, 1, 1, 1);
            mesh.vertices = branch.verts;
            mesh.normals = branch.normals;
            mesh.uv = branch.uvs;
            Color[] colors = branch.colors;
            mesh.triangles = branch.triangles;
            if (branch.leafTriangles.Length > 0)
            {
                mesh.subMeshCount = 2;
                mesh.SetTriangles(branch.leafTriangles, 1);
            }
            mesh.colors = colors;
            return mesh;
        }


        void DestoyObjects()
        {
            if (branchObject != null)
                DestroyImmediate(branchObject);
            if (cameraObject != null)
                DestroyImmediate(cameraObject);
            if (colliderObject != null)
                DestroyImmediate(colliderObject);
        }


        void CreateLeafMesh()
        {
            Vector3[] verts = new Vector3[4] { Vector3.left / 2, Vector3.left / 2 + Vector3.forward * leafLength, Vector3.right / 2 + Vector3.forward * leafLength, Vector3.right / 2 };
            Quaternion rot = Quaternion.Euler(0, leafRotation * 90, 0);
            Matrix4x4 trans = Matrix4x4.Translate(new Vector3(0, 0, -leafLength / 2));
            Matrix4x4 transInv = trans.inverse;

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = trans.MultiplyPoint(verts[i]);
                verts[i] = rot * verts[i];
                verts[i] = transInv.MultiplyPoint(verts[i]);
            }
            Vector3[] normals = new Vector3[4] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            Vector2[] uvs = new Vector2[4] { Vector2.zero, Vector2.up, Vector2.up + Vector2.right, Vector2.right };
            int[] triangles = new int[6] { 0, 1, 3, 1, 2, 3 };

            leafMesh = new Mesh();
            leafMesh.vertices = verts;
            leafMesh.normals = normals;
            leafMesh.uv = uvs;
            leafMesh.triangles = triangles;
        }


        void ExportTextures()
        {
           
            UpdateBranch();
            string path = SaveTexture();
            string name = Path.GetFileNameWithoutExtension(path);
            var nameDB = name;
            name += "_Normal.png";
            path = path.Replace(Path.GetFileName(path), name);
            UpdateBranch(true);
            SaveTexture(path);
            if (deadLeafs)
            {
                deadLeafsRendering = true;
                name = nameDB + "_DeadLeafs.png";
                path = path.Replace(Path.GetFileName(path), name);
                UpdateBranch();
                SaveTexture(path);

                deadLeafsRendering = true;
                name = nameDB + "_DeadLeafs_Normal.png";
                path = path.Replace(Path.GetFileName(path), name);
                UpdateBranch(true);
                SaveTexture(path);
                deadLeafsRendering = false;
            }
            UpdateBranch();

        }
        void CheckTextureType()
        {
            // var b_path = AssetDatabase.GetAssetPath(normalBarkTexture);
            // var l_path = AssetDatabase.GetAssetPath(normalLeafTexture);
            // TextureImporter b_t = (TextureImporter)TextureImporter.GetAtPath(b_path);
            // TextureImporter l_t = (TextureImporter)TextureImporter.GetAtPath(b_path);
            // if(b_t.textureType != TextureImporterType.NormalMap){
            //     b_t.textureType = TextureImporterType.NormalMap;
            //     b_t.SaveAndReimport();
            // }
            // if(l_t.textureType != TextureImporterType.NormalMap){
            //     l_t.textureType = TextureImporterType.NormalMap;
            //     l_t.SaveAndReimport();
            // }
        }

        string SaveTexture(string path = null)
        {
            if (path == null)
            {
                path = EditorUtility.SaveFilePanelInProject("Save png", texture.name + ".png", "png", "Please enter a file name to save the texture to");
            }
            if (path.Length != 0)
            {
                Utils.DilateTexture(texture, 100); 
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
            }
            return path;
        }

        private void RemoveBackground(ref Color[] colors)
        {
            Color backgroundColor = new Color(0.025f, 0.07f, 0.19f);
            for (int i = 0; i < colors.Length; i++)
            {
                Color c = colors[i];
                if (Mathf.Abs(c.r - backgroundColor.r) + Mathf.Abs(c.g - backgroundColor.g) + Mathf.Abs(c.b - backgroundColor.b) < .05)
                {
                    colors[i] = new Color(0, 0, 0, 0);
                }
            }
        }
    }
}