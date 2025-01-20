namespace FlexiblePathfindingSystem3D
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;

    [Serializable]
    public class Edge
    {
        public Vector3 start;
        public Vector3 end;
        public float length;
        public Vector3 edgeSurfaceNormal; // normal of a surface this edge is a part of 
        public float correction; // correction for common Unity bug where navMesh is generated slightly under the geometry

        // Points are structured in form of lists. Having multiple points could later be used to smooth the connections so that every connection uses the straightest path to the edge.
        public List<Vector3> connectionPoint = new List<Vector3>();
        public List<Vector3> falloffPoint = new List<Vector3>(); // points in the middle of the edge slightly shifted towards the ledge 

        public Vector3 falloffDirection; // direction of the ledge - where mesh ends or goes steeply down
        public bool hasPivotPoint = false;

        public float falloffDistance; // large enought offset to be hovering over the edge. Should always be equal to player width or slightly higher because the mesh is leaving gaps ROUGHLY (not exactly apparently) the size of half of the player width
        public float pivotCheckDistance; // check distance for ground to determine if the falloffpoint is hovering over the edge. Should always be equal to player step height or slightly lower just to be sure


        public Edge(Vector3 startPoint, Vector3 endPoint, Vector3 surfaceNormal, (NavMeshBuildSettings settings, float correction) navMesh)
        {
            correction = navMesh.correction;
            falloffDistance = (float)(navMesh.settings.agentRadius * 1.1 + 0.1);
            //falloffDistance = 0.7f;
            pivotCheckDistance = (float)(navMesh.settings.agentClimb * 1.1 + 0.1) + correction;
            //pivotCheckDistance = 0.4f;
            start = startPoint;
            end = endPoint;
            length = Vector3.Distance(start, end);
            connectionPoint.Add(Vector3.Lerp(start, end, 0.5f));
            edgeSurfaceNormal = surfaceNormal;
            falloffDirection = Vector3.Cross(startPoint - endPoint, surfaceNormal).normalized;
            CalculateFalloffPivots();
        }

        public Edge(Edge copiedEdge)
        {
            start = copiedEdge.start;
            end = copiedEdge.end;
            length = copiedEdge.length;
            edgeSurfaceNormal = copiedEdge.edgeSurfaceNormal;

            connectionPoint = new List<Vector3>(copiedEdge.connectionPoint);
            falloffPoint = new List<Vector3>(copiedEdge.falloffPoint);

            falloffDirection = copiedEdge.falloffDirection;
            hasPivotPoint = copiedEdge.hasPivotPoint;
            falloffDistance = copiedEdge.falloffDistance;
            pivotCheckDistance = copiedEdge.pivotCheckDistance;
        }

        private void CalculateFalloffPivots()
        {
            foreach (Vector3 point in connectionPoint)
            {
                // check 2 perpendicullar spots to the edge
                Vector3 edgeDirection = (end - start).normalized;
                edgeDirection.y = 0;
                Vector3 positivePivot = point + falloffDirection * falloffDistance;
                Vector3 negativePivot = point - falloffDirection * falloffDistance;

                // Check if the first pivot point is valid
                if (IsPivotValid(positivePivot, point))
                {
                    falloffPoint.Add(positivePivot);
                    hasPivotPoint = true;
                    continue;
                }
                // Check if the second pivot point is valid
                else if (IsPivotValid(negativePivot, point))
                {
                    falloffPoint.Add(negativePivot);
                    falloffDirection = -falloffDirection; //when the ledge was detected in the other way the falloff direction is reversed
                    hasPivotPoint = true;
                    continue;
                }
                else
                {
                    // No valid pivot point found
                    //pivotPoint = null;
                    continue;
                }
            }


        }

        private bool IsPivotValid(Vector3 pivot, Vector3 point)
        {
            // Check if the pivot space is clear (perpendicular raycast)
            if (Physics.Raycast(point + Vector3.up * correction, (pivot - point).normalized, falloffDistance))
            {
                return false;  // Obstacle in the pivot placement
            }

            //Cast a short ray downward from the pivot to detect a fall
            if (Physics.Raycast(pivot + Vector3.up * correction, -edgeSurfaceNormal, pivotCheckDistance))
            {
                return false;  // Hit something beneath, not hovering
            }

            //Pivot is in a free, hovering space
            return true;
        }

        public override string ToString()
        {
            return $"vector: {start} - {end}";
        }
    }
}
