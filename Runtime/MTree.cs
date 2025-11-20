using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mtree
{
    public class MTree
    {
        public List<Node> stems;
        public Vector3[] verts;
        public Vector3[] normals;
        public Vector2[] uvs;
        public int[] triangles;
        public int[] leafTriangles;
        public Color[] colors;
        public Transform treeTransform;
        public float colorMultiplier;
        private Queue<LeafPoint> leafPoints;


        public MTree(Transform transform)
        {
            stems = new List<Node>();
            leafPoints = new Queue<LeafPoint>();
            treeTransform = transform;
        }


        public void AddTrunk(Vector3 position, Vector3 direction, float length, AnimationCurve radius, float radiusMultiplier, float resolution
                           , float randomness, int creator, AnimationCurve rootShape, float rootRadius, float rootHeight, float RootResolutionMultiplier
                           , float originAttraction)
        {
			MtreeBezier bezier = treeTransform.GetComponent<MtreeBezier>();
            float remainingLength = length;
            Node extremity = new Node(position, radius.Evaluate(0) * radiusMultiplier, direction, creator, NodeType.Flare);
            stems.Add(extremity);
            
			if (!bezier || bezier && !bezier.MTreeDoBezier) {
				while (remainingLength > 0)
				{
					float res = resolution;
					NodeType type = NodeType.Trunk;
					Vector3 tangent = Vector3.Cross(direction, Random.onUnitSphere);
					if (length - remainingLength < rootHeight)
					{
						tangent /= RootResolutionMultiplier * 2;
						res *= RootResolutionMultiplier;
						type = NodeType.Flare;
					}

					float branchLength = 1f / res;
					Vector3 dir = randomness * tangent + extremity.direction * (1 - randomness);
					Vector3 originAttractionVector = (position - extremity.position) * originAttraction;
					originAttractionVector.y = 0;
					dir += originAttractionVector;
					dir.Normalize();
					if (remainingLength <= branchLength)
						branchLength = remainingLength;
					remainingLength -= branchLength;

					Vector3 pos = extremity.position + dir * branchLength;
					float rad = radius.Evaluate(1 - remainingLength / length) * radiusMultiplier;
					//if (length - remainingLength < rootHeight)
					//    rad += radiusMultiplier * rootRadius * rootShape.Evaluate((length - remainingLength) / rootHeight);
					Node child = new Node(pos, rad, dir, creator, type, distancToOrigin: length-remainingLength);
					extremity.children.Add(child);
					extremity = child;
				}
			}

			if (bezier && bezier.MTreeDoBezier) {
				length = bezier.GetLength ();
				remainingLength = length;
				while (remainingLength > 1f / resolution)
				{
					float step = 1 - (remainingLength / length);
					Vector3 stepDir = bezier.GetDirection(step);
					Vector3 stepPos = bezier.GetPoint(step) - treeTransform.position;

					float res = resolution;
					NodeType type = NodeType.Trunk;
					Vector3 tangent = Vector3.Cross(direction, Random.onUnitSphere);
					if (length - remainingLength < rootHeight)
					{
						tangent /= RootResolutionMultiplier * 2;
						res *= RootResolutionMultiplier;
						type = NodeType.Flare;
					}

					float branchLength = 1f / res;
					Vector3 dir = randomness * tangent + stepDir * (1 - randomness);
					Vector3 originAttractionVector = (position - stepPos) * originAttraction;
					originAttractionVector.y = 0;
					dir += originAttractionVector;
					dir.Normalize();
					if (remainingLength <= branchLength)
						branchLength = remainingLength;
					remainingLength -= branchLength;

					Vector3 pos = stepPos + dir * branchLength;
					float rad = radius.Evaluate(1 - remainingLength / length) * radiusMultiplier;
					//if (length - remainingLength < rootHeight)
					//    rad += radiusMultiplier * rootRadius * rootShape.Evaluate((length - remainingLength) / rootHeight);
					Node child = new Node(pos, rad, dir, creator, type, distancToOrigin: length-remainingLength);
					extremity.children.Add(child);
					extremity = child;
				}
			}

            
        }

        public void AddRoots(int selection, RootsFunction rootsFunction)
        {
            Queue<Node> candidatesParents = SelectNodes(
                (Node node) =>
                {
                    if (node.creator != selection || node.children.Count == 0)
                        return false;
                    if (node.position.y > rootsFunction.end || node.children[0].position.y < rootsFunction.start)
                        return false;
                    return true;
                });


            Queue<Node> candidates = new Queue<Node>();

            float angleStep = 68.75f;
            Vector3 tangent = Random.onUnitSphere;
            float remainder = 0f;
            float rootsPerMeter = rootsFunction.number / (rootsFunction.end - rootsFunction.start + .001f);
            foreach (Node parent in candidatesParents)
            {
                float deltaHeight = Mathf.Abs(Mathf.Min(rootsFunction.end, parent.children[0].position.y) - Mathf.Max(rootsFunction.start, parent.position.y));
                float targetRootsCount = deltaHeight * rootsPerMeter + remainder;
                int rootsCount = (int)targetRootsCount;
                remainder = targetRootsCount - rootsCount;

                float startFactor = 0f;
                float endFactor = 1f;
                startFactor = Mathf.Clamp01((rootsFunction.start - parent.position.y) / deltaHeight);
                endFactor = Mathf.Clamp01((rootsFunction.end - parent.position.y) / deltaHeight);
               
                for (int i = 0; i < rootsCount; i++)
                {
                    float childPosFactor = Random.Range(startFactor, endFactor);
                    tangent = Vector3.ProjectOnPlane(tangent, parent.direction).normalized;
                    tangent = Quaternion.AngleAxis(angleStep, parent.direction) * tangent;
                    Vector3 childDir = Vector3.Slerp(-parent.direction, tangent, rootsFunction.angle);
                    float childRadius = parent.radius * rootsFunction.radius * Random.Range(.5f,1f);
                    Vector3 childOriginPos = Vector3.Lerp(parent.position, parent.children[0].position, childPosFactor) + tangent * (parent.radius - childRadius);
                    Vector3 childPos = childOriginPos + childDir * .1f;

                    Node childOrigin = new Node(childOriginPos, childRadius, childDir, rootsFunction.id, NodeType.Branch, positionInBranch: childPosFactor);
                    Node child = new Node(childPos, childRadius, childDir, rootsFunction.id, NodeType.Branch, positionInBranch: 1);

                    childOrigin.children.Add(child);
                    parent.children.Add(childOrigin);
                    candidates.Enqueue(child);

                    if (candidates.Count >= rootsFunction.number)
                        break;
                }
                if (candidates.Count >= rootsFunction.number)
                    break;
            }

            foreach (Node extremity in candidates)
            {
                extremity.AddGrowth(rootsFunction.length);
            }

            float branchGrowthStep = 1 / (rootsFunction.resolution + Mathf.Epsilon);
            while (candidates.Count > 0)
            {
                Node extremity = candidates.Dequeue();

                float rad = rootsFunction.shape.Evaluate(extremity.growth.GrowthPercentage(.1f)) * extremity.growth.initialRadius;
                float extremitySplitProba = rootsFunction.splitProbaCurve.Evaluate(extremity.growth.GrowthPercentage()) * rootsFunction.splitProba;
                var modifiers = new Queue<System.Func<Vector3, Node, Vector3>>();
                modifiers.Enqueue((dir, node) => MoveToGroundModifier(dir, node, rootsFunction.groundHeight, rootsFunction.attractionStrength));
                Queue<Node> children = extremity.Grow(branchGrowthStep, extremitySplitProba, 2, rad, rootsFunction.radius, rootsFunction.angle, rootsFunction.id, rootsFunction.randomness, 0, treeTransform, 0, .7f, obstacleAvoidance:false, modifiers:modifiers);
                foreach (Node child in children)
                {
                    candidates.Enqueue(child);
                }
            }
        }

        private Vector3 MoveToGroundModifier(Vector3 dir, Node node, float groundHeight, float attractionStrength)
        {
            float sqrDistFromStem = node.position.x * node.position.x + node.position.z * node.position.z;
            float planePosition = groundHeight - sqrDistFromStem*.2f;

            dir.y *= Mathf.Lerp(.5f, 1, Mathf.Abs(node.position.y - planePosition));
            dir.y += (planePosition - (node.position.y + dir.y*.3f) ) * .5f * attractionStrength;

            //dir.y *= .5f;
            return dir;
        }

        public void Grow(float length, AnimationCurve lengthCurve, float resolution, float splitProba, AnimationCurve probaCurve, float splitAngle
                        , int maxSplits, int selection, int creator, float randomness, AnimationCurve radius, float splitRadius, float upAttraction
                        , float gravityStrength, float flatten, float minRadius, bool obstacleAvoidance)
        {
            float maxHeight = 0;
            Queue<Node> extremities = GetSelection(selection, true, ref maxHeight);
            int n = extremities.Count;

            for(int i=0; i<n; i++) // Update Growth information for the extremities to grow
            {
                Node ext = extremities.Dequeue();

                float growthLength = length * lengthCurve.Evaluate(ext.position.y / maxHeight); // The length of this specific branch after growth
                ext.AddGrowth(growthLength);
                extremities.Enqueue(ext);
            }

            float branchLength = 1 / resolution;
            while (extremities.Count > 0)
            {
                Queue<Node> newExtremities = new Queue<Node>();
                while (extremities.Count > 0)
                {
                    Node extremity = extremities.Dequeue();

                    float rad = radius.Evaluate(extremity.growth.GrowthPercentage(branchLength)) * extremity.growth.initialRadius;
                    if (rad < minRadius)
                        rad = minRadius;
                    float extremitySplitProba = probaCurve.Evaluate(extremity.growth.GrowthPercentage()) * splitProba;
                    Queue<Node> children = extremity.Grow(branchLength, extremitySplitProba, maxSplits, rad, splitRadius, splitAngle, creator, randomness, upAttraction, treeTransform, gravityStrength, flatten, obstacleAvoidance);
                    if (rad == minRadius)
                        continue;
                    foreach (Node child in children)
                    {
                        newExtremities.Enqueue(child);
                    }
                    
                }
                extremities = newExtremities;
            }
        }




        public void Split(int selection, int expectedNumber, float splitAngle, int creator, float splitRadius, float startLength
                        , float spread, float flatten)
        {
 
            Queue<Node> candidates = GetSplitCandidates(selection, startLength);
			MtreeBezier bezier = treeTransform.GetComponent<MtreeBezier>(); Vector3 direction = Vector3.zero;
			if(bezier && bezier.MTreeLeafDirection)
				direction = bezier.s_branchdirection;
			
            float totalLength = 0;
            foreach (Node ext in candidates)
                totalLength += (ext.position - ext.children[0].position).magnitude;

            float splitPerMeter = expectedNumber / totalLength;

            float remainder = 0f;

            foreach (Node ext in candidates)
            { 
                float distToChild = (ext.position - ext.children[0].position).magnitude;

                float targetSplitNumber = distToChild * splitPerMeter + remainder;
                int number = (int)targetSplitNumber;
                remainder = targetSplitNumber - number;
                
                Vector3 randomVect = Random.onUnitSphere;
                if (flatten > 0)
                    randomVect = Vector3.Lerp(randomVect, Vector3.up, flatten).normalized * (Random.Range(0, 1) * 2 - 1);
                Vector3 tangent = Vector3.Cross(randomVect, ext.direction);
                Quaternion rot = Quaternion.AngleAxis(360f / number, ext.direction);
                Vector3 spreadDirection = ext.children[0].direction;
                for (int i = 0; i < number; i++)
                {
                    float blend = Random.value * spread;
                    float rad = Mathf.Lerp(ext.radius * splitRadius, ext.children[0].radius * splitRadius, blend);
                    Vector3 pos = ext.position + spreadDirection * blend * distToChild;
					Vector3 dir = Vector3.LerpUnclamped(ext.direction, tangent, splitAngle * (1-ext.positionInBranch*.7f)).normalized + direction;
                    float radiusFactor = Mathf.Clamp01(rad);
                    Node child0 = new Node(pos, ext.radius * radiusFactor + rad * (1-radiusFactor), dir, creator, distancToOrigin: ext.distanceFromOrigin + blend * distToChild);
                    child0.branchVariation = ext.branchVariation;
                    if (ext.type == NodeType.Trunk || ext.type == NodeType.Flare)
                        child0.type = NodeType.FromTrunk;
                    Node child1 = new Node(pos + dir * ext.radius / Mathf.Max(0.3f,Vector3.Dot(dir, tangent)) * 1f, rad, dir, creator, distancToOrigin: ext.distanceFromOrigin + blend * distToChild);
                    child1.branchVariation = ext.branchVariation;
                    child0.children.Add(child1);

                    child0.growth.canBeSplit = false;
                    ext.children.Add(child0);
                    tangent = rot * tangent;
                }
                                
            }

        }

        public void TwigSplit(int expectedNumber, float splitAngle, float splitRadius, float startLength)
        {
            Queue<Node> candidates = GetSplitCandidates(0, startLength);

            float totalLength = 0;
            foreach (Node ext in candidates)
                totalLength += (ext.position - ext.children[0].position).magnitude;

            float splitPerMeter = expectedNumber / totalLength;

            float remainder = 0f;
            int side = 1;
            foreach (Node ext in candidates)
            {
                float distToChild = (ext.position - ext.children[0].position).magnitude;
                float targetSplitNumber = distToChild * splitPerMeter + remainder;
                int number = (int)targetSplitNumber;
                remainder = targetSplitNumber - number;


                Vector3 spreadDirection = ext.children[0].direction;
                for (int i = 0; i < number; i++)
                {
                    float blend = i * 1f / number;
                    float rad = Mathf.Lerp(ext.radius * splitRadius, ext.children[0].radius * splitRadius, blend);
                    Vector3 pos = ext.position + spreadDirection * distToChild * blend;
                    Quaternion rot = Quaternion.AngleAxis(splitAngle * side, Vector3.up);
                    Vector3 dir = rot * ext.direction;
                    float radiusFactor = Mathf.Clamp01(rad);
                    Node child0 = new Node(pos, ext.radius * radiusFactor + rad * (1 - radiusFactor), dir, 1, distancToOrigin: ext.distanceFromOrigin + blend * distToChild);
                    child0.branchVariation = ext.branchVariation;
                    if (ext.type == NodeType.Trunk || ext.type == NodeType.Flare)
                        child0.type = NodeType.FromTrunk;
                    child0.growth.canBeSplit = true;
                    ext.children.Add(child0);
                    side *= -1;
                }
            }
        }


        public void AddBranches(int selection,  float length, AnimationCurve lengthCurve, float resolution, int number, float splitProba, AnimationCurve splitProbaCurve
                                , float angle, float randomness, AnimationCurve shape, float radius, float upAttraction, int creator
                                , float start, float gravityStrength, float flatten, float minRadius, bool obstacleAvoidance)
        {
            Split(selection, number, angle, creator, radius, start, 1, flatten);
            Grow(length, lengthCurve, resolution, splitProba, splitProbaCurve, .3f, 2, creator, creator, randomness, shape, .9f, upAttraction, gravityStrength, flatten, minRadius, obstacleAvoidance);
        }


        public void AddLeafs(float maxRadius, int number, Mesh[] mesh, float size, bool overrideNormals, float minWeight, float maxWeight, int selection,
                             float leafAngle, bool procedural, float length, float resolution, int uLoops, float gravityStrength)
        {
            Queue<LeafCandidate> candidates = new Queue<LeafCandidate>();
            float totalLength = 0f;
            foreach (Node stem in stems)
                stem.GetLeafCandidates(candidates, maxRadius * stem.radius, ref totalLength, selection);
            if (number < 1)
                number = 2;
            if (number > candidates.Count)
            {
                ExtendLeafCandidates(candidates, number);
            }

            LeafCandidate[] leafSpawners = Sample(candidates.ToArray(), number);
            foreach (LeafCandidate spawner in leafSpawners)
            {
                
                Vector3 direction = spawner.direction;
                direction.y /= 3;
                Vector3 dir = Vector3.LerpUnclamped(direction, Vector3.down, Random.Range(minWeight, maxWeight));
                Quaternion rot = Quaternion.Euler(0, Random.Range(-leafAngle, leafAngle), 0);
                dir = rot * dir;
                leafPoints.Enqueue(new LeafPoint(spawner.position, dir, spawner.size * size, mesh, overrideNormals, spawner.distanceFromOrigin, length, uLoops, resolution, procedural, gravityStrength));
            }
        }

        void ExtendLeafCandidates(Queue<LeafCandidate> candidates, int number)
        {
            LeafCandidate[] candidateArray = candidates.ToArray();
            int dupliNumber = 0;
            int candidatesNotAtEnd = 0;
            foreach (LeafCandidate l in candidateArray)
            {
                if (!l.isEnd)
                    candidatesNotAtEnd++;
            }
            dupliNumber = candidatesNotAtEnd == 0 ? 0 : number / candidatesNotAtEnd;

            foreach (LeafCandidate candidate in candidateArray)
            {
                //if (candidate.isEnd)
                //    continue;
                for (int i=0; i<dupliNumber; i++)
                {
                    Vector3 position = candidate.position + candidate.direction * candidate.parentBranchLength * (i + 1) / (dupliNumber + 1);
                    //position = Vector3.zero;
                    candidates.Enqueue(new LeafCandidate(position, candidate.direction, candidate.size, candidate.parentBranchLength, candidate.distanceFromOrigin, candidate.isEnd));
                }
            }
        }


        LeafCandidate[] Sample(LeafCandidate[] candidates, int number)
        {
            number = Mathf.Min(candidates.Length, number);
            LeafCandidate[] result = new LeafCandidate[number];
            for (int i=0; i<number; i++)
            {
                int index = Random.Range(i, candidates.Length - 1);
                result[i] = candidates[index];
                candidates[index] = candidates[i];
            }

            return result;
        }

        private Queue<Node> GetSelection(int selection, bool extremitiesOnly, ref float maxHeight)
        {
            Queue<Node> selected = new Queue<Node>();
            foreach (Node stem in stems)
            {
                stem.GetSelection(selected, selection, extremitiesOnly, ref maxHeight);
            }
            return selected;
        }

        
        private Queue<Node> GetSplitCandidates(int selection, float start)
        {
            Queue<Node> selected = new Queue<Node>();
            foreach (Node stem in stems)
            {
                stem.branchVariation = Random.Range(0, 1f);
                stem.UpdatePositionInBranch();
                stem.GetSplitCandidates(selected, selection, start, start);
            }
            return selected;
        }


        private Queue<Node> SelectNodes(System.Func<Node, bool> filter)
        {
            Queue<Node> selection = new Queue<Node>();
            foreach (Node stem in stems)
            {
                SelectNodesRec(stem, ref selection, filter);
            }
            return selection;
        }

        private void SelectNodesRec(Node node, ref Queue<Node> selection, System.Func<Node, bool> filter)
        {
            if (filter(node))
            {
                selection.Enqueue(node);
            }
            foreach (Node child in node.children)
            {
                SelectNodesRec(child, ref selection, filter);
            }
        }

        public void Simplify(float angleThreshold, float radiusTreshold)
        {
            foreach(Node stem in stems)
            {
                stem.Simplify(null, angleThreshold, radiusTreshold);
            }
        }


        public void GenerateMeshData(TrunkFunction trunk, float simplifyLeafs, float radialResolution,float VColBarkModifier,float VColLeafModifier)
        {
            Stack<Queue<TreePoint>> treePoints = new Stack<Queue<TreePoint>>();
            foreach (Node stem in stems)
            {
                Stack<Queue<TreePoint>> newPoints = stem.ToSplines();
                while (newPoints.Count > 0)
                {
                    treePoints.Push(newPoints.Pop());
                }
            }
            Splines splines = new Splines(treePoints);
            splines.GenerateMeshData(7 * radialResolution, 3, trunk.rootShape, trunk.radiusMultiplier, trunk.rootRadius, trunk.rootInnerRadius, trunk.rootHeight, trunk.rootResolution
                                    , trunk.flareNumber, trunk.displacementStrength, trunk.displacementSize, trunk.spinAmount,VColBarkModifier);


            Queue<int> leafTriangles = new Queue<int>();
            GenerateLeafData(splines.verts, splines.normals, splines.uvs, leafTriangles, splines.verts.Count, splines.colors, simplifyLeafs,VColBarkModifier,VColLeafModifier);
            verts = splines.verts.ToArray();
            normals = splines.normals.ToArray();
            uvs = splines.uvs.ToArray();
            triangles = splines.triangles.ToArray();
            colors = splines.colors.ToArray();
            this.leafTriangles = leafTriangles.ToArray();
        }


        public void GenerateLeafData(Queue<Vector3> leafVerts, Queue<Vector3> leafNormals, Queue<Vector2> leafUvs, Queue<int> leafTriangles, int vertexOffset, Queue<Color> colors,
                                        float simplify,float VColBarkModifier,float VColLeafModifier)
        {            
            foreach(LeafPoint l in leafPoints)
            {
                Mesh leafMesh = l.GetMesh(simplify > .1f ? 1 : 0);
                if (leafMesh == null || Random.value < simplify) // don't create the leaf when no object is available or when simplifying
                    continue;

                int n = leafMesh.vertexCount;
                Quaternion rot = Quaternion.FromToRotation(Vector3.forward, l.direction);
                rot = Quaternion.LookRotation(l.direction);
                float randomWindPhase = Random.value; // used is wind shader to randomise vertex displacement phase
                float maxDistance = 0f;
                float minDistance = Mathf.Infinity;
                float[] distances = new float[n];
                for (int i = 0; i < n; i++) // iterating over leaf object vertices
                {
                    Vector3 vert = leafMesh.vertices[i];
                    Vector3 normal = leafMesh.normals[i];
                    if (!l.procedural)
                    {
                        normal = rot * normal;
                        vert = rot * vert * l.size * (1 + simplify * .5f) + l.position;
                    }
                    leafVerts.Enqueue(vert);
                    if (l.overrideNormals)
                    {
                        leafNormals.Enqueue(l.position.normalized);
                    }
                    else
                    {
                        leafNormals.Enqueue(normal);
                    }
                    leafUvs.Enqueue(leafMesh.uv[i]);
                    float distance = Vector3.Distance(l.position, vert);
                    maxDistance = Mathf.Max(maxDistance, distance);
                    minDistance = Mathf.Min(minDistance, distance);
                    distances[i] = distance;
                }
                for (int i=0; i<n; i++)
                {

                    float blueChannel = (distances[i] - minDistance);
                    colors.Enqueue(new Color((l.distanceFromOrigin/10) * VColBarkModifier, randomWindPhase , blueChannel * VColLeafModifier));
                }
                int m = leafMesh.triangles.Length;
                for (int i = 0; i < m; i++) // creating the triangles
                {
                    leafTriangles.Enqueue(leafMesh.triangles[i] + vertexOffset);
                }
                vertexOffset += n; // used when creating submeshes for the mesh component
                
            }
        }

        
        public List<Vector3> DebugPositions()
        {
            List<Vector3> positions = new List<Vector3>();
            foreach(Node stem in stems)
            {
                stem.DebugPosRec(positions);
            }
            return positions;
        }
    }

}
