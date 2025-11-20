using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mtree
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    public class KDTreeNode
    {
        public Vector3 position;
        public KDTreeNode left;
        public KDTreeNode right;

        public KDTreeNode(Vector3 position)
        {
            this.position = position;
        }

        public void FindClosestRec(Vector3 point, ref float currBestDist, ref Vector3 currBest, int axis)
        {
            float dist = (point - position).sqrMagnitude;
            if (dist < currBestDist)
            {
                currBestDist = dist;
                currBest = position;
            }
            float diff = point[axis] - position[axis];
            bool leftFirst = diff <= 0;

            axis = (axis + 1) % 3;
            if (leftFirst)
            {
                if (left != null)
                    left.FindClosestRec(point, ref currBestDist, ref currBest, axis);
                if (diff * diff < currBestDist && right != null)
                    right.FindClosestRec(point, ref currBestDist, ref currBest, axis);
            }
            else
            {
                if (right != null)
                    right.FindClosestRec(point, ref currBestDist, ref currBest, axis);
                if (diff * diff < currBestDist && left != null)
                    left.FindClosestRec(point, ref currBestDist, ref currBest, axis);
            }
        }

        public void RadiusSearchRec(Vector3 center, float radiusSquared, ref List<Vector3> neighbours, int axis)
        {
            float dist = Vector3.SqrMagnitude(center - position);
            if (dist < radiusSquared)
                neighbours.Add(position);
            float diff = (position[axis] - center[axis]);
            axis = (axis + 1) % 3;
            bool searchLeft = diff * diff * Mathf.Sign(diff) > -radiusSquared / 4;
            bool searchRight = diff * diff * Mathf.Sign(diff) < radiusSquared / 4;

            if (searchLeft && left != null)
                left.RadiusSearchRec(center, radiusSquared, ref neighbours, axis);
            if (searchRight && right != null)
                right.RadiusSearchRec(center, radiusSquared, ref neighbours, axis);
        }


        public void AddPoint(Vector3 point, int axis)
        {
            bool isLeft = position[axis] > point[axis];
            KDTreeNode child = isLeft ? left : right;
            axis = (axis + 1) % 3;
            if (child == null)
            {
                child = new KDTreeNode(point);
                if (isLeft) left = child;
                else right = child;
            }
            else
                child.AddPoint(point, axis);

        }
    }

    public class KDTree
    {
        private KDTreeNode root;

        public KDTree(Vector3[] points,Vector3 t,string TreeName)
        {
            if (points.Length == 0)
            {
                points = new Vector3[]{t};
            }

            root = new KDTreeNode(points[0]);
            for (int i = 1; i < points.Length; i++)
            {
                AddPoint(points[i]);
            }

            //root = BuildFromPointsRec(points, 0, 0, points.Length);
        }

        public KDTree()
        {
            root = new KDTreeNode(Vector3.zero);
        }

        public void AddPoint(Vector3 point)
        {
            root.AddPoint(point, 0);
        }

        private KDTreeNode BuildFromPointsRec(Vector3[] points, int axis, int start, int end)
        {
            Debug.Log(start + " " + end);

            if (start == end - 1)
            {
                Debug.Log(start);
                return new KDTreeNode(points[start]);
            }

            //int medianIndex = SelectMedian(points, axis, start, end);
            int medianIndex = Random.Range(start, end - 1);

            KDTreeNode node = new KDTreeNode(points[medianIndex]);
            axis = (axis + 1) % 3;
            node.left = BuildFromPointsRec(points, axis, start, medianIndex);
            node.right = BuildFromPointsRec(points, axis, medianIndex, end);

            return node;
        }

        public Vector3 FindClosest(Vector3 point)
        {
            float currBestDist = Mathf.Infinity;
            Vector3 bestPoint = root.position;
            root.FindClosestRec(point, ref currBestDist, ref bestPoint, 0);
            return bestPoint;
        }

        public List<Vector3> RadiusSearch(Vector3 center, float radius)
        {
            List<Vector3> neighbours = new List<Vector3>();
            root.RadiusSearchRec(center, radius * radius, ref neighbours, 0);
            return neighbours;
        }

        private int SelectMedian(Vector3[] points, int axis, int start, int end)
        {
            float[] values = new float[end - start];
            for (int i = start; i < end; i++)
            {
                values[i - start] = points[i][axis];
            }
            int[] indices = new int[values.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            System.Array.Sort(values, indices);
            return indices[indices.Length / 2];
        }
    }

}