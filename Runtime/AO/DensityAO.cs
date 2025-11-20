using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace Mtree
{
    public static class DensityAO
    {
        private static void BakeAo(Color[] colors, Vector3[] verts, Vector3[] normals, int[] triangles, Matrix4x4 transform, float resolution, float distance,string TreeName)
        {
            float[] weights = new float[triangles.Length / 3];
            float totalArea = 0f;
            for (int i = 0; i < triangles.Length; i+=3)
            {
                weights[i/3] = GetTriangleArea(verts[triangles[i]], verts[triangles[i + 1]], verts[triangles[i + 2]]);
                totalArea += weights[i / 3];
            }
            for (int i = 0; i < weights.Length; i++)
                weights[i] /= totalArea;

            List<Vector3> points = new List<Vector3>();
            WeightedRandom randomGenerator = new WeightedRandom(weights);
            int sampleCount = (int)(totalArea / (resolution * resolution));
            System.Random random = new System.Random();
            for (int i = 0; i < sampleCount; i++)
            {
                int triangleIndex = randomGenerator.GetRandomIndex(random);
                Vector3 point = SamplePointInTriangle(verts[triangles[triangleIndex * 3]], verts[triangles[triangleIndex * 3 + 1]], verts[triangles[triangleIndex * 3 + 2]], random);
                point = transform.MultiplyPoint3x4(point);
                points.Add(point);
            }
            KDTree tree = new KDTree(points.ToArray(),transform.MultiplyPoint3x4(Vector3.zero),TreeName);

            float[] AoValues = new float[verts.Length];

            float max = 0;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 origin = transform.MultiplyPoint3x4(verts[i]);
                List <Vector3> neighbours = tree.RadiusSearch(origin, distance);
                float cost = 0f;
                Vector3 normal = transform.MultiplyVector(normals[i]);
                foreach (Vector3 point in neighbours)
                {
                    float dist = Vector3.Distance(point, origin);
                    cost += Mathf.Clamp01(Vector3.Dot(normal, point - origin) / dist + .5f) * Mathf.Exp(-dist);
                }
                max = Mathf.Max(cost, max);
                AoValues[i] = cost;
            }

            for (int i = 0; i < AoValues.Length; i++)
                AoValues[i] /= max;

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].a = Mathf.Pow(1 - AoValues[i], 5f);
            }

        }

        public static float GetTriangleArea(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 AB = v2 - v1;
            Vector3 AC = v3 - v1;
            return Vector3.Cross(AB, AC).magnitude / 2f;
        }

        private static Vector3 SamplePointInTriangle(Vector3 v1, Vector3 v2, Vector3 v3, System.Random random)
        {
            float w1 = (float)random.NextDouble();
            float w2 = (float)random.NextDouble();
            return (1 - Mathf.Sqrt(w1)) * v1 + (Mathf.Sqrt(w1) * (1 - w2)) * v2 + (w2 * Mathf.Sqrt(w1)) * v3;
        }

        public static ThreadHandleEditor BakeAoAsync(MeshFilter meshFilter, GameObject ob, float resolution, float distance)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Color[] colors = mesh.colors;
            Vector3[] verts = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            string name = ob.name;

            if (meshFilter == null || ob == null)
                return null;

            Matrix4x4 transformMatrix = ob.transform.localToWorldMatrix;
            ThreadHandleEditor handle = new ThreadHandleEditor(
                null,
                () => BakeAo(colors, verts, normals, triangles, transformMatrix, resolution, distance, name),
                () => { mesh.colors = colors; if (meshFilter != null) meshFilter.sharedMesh = mesh; });
            handle.Start();

            return handle;
        }

        public static void BakeAo(MeshFilter meshFilter, GameObject ob, float resolution, float distance)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Color[] colors = mesh.colors;
            Vector3[] verts = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            Matrix4x4 transformMatrix = ob.transform.localToWorldMatrix;
            BakeAo(colors, verts, normals, triangles, transformMatrix, resolution, distance,ob.name);
            mesh.colors = colors;
            meshFilter.sharedMesh = mesh;
        }
    }

    public class ThreadHandleEditor
    {
        private Thread thread;
        private readonly System.Action OnStartCallback;
        private readonly System.Action ThreadedFunction;
        private readonly System.Action OnEndCallback;
        private Coroutine coroutine;
        private bool begun;
        private bool waited;
        bool ended;
        private float time;
        

        public ThreadHandleEditor(System.Action onStartCallback, System.Action threadedFunction, System.Action onEndCallback)
        {
            OnStartCallback = onStartCallback;
            ThreadedFunction = threadedFunction;
            OnEndCallback = onEndCallback;
        }

        public void Start()
        {
        #if UNITY_EDITOR
            waited = false;
            begun = false;
            ended = false;
            time = Time.realtimeSinceStartup;
            UnityEditor.EditorApplication.update += Run;
        #endif
        }

        private void Run()
        {
        #if UNITY_EDITOR

            if (Time.realtimeSinceStartup - time > 1f)
            {
                waited = true;
            }

            if (waited && !begun)
            {
                begun = true;
                if (OnStartCallback != null)
                    OnStartCallback.Invoke();

                if (ThreadedFunction != null)
                {
                    thread = new Thread(new ThreadStart(ThreadedFunction));
                    thread.Start();
                }
                time = Time.realtimeSinceStartup;
            }

            if (!ended && waited && begun && (thread == null || !thread.IsAlive))
            {
                OnEndCallback.Invoke();
                ended = true;
                UnityEditor.EditorApplication.update -= Run;
            }
        #endif
        }

        public void Abort()
        {
            if(thread != null)
                thread.Abort();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= Run;
            #endif
        }

        private IEnumerator RunThread()
        {
            if (OnStartCallback != null)
                OnStartCallback.Invoke();

            if (ThreadedFunction != null)
            {
                thread = new Thread(new ThreadStart(ThreadedFunction));
                thread.Start();
            }

            while (thread != null && thread.IsAlive)
                yield return null;

            if(OnEndCallback != null)
                OnEndCallback.Invoke();
        }
    }
}
