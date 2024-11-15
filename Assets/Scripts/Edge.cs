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
    public Vector3 midPoint;

    public Quaternion facingNormal;
    public Vector3 startUp;
    public Vector3 endUp;
    public bool facingNormalCalculated = false;


    public Edge(Vector3 startPoint, Vector3 endPoint)
    {
        start = startPoint;
        end = endPoint;
        length = Vector3.Distance(start, end);
        midPoint = Vector3.Lerp(start, end, 0.5f);
    }

    public override string ToString()
    {
        return $"vector: {start} - {end}";
    }
}
