using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEditor;

[Serializable]
public struct LinkData
{
    public Vector3 start;
    public Vector3 end;
    public float length;
    public float angle;
    public int ogCostModifier;
    public NavMeshLink linkComponent;

    public LinkData(Vector3 startPoint, Vector3 endPoint, NavMeshLink _linkObject)
    {
        this.start = startPoint;
        this.end = endPoint;
        this.length = Vector3.Distance(start, end);
        this.ogCostModifier = _linkObject.costModifier;
        Vector3 direction = (end - start).normalized;
        Vector3 flat = new Vector3(direction.x, 0, direction.z);
        this.angle = Vector3.Angle(direction, flat);
        if (end.y < start.y) { angle = -angle; }
        linkComponent = _linkObject;
    }

    public bool MatchesLink(NavMeshLink link)
    {
        return link == linkComponent;
    }
}
