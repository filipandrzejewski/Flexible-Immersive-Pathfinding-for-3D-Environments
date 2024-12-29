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
    public Vector3 edgeSurfaceNormal; // normal of a surface this edge is a part of - used only to calculate the direction of raycast for pivot points as they should check for collision with the floor
    public List<Vector3> connectionPoint = new List<Vector3>();
    public Vector3 falloffDirection; // direction of the ledge - where mesh ends or goes steeply down
    public List<Vector3> falloffPoint = new List<Vector3>(); // points in the middle of the edge slightly shifted towards the ledge 
    public bool hasPivotPoint = false;

    public float falloffDistance; // large enought offset to be hovering over the edge. Should always be equal to player width or slightly higher because the mesh is leaving gaps ROUGHLY (not exactly apparently) the size of half of the player width
    public float pivotCheckDistance; // check distance for ground to determine if the falloffpoint is hovering over the edge. Should always be equal to player step height or slightly lower just to be sure


    public Edge(Vector3 startPoint, Vector3 endPoint, Vector3 surfaceNormal, NavMeshBuildSettings navMeshSettings )
    {
        falloffDistance = (float)(navMeshSettings.agentRadius * 1.1 + 0.1);
        //falloffDistance = 0.7f;
        pivotCheckDistance = (float)(navMeshSettings.agentClimb * 1.1 + 0.1);
        //pivotCheckDistance = 0.4f;
        start = startPoint;
        end = endPoint;
        length = Vector3.Distance(start, end);
        connectionPoint.Add(Vector3.Lerp(start, end, 0.5f)); // 
        edgeSurfaceNormal = surfaceNormal;
        falloffDirection = Vector3.Cross(startPoint - endPoint, surfaceNormal).normalized;
        CalculateFalloffPivots();
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
        if (Physics.Raycast(point, (pivot - point).normalized, falloffDistance))
        {
            return false;  // Obstacle in the pivot placement
        }

        //Cast a short ray downward from the pivot to detect a fall
        if (Physics.Raycast(pivot, edgeSurfaceNormal, pivotCheckDistance))
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
