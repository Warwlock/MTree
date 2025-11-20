#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace Mtree
{
    class Billboard
    {
        private Camera cam;
        private GameObject target;
        private int width = 512;
        private int height = 512;
        private Texture2D[] sides;
        private Rect[] rects;
        private float ZBuffer = 0.03f;
        private float TopCardPosition = 0.5f;
        private bool hasTopCard,isSingleSided;
        int cards = 2;
        public Billboard(Camera cam, GameObject target, int width, int height, float TopCardPosition, bool hasTopCard, bool isSingleSided)
        {
            this.cam = cam;
            this.target = target;
            this.width = width;
            this.height = height;
            this.TopCardPosition = TopCardPosition;
            this.isSingleSided = isSingleSided;

            if (isSingleSided)
                cards += 2;
            if (hasTopCard && !isSingleSided)
                cards += 1;
            if (hasTopCard && isSingleSided)
                cards += 2;

            sides = new Texture2D[cards];
        }



        public void SetupCamera()
        {
            float rw = width;
            rw /= Screen.width;
            float rh = height;
            rh /= Screen.height;

            Bounds bb = target.GetComponent<Renderer>().bounds;

            cam.transform.position = bb.center;
            cam.nearClipPlane = -bb.extents.x;
            cam.farClipPlane = bb.extents.x;
            cam.orthographicSize = bb.extents.y;
            cam.aspect = bb.extents.x / bb.extents.y * 2;

            if (cam.aspect <= 1)
                width = (int)(height * cam.aspect);
            else
                height = (int)(width / cam.aspect);

        }

        public void Render(string path)
        {
            Bounds bb = target.GetComponent<Renderer>().bounds;
            int layer = target.layer;
            target.layer = 31;
            cam.cullingMask = 1 << 31;
            var originalPos = cam.transform.position;
            cam.transform.position = new Vector3(target.transform.position.x, bb.center.y, target.transform.position.z);
            for (int i = 0; i < cards; i++)
            {
                RenderTexture currentRT = RenderTexture.active;
                RenderTexture camText = new RenderTexture(width, height, 16);

                cam.targetTexture = camText;
                RenderTexture.active = cam.targetTexture;
                cam.Render();
                Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
                image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);

                if (i == 5)
                {
                    Color32[] originalPixels = image.GetPixels32().Reverse().ToArray();
                    var flippedPixels =
                        Enumerable.Range(0, image.width * image.height)
                        .Select(index =>
                        {
                            int x = index % image.width;
                            int y = index / image.width;
                            x = image.width - 1 - x;
                            return originalPixels[y * image.width + x];
                        }
                        );
                    image.SetPixels32(flippedPixels.ToArray());
                }
                image.Apply();
                RenderTexture.active = currentRT;
                sides[i] = image;

                if (isSingleSided)
                {
                    if (i < 3)
                        cam.transform.Rotate(Vector3.up * 90);
                    if (i == 3)
                    {
                        cam.ResetAspect();
                        cam.aspect = 1;
                        cam.orthographicSize = Mathf.Max(bb.extents.x, bb.extents.z) * 2;

                        cam.transform.position = originalPos;
                        var t = cam.transform.position;
                        cam.transform.position = new Vector3(t.x, bb.extents.y * 2, t.z);
                        cam.transform.Rotate(Vector3.right * 90);
                        cam.farClipPlane = bb.extents.y * 3;
                        cam.nearClipPlane = -bb.extents.y * 3;
                    }
                   
                }
                if (!isSingleSided)
                {
                    if (i == 0)
                        cam.transform.Rotate(Vector3.up * 90);
                    if (i == 1)
                    {
                        cam.ResetAspect();
                        cam.aspect = 1;
                        cam.orthographicSize = Mathf.Max(bb.extents.x, bb.extents.z) * 2;

                        cam.transform.position = originalPos;
                        cam.transform.Rotate(Vector3.up * 180);
                        var t = cam.transform.position;
                        cam.transform.position = new Vector3(t.x, bb.extents.y * 2, t.z);
                        cam.transform.Rotate(Vector3.right * 90);
                        cam.farClipPlane = bb.extents.y * 3;
                        cam.nearClipPlane = -bb.extents.y * 3;

                    }
                }

            }

            Texture2D atlas = new Texture2D(width, height);
            rects = atlas.PackTextures(sides, 0, Mathf.Min(height, width) * cards);
            Utils.DilateTexture(atlas, 100);
            byte[] bytes = atlas.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            target.layer = layer;
        }

        public Mesh CreateMesh()
        {
            Vector3[] verts = new Vector3[cards * 4];
            Vector3[] normals = new Vector3[cards * 4];
            Vector2[] uvs = new Vector2[cards * 4];
            int[] triangles = new int[cards * 6];

            Bounds bb = target.GetComponent<MeshRenderer>().bounds;
            float maxTC = Mathf.Max(bb.extents.x, bb.extents.z);
            for (int i = 0; i < cards; i++)
            {
                var count = i;

                if (i == 4 || i == 2 && !isSingleSided)
                    count = 0;

                float angle = 90f * count + 180;
                Vector3 x = Quaternion.Euler(0f, angle, 0f) * Vector3.left * bb.extents.x * 2;
                Vector3 normal = Quaternion.Euler(0f, angle, 0f) * Vector3.back;
                Vector3 y = Vector3.up * bb.extents.y * 2;

                Vector3 v0 = x;
                Vector3 v1 = x + y;
                Vector3 v2 = y - x;
                Vector3 v3 = -x;


                if (i == 0 && isSingleSided)
                {

                    v0.z += ZBuffer;
                    v1.z += ZBuffer;
                    v2.z += ZBuffer;
                    v3.z += ZBuffer;
                }
                if (i == 1 && isSingleSided)
                {

                    v0.x += ZBuffer;
                    v1.x += ZBuffer;
                    v2.x += ZBuffer;
                    v3.x += ZBuffer;
                }
                
                if (i == 4 || !isSingleSided && i == 2)
                {
                    float height = Mathf.Lerp(0, bb.extents.y * 2, TopCardPosition);
                    float bbX = bb.extents.x * 2 - (rects[i].width / 2);
                    float bbZ = bb.extents.z * 2 - (rects[i].height / 2);
                    
                    v0 = new Vector3(-bbX, height, bbZ);
                    v1 = new Vector3(bbX, height, bbZ);
                    v2 = new Vector3(bbX, height, -bbZ);
                    v3 = new Vector3(-bbX, height, -bbZ);

                }
                if (i == 5)
                {
                    float height = Mathf.Lerp(0, bb.extents.y * 2, TopCardPosition) - ZBuffer;
                    float bbX = bb.extents.x * 2 - (rects[i-1].width / 2);
                    float bbZ = bb.extents.z * 2 - (rects[i-1].height / 2);

                    v0 = new Vector3(bbX, height, bbZ);
                    v1 = new Vector3(-bbX, height, bbZ);
                    v2 = new Vector3(-bbX, height, -bbZ);
                    v3 = new Vector3(bbX, height, -bbZ);
                }


                verts[4 * i + 0] = v0;
                verts[4 * i + 1] = v1;
                verts[4 * i + 2] = v2;
                verts[4 * i + 3] = v3;

                x.Normalize();
                float normalFacing = 50f;
                Vector3 n0 = (x + normalFacing * normal).normalized;
                Vector3 n1 = (x + normalFacing * normal).normalized;
                Vector3 n2 = (-x + normalFacing * normal).normalized;
                Vector3 n3 = (-x + normalFacing * normal).normalized;


                normals[4 * i] = n0;
                normals[4 * i + 1] = n1;
                normals[4 * i + 2] = n2;
                normals[4 * i + 3] = n3;

                triangles[6 * i] = 4 * i;
                triangles[6 * i + 1] = 4 * i + 1;
                triangles[6 * i + 2] = 4 * i + 2;
                triangles[6 * i + 3] = 4 * i + 2;
                triangles[6 * i + 4] = 4 * i + 3;
                triangles[6 * i + 5] = 4 * i;

                Vector2 c = rects[i].center;
                Vector2 u = new Vector2();
                Vector2 v = new Vector2();

                u = new Vector2(rects[i].max.x - c.x, 0f);
                v = new Vector2(0f, rects[i].max.y - c.y);


                uvs[4 * i] = c + u - v;
                uvs[4 * i + 1] = c + u + v;
                uvs[4 * i + 2] = c - u + v;
                uvs[4 * i + 3] = c - u - v;

            }
            Mesh mesh = new Mesh
            {
                vertices = verts,
                triangles = triangles,
                uv = uvs,
                normals = normals
            };
            return mesh;
        }

        public Material CreateMaterial(Texture tex, bool usingHDRP)
        {
            Bounds bb = target.GetComponent<MeshRenderer>().bounds;
            var pipeline = GraphicsSettings.defaultRenderPipeline;
            Shader shader = Utils.GetBillboardShader();
            if (usingHDRP)
                shader = Shader.Find("Mtree/SRP/Billboard HDRP");
            Material mat = new Material(shader);
            mat.mainTexture = tex;
            mat.SetFloat("_AlphaClip", 1);
            return mat;
        }

    }

}
#endif