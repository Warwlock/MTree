using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MtreeBezier))]
public class MtreeBezierInspector : Editor {

	private const int stepsPerCurve = 10;
	private const float directionScale = 0.5f;
	private const float handleSize = 0.04f;
	private const float pickSize = 0.06f;
	private static Color[] modeColors = {
		Color.white,
		Color.blue,
		Color.cyan
	};

	private MtreeBezier spline;
	private Transform handleTransform;
	private Quaternion handleRotation;
	private int selectedIndex = -1;

	private string DoBezier,DoLeaf;
	public override void OnInspectorGUI () {

		var gs = new GUIStyle();
		gs.fontStyle = FontStyle.Bold;

		spline = target as MtreeBezier;
		GUILayout.Space(10);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.LabelField("MTree Addon",gs);

		if(spline.MTreeDoBezier){
			DoBezier = "Deactivate Bezier Mode";
		}else{
			DoBezier = "Activate Bezier Mode";
		}
		if(GUILayout.Button(DoBezier)){
			Undo.RecordObject(spline,DoBezier);
			spline.MTreeDoBezier = !spline.MTreeDoBezier;
			EditorUtility.SetDirty(spline);
		}
		if (GUILayout.Button("Add Curve")) {
			Undo.RecordObject(spline, "Add Curve");
			spline.AddCurve();
			EditorUtility.SetDirty(spline);
		}
		if(EditorGUI.EndChangeCheck()){
			spline.GetComponent<MtreeComponent>().GenerateTree();
		}
		GUILayout.Space(10);
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.LabelField("Branch / Leaf Directions",gs);
		if(spline.MTreeLeafDirection){
			DoLeaf = "Deactivate Leaf Direction Mode";
		}else{
			DoLeaf = "Activate Leaf Direction Mode";
		}
		if(GUILayout.Button(DoLeaf)){
			Undo.RecordObject(spline,DoLeaf);
			spline.MTreeLeafDirection = !spline.MTreeLeafDirection;
			EditorUtility.SetDirty(spline);
		}
		spline.s_branchdirection = EditorGUILayout.Vector3Field("Branch Growth Direction",spline.s_branchdirection);
		
		if(EditorGUI.EndChangeCheck()){
			
			spline.s_branchdirection.x = Mathf.Clamp(spline.s_branchdirection.x,-1,1);
			spline.s_branchdirection.y = Mathf.Clamp(spline.s_branchdirection.y,-1,1);
			spline.s_branchdirection.z = Mathf.Clamp(spline.s_branchdirection.z,-1,1);
			Undo.RecordObject(spline,"Branch Growth Direction");
			spline.GetComponent<MtreeComponent>().GenerateTree();
			EditorUtility.SetDirty(spline);
		}
		GUILayout.Space(10);
		
		if (selectedIndex >= 0 && selectedIndex < spline.ControlPointCount) {
			EditorGUILayout.LabelField("Bezier Position / Mode",gs);
			DrawSelectedPointInspector();
		}

		

		
	}

	private void DrawSelectedPointInspector() {
		GUILayout.Label("Selected Point");
		EditorGUI.BeginChangeCheck();
		Vector3 point = EditorGUILayout.Vector3Field("Position", spline.GetControlPoint(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Move Point");
			EditorUtility.SetDirty(spline);
			spline.SetControlPoint(selectedIndex, point);
		}
		EditorGUI.BeginChangeCheck();
		BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", spline.GetControlPointMode(selectedIndex));
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(spline, "Change Point Mode");
			spline.SetControlPointMode(selectedIndex, mode);
			EditorUtility.SetDirty(spline);
		}
		
	}

	private void OnSceneGUI () {
		spline = target as MtreeBezier;
		handleTransform = spline.transform;
		handleRotation = Tools.pivotRotation == PivotRotation.Local ?
		handleTransform.rotation : Quaternion.identity;
		
		Vector3 p0 = ShowPoint(0);
		if(spline.points.Length>3){
			for (int i = 1; i < spline.ControlPointCount; i += 3) {
				Vector3 p1 = ShowPoint(i);
				Vector3 p2 = ShowPoint(i + 1);
				Vector3 p3 = ShowPoint(i + 2);
				
				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);
				
				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
			}
		}
	}


	private Vector3 ShowPoint (int index) {
		Vector3 point = handleTransform.TransformPoint(spline.GetControlPoint(index));
		float size = HandleUtility.GetHandleSize(point);
		if (index == 0) {
			size *= 5;
		}
		if(index > 0 ){
			size *= 3;
		}
		if(index % 3 == 1 || index % 3 == 2){
			Handles.color = modeColors[(int)spline.GetControlPointMode(index)] * Color.blue;
			}
		if(index % 3 == 0){
			Handles.color = modeColors[(int)spline.GetControlPointMode(index)] * Color.green;
			}
		if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.SphereHandleCap)) {
			selectedIndex = index;
			Repaint();
		}
		if (selectedIndex == index) {
			EditorGUI.BeginChangeCheck();

			point = Handles.DoPositionHandle(point, handleRotation);
			if (EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(spline, "Move Point");
				EditorUtility.SetDirty(spline);
				spline.SetControlPoint(index, handleTransform.InverseTransformPoint(point));
			}
		}
		return point;
	}
}