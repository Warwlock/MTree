using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Mtree;
public class VertexColorAdjustmentEditor: EditorWindow
{
    SerializedObject serialObj;
    SerializedProperty serialProp;
    [SerializeField] List<GameObject> gameObjects = new List<GameObject>();
    Color color = Color.magenta;
    float RandomRange = 0.5f;
    bool overrideShaders = false;
    int select = -1;
    static Texture2D black,green;
    Vector2 scroll;
    [MenuItem("Window/Mtree/Open Vertex Color Adjustment Editor...")]
    static void ShowWindow()
    {
        black = BackgroundTexture(Color.black);
        green = BackgroundTexture(new Color(0.2f,0.7f,0,0.5f));
        
        GetWindowWithRect<VertexColorAdjustmentEditor>(new Rect(0,0,512,512));    
    }

    void OnGUI()
    {
        var header = new GUIStyle();
        header.fontStyle = FontStyle.Bold;
        header.normal.textColor = Color.white;

        var bg = new GUIStyle(GUI.skin.box);
        bg.normal.background = black;
        
        EditorGUILayout.BeginHorizontal(bg);
        EditorGUILayout.LabelField("Vertex Color Adjustment Tool",header);
        EditorGUILayout.EndHorizontal();

        scroll = EditorGUILayout.BeginScrollView(scroll,false,false);
        DrawSettings();
        DrawButtons();
        EditorGUILayout.EndScrollView();
        
        

    }
    void Awake(){
        ScriptableObject scriptableObj = this;
        serialObj = new SerializedObject (scriptableObj);
        serialProp = serialObj.FindProperty ("gameObjects");
    }
    void DrawSettings(){

        var bg = new GUIStyle(GUI.skin.box);
        bg.normal.background = green;

        EditorGUILayout.BeginVertical(bg);

        
       
        
        EditorGUILayout.PropertyField (serialProp,new GUIContent("GameObjects"), true);
        serialObj.ApplyModifiedProperties ();
        
        color = EditorGUILayout.ColorField("Vertex Color",color);
        RandomRange = EditorGUILayout.Slider("Random Range",RandomRange,0,1);
        EditorGUILayout.EndVertical();
    }
    
    void DrawButtons(){
        var gs = new GUIStyle(GUI.skin.button);
        gs.richText = true;

        var bg = new GUIStyle(GUI.skin.box);
        bg.normal.background = green;
        
        

        EditorGUILayout.BeginVertical(bg);
            
        if(!overrideShaders){
            if(GUILayout.Button("<b>Enable Override Mesh Options!</b> <i>(No Undo)</i>",gs))
                overrideShaders = true;
            }
        
        if(!overrideShaders)
            EditorGUI.BeginDisabledGroup(true);
            OverrideShaders();
        

        if(GUILayout.Button("Update all Vertex Colors!")){
            
            foreach(var go in gameObjects){
                if(go != null){
                    foreach(var f in go.GetComponentsInChildren<MeshFilter>()){
                        var mesh = f.sharedMesh;
                        var top = mesh.bounds.size.y;
                        var col = mesh.colors;
                        for(int i = 0; i<col.Length;i++){
                            Color random = color * Random.Range((1-RandomRange), 1);
                            col[i] = Color.Lerp(Color.clear, new Color(random.r,random.g,random.b,1), mesh.vertices[i].y / top);
                            
                        }
                        mesh.colors = col;
                        f.sharedMesh = mesh;
                    }
                }
            }
        }
        if(!overrideShaders)
            EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndVertical();
        
        
    }
    void OverrideShaders(){
        
        EditorGUI.BeginChangeCheck();
        select = GUILayout.Toolbar(select,new string[]{"Leaves Shader", "Vertex Color Preview"});

        if(EditorGUI.EndChangeCheck()){
            ApplyShaders();
        }
    }
    void ApplyShaders(){

        switch(select){
            case 0:
                foreach(var go in gameObjects){
                    if(go != null){
                        foreach(var r in go.GetComponentsInChildren<MeshRenderer>()){
                            foreach(var m in r.sharedMaterials){
                                m.shader = Utils.GetLeafShader();
                                m.SetFloat("_WindMode", 3);
                            }
                        }
                    }
                }
            break;
            case 1:
                foreach(var go in gameObjects){
                    if(go != null){
                        foreach(var r in go.GetComponentsInChildren<MeshRenderer>()){
                            foreach(var m in r.sharedMaterials){
                                m.shader = Utils.GetVertexColorShader();
                                
                            }
                        }
                    }
                }
            break;
        }
    }
    static Texture2D BackgroundTexture(Color col){
        var tex = new Texture2D(1,1);
        tex.SetPixel(0,0,col);
        tex.Apply();
        return tex;
    }
    void OnDestroy(){
        if(overrideShaders){
            select = 0;
            ApplyShaders();
        }
    }
}