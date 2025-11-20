using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mtree;

[RequireComponent(typeof(MtreeComponent))]
public class MtreeBezier : MonoBehaviour {

	[SerializeField]
	public Vector3[] points;
	
	[SerializeField] 
	public Vector3[] s_positions = new Vector3[0],s_directions = new Vector3[0];
	
	[SerializeField]
	public bool MTreeDoBezier,MTreeLeafDirection;

	[SerializeField]
	private BezierControlPointMode[] modes;
	
	[SerializeField]
	public Vector3 s_branchdirection;
	public int ControlPointCount {
		get {
			return points.Length;
		}
		
	}

	public Vector3 GetControlPoint (int index) {
		return points[index];
	}

	public void SetControlPoint (int index, Vector3 point) {
		if (index % 3 == 0) {
			Vector3 delta = point - points[index];
				if (index > 0) {
					points[index - 1] += delta;
				}
				if (index + 1 < points.Length) {
					points[index + 1] += delta;
				}
			
		}
		points[index] = point;
		EnforceMode(index);

		if(points.Length > 3 && MTreeDoBezier){
			s_positions = new Vector3[0];
			s_directions = new Vector3[0];
            //SetMtreeCalculations();
#if UNITY_EDITOR

            GetComponent<MtreeComponent>().GenerateTree();
#endif
        }

	}

	public BezierControlPointMode GetControlPointMode (int index) {
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (modeIndex == modes.Length - 1) {
				modes[0] = mode;
			
		}
		EnforceMode(index);

	}

	private void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || (modeIndex == 0 || modeIndex == modes.Length - 1)) {
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) {
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0) {
				enforcedIndex = points.Length - 2;
			}
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
	}

	public int CurveCount {
		get {
			return (points.Length - 1) / 3;
		}
	}

	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
	}
	
	public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
	}
	
	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

	public void AddCurve () {
		Vector3 point = points[points.Length - 1];
		System.Array.Resize(ref points, points.Length + 3);
		point.y += 1f;
		points[points.Length - 3] = point;
		point.y += 1f;
		points[points.Length - 2] = point;
		point.y += 1f;
		points[points.Length - 1] = point;

		System.Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);
	}
	
	public float GetLength(){
		return Bezier.GetTotalLenght(points);
	}
	public void Reset () {
		points = new Vector3[] {
			new Vector3(0f, 0f, 0f),new Vector3(0f, 2f, 0f),new Vector3(0f, 6f, 0f),new Vector3(0f, 8f, 0f)
			
		};
		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		};
		s_positions = new Vector3[0];
		s_directions = new Vector3[0];
	}
}


