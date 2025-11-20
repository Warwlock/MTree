using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mtree
{
    public struct TreePoint
    {
        public Vector3 position, direction;
        public float radius;
        public NodeType type;
        public Vector3 parentDirection;
        public float parentRadius;
        public float distanceFromOrigin;

        public TreePoint(Vector3 pos, Vector3 dir, float rad, NodeType type, Vector3 parentDirection, float distanceFromOrigin, float parentRadius)
        {
            position = pos;
            direction = dir;
            radius = rad;
            this.type = type;
            this.parentDirection = parentDirection;
            this.distanceFromOrigin = distanceFromOrigin;
            this.parentRadius = parentRadius;
        }
    }
}
