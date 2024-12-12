using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Edge
{
    public Vector3 start;
    public Vector3 end;
    public float length;
    public Vector3 midPoint; // change to list of connection points
    public Vector3 falloffDirection; // direction of the ledge - where mesh ends or goes steeply down
    public Vector3 falloffPivotPoint; // point in the middle of the edge slightly shifted towards the ledge 
    public bool hasPivotPoint = false;

    public Quaternion facingNormal;
    public Vector3 startUp;
    public Vector3 endUp;
    public bool facingNormalCalculated = false;

    private float pivotDistance = 0.3f; // should be adjustable elsewhere
    private float pivotCheckDistance = 0.4f; // should always be equal to player step height or slightly lower just to be sure


    public Edge(Vector3 startPoint, Vector3 endPoint, Vector3 surfaceNormal)
    {
        start = startPoint;
        end = endPoint;
        length = Vector3.Distance(start, end);
        midPoint = Vector3.Lerp(start, end, 0.5f);
        falloffDirection = Vector3.Cross(startPoint - endPoint, surfaceNormal).normalized;
        CalculateFalloffPivot();
    }

    private void CalculateFalloffPivot()
    {
        // check 2 perpendicullar spots to the edge
        Vector3 edgeDirection = (end - start).normalized;
        edgeDirection.y = 0;
        Vector3 positivePivot = midPoint + falloffDirection * pivotDistance;
        Vector3 negativePivot = midPoint - falloffDirection * pivotDistance;

        // Check if the first pivot point is valid
        if (IsPivotValid(positivePivot))
        {
            falloffPivotPoint = positivePivot;
            hasPivotPoint = true;
            return;
        }
        // Check if the second pivot point is valid
        else if (IsPivotValid(negativePivot))
        {
            falloffPivotPoint = negativePivot;
            falloffDirection = -falloffDirection; //when the ledge was detected in the other way the falloff direction is reversed
            hasPivotPoint = true;
            return;
        }
        else
        {
            // No valid pivot point found
            //pivotPoint = null;
            return;
        }

    }

    private bool IsPivotValid(Vector3 pivot)
    {
        // Check if the pivot space is clear (perpendicular raycast)
        if (Physics.Raycast(midPoint, (pivot - midPoint).normalized, pivotDistance))
        {
            return false;  // Obstacle in the pivot placement
        }

        //Cast a short ray downward from the pivot to detect a fall
        if (Physics.Raycast(pivot, Vector3.down, pivotCheckDistance))
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
