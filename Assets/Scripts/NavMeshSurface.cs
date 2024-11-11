using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using DG.Tweening;

public class NavMeshSurfaceTest : MonoBehaviour
{
    public NavMeshSurface surface;
    public NavMeshLinkData link;

    void Start()
    {
        List < NavMeshSurface > navMeshList = NavMeshSurface.activeSurfaces;
        NavMeshData navMeshData = surface.navMeshData;
        Debug.Log(navMeshList);
        Debug.Log(navMeshData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
