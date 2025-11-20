using UnityEngine;
public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
	}

public static class Bezier {

	public static Vector3 GetPoint (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float OneMinusT = 1f - t;
		return
			OneMinusT * OneMinusT * OneMinusT * p0 +
			3f * OneMinusT * OneMinusT * t * p1 +
			3f * OneMinusT * t * t * p2 +
			t * t * t * p3;
	}

	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
			6f * oneMinusT * t * (p2 - p1) +
			3f * t * t * (p3 - p2);
	}
	public static float GetTotalLenght(Vector3[] points){
		float length = 0;
		for(int i = 0; i<points.Length-3;i += 3){
			if(points.Length>=3) length += BezierSingleLength(new Vector3[]{points[i],points[i+1],points[i+2],points[i+3]});
		}
		return length;
	}
	public static float BezierSingleLength(Vector3[] p){
		var p0 = p[0] - p[1];
		var p1 = p[2] - p[1];
		var p2 = new Vector3();
		var p3 = p[3]-p[2];

		var l0 = p0.magnitude;
		var l1 = p1.magnitude;
		var l3 = p3.magnitude;
		if(l0 > 0) p0 /= l0;
		if(l1 > 0) p1 /= l1;
		if(l3 > 0) p3 /= l3;

		p2 = -p1;
		var a = Mathf.Abs(Vector3.Dot(p0,p1)) + Mathf.Abs(Vector3.Dot(p2,p3));
		if(a > 1.98f || l0 + l1 + l3 < (4 - a)*8) return l0+l1+l3;

		var bl = new Vector3[4];
		var br = new Vector3[4];

		bl[0] = p[0];
		bl[1] = (p[0]+p[1]) 	* 0.5f;

		var mid = (p[1]+p[2]) 	* 0.5f;

		bl[2] = (bl[1]+mid) 	* 0.5f;
		br[3] = p[3];
		br[2] = (p[2]+p[3]) 	* 0.5f;
		br[1] = (br[2]+mid)		* 0.5f;
		br[0] = (br[1]+bl[2])	* 0.5f;
		bl[3] = br[0];
		
		return BezierSingleLength(bl) + BezierSingleLength(br);
	}
}