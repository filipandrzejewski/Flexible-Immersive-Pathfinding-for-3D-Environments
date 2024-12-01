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
    public Vector3 pivotPoint; // currently a midPoint moved by offset necessary so that the point be outside of the ledge 
    public bool hasPivotPoint = false;

    public Quaternion facingNormal;
    public Vector3 startUp;
    public Vector3 endUp;
    public bool facingNormalCalculated = false;

    private float pivotDistance = 0.3f;


    public Edge(Vector3 startPoint, Vector3 endPoint)
    {
        start = startPoint;
        end = endPoint;
        length = Vector3.Distance(start, end);
        midPoint = Vector3.Lerp(start, end, 0.5f);
        //pivotDistance = // have to get it from static class
        CalculatePivot();
    }

    private void CalculatePivot()
    {
        // check 2 perpendicullar spots to the edge
        Vector3 edgeDirection = (end - start).normalized;
        edgeDirection.y = 0;
        Vector3 perpendicularDirection = Vector3.Cross(edgeDirection, Vector3.up).normalized;
        Vector3 pivot1 = midPoint + perpendicularDirection * pivotDistance;
        Vector3 pivot2 = midPoint - perpendicularDirection * pivotDistance;

        // Check if the first pivot point is valid
        if (IsPivotValid(pivot1))
        {
            pivotPoint = pivot1;
            hasPivotPoint = true;
            return;
        }
        // Check if the second pivot point is valid
        else if (IsPivotValid(pivot2))
        {
            pivotPoint = pivot2;
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
        if (Physics.Raycast(pivot, Vector3.down, 0.1f))
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
