using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mtree
{

    public class Node
    {
        public List<Node> children;
        public Vector3 position;
        public Vector3 direction;
        public float radius;
        public int creator;
        public float distanceFromOrigin; // total branch distance from the root to the Node
        public NodeType type; // Used to displace the tree after Growth
        public GrowthInformation growth;
        public float positionInBranch; // relative position (0 to 1) of node in branch.
        public float branchVariation;

        public Node(Vector3 pos, float rad, Vector3 dir, int crea, NodeType type = NodeType.Branch, float distancToOrigin=0, float positionInBranch=0)
        {
            position = pos;
            radius = rad;
            direction = dir;
            children = new List<Node>();
            creator = crea;
            growth = new GrowthInformation(0, 0, 1);
            this.type = type;
            this.distanceFromOrigin = distancToOrigin;
            this.positionInBranch = positionInBranch;
        }

        public Queue<Node> Grow(float length, float splitProba, int maxNumber, float rad, float secondaryBranchRadius, float angle
                                , int creator, float randomness, float upAttraction, Transform treeTransform, float gravityStrength, float flatten, bool obstacleAvoidance = false, Queue<System.Func<Vector3, Node, Vector3>> modifiers = null)
        {
            int number = (int) (splitProba / Random.value) + 1; // The number of branches that will be generated
            if (number > maxNumber)
                number = maxNumber;
            if (!growth.canBeSplit)
                number = 1;
            
            Queue<Node> extremities = new Queue<Node>();
            bool outOfLength = growth.remainingGrowth < length; // Is the growth ending
            if (outOfLength)
                length = growth.remainingGrowth;

            Vector3 mainBranchTangent = Vector3.zero; // when there are only two splits, the two splits repulse each other

            for (int i=0; i<number; i++)
            {
                Vector3 randomVect = Random.onUnitSphere;
                if (flatten >= 0 && number > 1)
                    randomVect = Vector3.Lerp(randomVect, Vector3.up * (Random.Range(-1, 1)), flatten);
                Vector3 tangent = Vector3.Cross(randomVect, direction);
                if (number == 2)
                {
                    if (i == 0)
                        mainBranchTangent = tangent;
                    if (i == 1)
                        tangent = -mainBranchTangent;

                    randomness = secondaryBranchRadius * angle;
                }

                if (tangent.y < 0)
                {
                    tangent.y -= tangent.y * upAttraction;
                }
                Vector3 gravityForce = Vector3.down * gravityStrength * .1f * length;
                if (i == 0)
                {
                    Vector3 dir = Vector3.Lerp(direction, tangent, randomness).normalized;
                    if (obstacleAvoidance && ObstacleAvoidance(ref dir, position, treeTransform, 10))
                        continue;
                    dir += gravityForce;

                    if (modifiers != null)
                    {
                        foreach (var modifier in modifiers)
                        {
                            dir = modifier(dir, this);
                        }
                    }

                    if (gravityStrength != 0 || modifiers != null)
                        dir.Normalize();
                    Vector3 pos = position + dir * length;
                    Node child = new Node(pos, rad, dir, creator, distancToOrigin: distanceFromOrigin+(length/(.1f + radius)));
                    child.growth = new GrowthInformation(growth.growthGoal, growth.remainingGrowth - length, growth.initialRadius);
                    if(!outOfLength)
                        extremities.Enqueue(child);
                    children.Add(child);
                }
                else
                {
                    Vector3 dir = (direction * (1 - angle) + tangent * (angle)).normalized;
                    if (ObstacleAvoidance(ref dir, position, treeTransform, 10))
                        continue;
                    dir += gravityForce;
                    if (modifiers != null)
                    {
                        foreach (var modifier in modifiers)
                        {
                            dir = modifier(dir, this);
                        }
                    }
                    if (gravityStrength != 0 || modifiers!=null)
                        dir.Normalize();
                    Vector3 pos = position + dir * length;
                    Node child0 = new Node(position, rad*.9f, dir, creator, distancToOrigin: distanceFromOrigin);

                    child0.growth.canBeSplit = false; // Make sure The start of the new branch can't be split.
                    Node child1 = new Node(pos, rad*secondaryBranchRadius, dir, creator, distancToOrigin: distanceFromOrigin + length);
                    child0.children.Add(child1);
                    children.Add(child0);
                    child1.growth = new GrowthInformation(growth.growthGoal, growth.remainingGrowth - length, growth.initialRadius * secondaryBranchRadius);
                    if (!outOfLength)
                        extremities.Enqueue(child1);
                }

            }
            return extremities;
        }


        public Stack<Queue<TreePoint>> ToSplines()
        {
            Stack<Queue<TreePoint>> points = new Stack<Queue<TreePoint>>();
            points.Push(new Queue<TreePoint>());
            this.ToSplinesRec(points, Vector3.up);
            return points;
        }

        private void ToSplinesRec(Stack<Queue<TreePoint>> points, Vector3 parentDirection, float parentRadius=0)
        {
            int n = children.Count;
            float rad = radius;
            if (type == NodeType.Trunk && n > 0)
            {
                rad = children[0].radius;
                direction = (children[0].position - position).normalized;
            }
            points.Peek().Enqueue(new TreePoint(position, direction, radius, type, parentDirection, distanceFromOrigin, parentRadius));
            if (n > 0)
                children[0].ToSplinesRec(points, direction);
            for (int i=1; i<n; i++)
            {
                points.Push(new Queue<TreePoint>());
                children[i].ToSplinesRec(points, direction, rad);
            }
        }

        public void GetSelection(Queue<Node> selected, int selection, bool extremitiesOnly, ref float maxHeight)
        {
            if (selection == creator && !(extremitiesOnly && children.Count > 0))
            {
                selected.Enqueue(this);
                if (position.y > maxHeight)
                    maxHeight = position.y;
            }
            foreach (Node child in children)
            {
                child.GetSelection(selected, selection, extremitiesOnly, ref maxHeight);
            }
        }
        
        public void GetSplitCandidates(Queue<Node> selected, int selection, float start, float remainingLength)
        {
            if (start <= positionInBranch && selection == creator && children.Count == 1)
            {
                positionInBranch = (positionInBranch - start) * (1 - start);
                selected.Enqueue(this);
            }
            
            for(int i=0; i<children.Count; i++)
            {
                if (i == 0)
                {
                    float dist = (children[i].position - position).magnitude;
                    children[i].GetSplitCandidates(selected, selection, start, remainingLength - dist);
                }
                else
                {
                    children[i].GetSplitCandidates(selected, selection, start, start);
                }
            }
        }

        public void DebugPosRec(List<Vector3> positions)
        {
            positions.Add(position);
            foreach(Node child in children)
            {
                child.DebugPosRec(positions);
            }
        }

        public void AddGrowth(float growthLength)
        {
            growth.remainingGrowth = growthLength;
            growth.growthGoal = growthLength;
            growth.initialRadius = radius;
        }
        
        private bool ObstacleAvoidance(ref Vector3 dir, Vector3 position, Transform transform, float distance)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.TransformPoint(position), transform.TransformDirection(dir), out hit, distance))
            {
                Vector3 disp = transform.InverseTransformDirection(hit.normal) * Mathf.Exp(-hit.distance * .3f) * 1;
                dir += disp;
                dir.Normalize();
                return disp.magnitude > .5f;
            }
            return false;
        }

        public void GetLeafCandidates(Queue<LeafCandidate> candidates, float maxBranchRadius, ref float totalLength, int selection)
        {
            if (radius < maxBranchRadius && creator == selection)
            {
                float branchLength = 0f;
                if (children.Count > 0)
                {
                    branchLength = (children[0].position - position).magnitude;
                }
                totalLength += branchLength;
                candidates.Enqueue(new LeafCandidate(position, direction, Mathf.Pow(radius, .2f), branchLength, distanceFromOrigin, children.Count > 0));
            }
            foreach (Node child in children)
            {
                child.GetLeafCandidates(candidates, maxBranchRadius, ref totalLength, selection);
            }
        }

        public void Simplify(Node parent, float angleThreshold, float radiusThreshold)
        {
            if (radius < radiusThreshold)
            {
                children = new List<Node>();
                return;
            }

            Node nextParent = this;
            int n;
            if (children.Count > 0 && parent != null && parent.radius / children[0].radius < 2)
            {
                Vector3 v1 = position - parent.position;
                Vector3 v2 = children[0].position - position;
                if ((Vector3.Angle(v1, v2) < angleThreshold) && type != NodeType.Flare) // if true current Node must be removed
                {
                    List<Node> parentChildren = new List<Node>() {children[0]}; // new childern for parent, with first child being self first child
                    n = parent.children.Count;
                    for (int i = 1; i < n; i++) // adding  original parent children
                    {
                        parentChildren.Add(parent.children[i]);
                    }
                    n = children.Count;
                    for(int i=1; i<n; i++)
                    {
                        parentChildren.Add(children[i]); // adding self children except firt one whih is already in list
                    }                    
                    parent.children = parentChildren;
                    nextParent = parent;
                }
            }
            n = 0;
            foreach (Node child in children)
            {
                if (n == 0)
                {
                    child.Simplify(nextParent, angleThreshold, radiusThreshold);
                }
                else
                {
                    child.Simplify(null, angleThreshold, radiusThreshold);
                }
                n++;
            }
        }

        public float UpdatePositionInBranch(float distanceFromBranchOrigin=0, Node parent = null)
        {
            float dist = parent == null ? 0 : Vector3.Distance(parent.position, position);
            dist += distanceFromBranchOrigin;
            float totalDistance;
            if (children.Count == 0)
            {
                positionInBranch = 1f;
                totalDistance = dist;
            }
            else
            {
                totalDistance = children[0].UpdatePositionInBranch(dist, this);
                positionInBranch = dist / totalDistance;
                for (int i=1; i<children.Count; i++)
                {
                    children[i].UpdatePositionInBranch(0);
                }
            }
            return totalDistance;
        }
    }


    public enum NodeType {Flare, Trunk, Branch, FromTrunk}

    public struct GrowthInformation
    {
        public float remainingGrowth;
        public float growthGoal;
        public float initialRadius;
        public bool canBeSplit;

        public GrowthInformation(float goal = 0f, float remaining = 0f, float radius = 1f)
        {
            remainingGrowth = remaining;
            growthGoal = goal;
            initialRadius = radius;
            canBeSplit = true;
        }

        public float GrowthPercentage(float additionalLength = 0f)
        {
            if (growthGoal == 0)
                return 0;
            return 1 - (remainingGrowth - additionalLength) / growthGoal;
        }
    }
}
