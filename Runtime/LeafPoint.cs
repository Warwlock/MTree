using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mtree
{
    public struct LeafPoint
    {
        public Vector3 position;
        public Vector3 direction;
        public float size;
        public Mesh[] meshes;
        public bool overrideNormals;
        public float distanceFromOrigin;
        private float length;
        private int uLoops;
        private float resolution;
        public bool procedural;
        private float gravityStrength;


        public LeafPoint(Vector3 pos, Vector3 dir, float size, Mesh[] m, bool overrideNorm, float distanceFromOrigin, float length=1, int uLoops=2, float resolution=1, bool procedural=false, float gravityStrength=1)
        {
            position = pos;
            direction = dir;
            this.size = size;
            meshes = m;
            overrideNormals = overrideNorm;
            this.distanceFromOrigin = distanceFromOrigin;
            this.length = length;
            this.uLoops = uLoops;
            this.resolution = resolution;
            this.procedural = procedural;
            this.gravityStrength = gravityStrength;
        }

        public Mesh GetMesh(int lodLevel)
        {
            if (!procedural)
            {
                if (meshes != null && meshes.Length > 0)
                    return meshes[Mathf.Min(lodLevel, meshes.Length-1)];
                else
                    return null;
            }
            else
                return GrowLeaf(); 
        }

        private Mesh GrowLeaf()
        {
            Vector3[] profile = new Vector3[uLoops];
            Vector2[] profileUvs = new Vector2[uLoops];
            for (int i=0; i<uLoops; i++)
            {
                float t = (float)i / (uLoops - 1);
                float x_pos = t - 0.5f;
                float y_pos = - Mathf.Pow(t-0.5f, 2);
                if (uLoops == 2)
                    y_pos = 0;
                profile[i] = new Vector3(x_pos*2, y_pos, 0);
                profileUvs[i] = new Vector2(t, 0);
            }

            Queue<Vector3> verts = new Queue<Vector3>();
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, direction);
            rot = Quaternion.LookRotation(direction);
            foreach (Vector3 v in profile)
                verts.Enqueue(rot * v*size + position);
            Queue<Vector2> uvs = new Queue<Vector2>(profileUvs);
            Queue<int> triangles = new Queue<int>();
            Vector3 lastPos = position;
            Vector3 lastDir = direction.normalized;
            int vCount = Mathf.Max(1, (int)(length * resolution));
            float stepLength = length / vCount;
            for (int i=0; i < vCount; i++)
            {
                Vector3 newDir = Vector3.Normalize(lastDir + Vector3.down * gravityStrength / resolution);
                Vector3 newPos = lastPos + lastDir * stepLength;
                if (Vector3.Angle(lastDir, newDir) < 10 && i != vCount - 1 || newPos.y < 0)
                {
                    lastPos = newPos;
                    lastDir = newDir;
                    continue;
                }
                //newDir = lastDir;
                //newPos = position;
                rot = Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(lastDir, newDir, .5f));
                rot = Quaternion.LookRotation(lastDir, Vector3.up);
                int n = verts.Count;
                for (int j=0; j<uLoops; j++)
                {
                    Vector3 v = (rot * profile[j]*size) + newPos;
                    verts.Enqueue(v);
                    uvs.Enqueue(new Vector2((float)j / (uLoops - 1), (float)(i+1)/vCount));
                    if (j < uLoops - 1)
                    {
                        Utils.AddTriangle(triangles, n-uLoops + j, n + j , n - uLoops + 1 + j);
                        Utils.AddTriangle(triangles, n + j , n + j + 1, n - uLoops + j + 1);
                    }
                }
                lastPos = newPos;
                lastDir = newDir;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            return mesh;
        }
    }

    public struct LeafCandidate
    {
        public Vector3 position;
        public Vector3 direction;
        public float size;
        public float parentBranchLength;
        public float distanceFromOrigin;
        public bool isEnd;


        public LeafCandidate(Vector3 pos, Vector3 dir, float size, float branchLength, float distanceFromOrigin, bool isEnd)
        {
            position = pos;
            direction = dir;
            this.size = size;
            parentBranchLength = branchLength;
            this.distanceFromOrigin = distanceFromOrigin; ;
            this.isEnd = isEnd;
        }
    }
}
