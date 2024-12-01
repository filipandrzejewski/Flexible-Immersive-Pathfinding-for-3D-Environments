using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;

public class NavMeshLinksGenerator : MonoBehaviour
{
    public Transform linkPrefab;
    public GameObject debugPivotPointPrefab;
    public NavMeshSurface navMeshSurface;

    private float linkCheckOffset;
    private NavMeshBuildSettings navMeshSettings;

    private Transform linkGroup;
    public float minEdgeLength = 0.2f; // minimal distance to classify Edge (will not work on high res mesh with rounded corners, need to simplify edges in the calculations)
    private List<Edge> edges = new List<Edge>();
    private Vector3[] vertices; // every vertice of the nav mesh
    private int[] pairIndices; // connections between vertices paired by index in the vertices table

    private void Start()
    {
        FindEdges();
        linkGroup = new GameObject("LinkGroup").transform;
        GetCurrentNavMeshSettings();
        //CreateLinks();

        //navMeshSurface.BuildNavMesh();

    }

    private void Update()
    {
       HighlightEdges(edges);
    }

    private void GetCurrentNavMeshSettings()
    {
        navMeshSettings = NavMesh.GetSettingsByID(navMeshSurface.agentTypeID);
        linkCheckOffset = 0.6f * navMeshSettings.agentRadius;

    }

    private void FindEdges()
    {
        NavMeshTriangulation meshData = NavMesh.CalculateTriangulation();

        vertices = meshData.vertices;
        pairIndices = meshData.indices;


        for (int i = 0; i < pairIndices.Length - 1; i += 3)
        {
            // the process works based on triangles (even though the visual mesh may not) which is why the pair function is called 3 times)
            // If a triangle has its edge repeated in another triangle the connection should be deleted as it is not actually the Edge of the solid
            PairToEdge(i, i + 1);
            PairToEdge(i + 1, i + 2);
            PairToEdge(i + 2, i);
        }

        Debug.Log(edges.Count);


        foreach (Edge edge in edges)
        {
            if (edge.hasPivotPoint)
            {
                Instantiate(debugPivotPointPrefab, edge.pivotPoint, Quaternion.identity);
            }
            else
            {
                Instantiate(LinkController.Instance.debugCornerPointPrefab, edge.start, Quaternion.identity);
                Instantiate(LinkController.Instance.debugCornerPointPrefab, edge.end, Quaternion.identity);
            }
        }
    }

    private void PairToEdge(int n1, int n2)     
    {
        Vector3 point1 = vertices[pairIndices[n1]];
        Vector3 point2 = vertices[pairIndices[n2]];
        
        if (Vector3.Distance(point1, point2) < minEdgeLength)
        {
            return; // (will not work on high res mesh with rounded corners, need to simplify edges in the calculations)
        }

        Edge newEdge = new Edge(point1, point2);

        //remove duplicate connection as they are not edges
        foreach (Edge edge in edges)
        {
            if ((edge.start == point1 & edge.end == point2) || (edge.start == point2 & edge.end == point1))
            {
                edges.Remove(edge);
                return;
            }
        }

        edges.Add(newEdge);
    }

    private void CreateLinks()
    {
        if (linkPrefab == null) { return; }

        foreach (Edge edge in edges)
        {
            foreach (Edge targetEdge in edges)
            {
                Vector3 direction = (targetEdge.midPoint - edge.midPoint).normalized;
                float distance = Vector3.Distance(targetEdge.midPoint, edge.midPoint);

                if (!Physics.Raycast(edge.midPoint, direction, distance)) // no collision detected
                {
                    Transform linkObject = Instantiate(linkPrefab.transform, edge.midPoint, Quaternion.LookRotation(direction));
                    var link = linkObject.GetComponent<NavMeshLink>();
                    link.endPoint = linkObject.transform.InverseTransformPoint(targetEdge.midPoint); // transform global Vector3 targetEdge.midPoint into local relative to linkObject
                    link.UpdateLink();
                    linkObject.transform.SetParent(linkGroup);
                }
                
            }
        } 
    }


    // To make connections properely I would have to distinguish between specific areas as well as concave or convex edges (inside or outside of areas)
    // Maybe the easier method would be to connect everything with everything in range in order to just force unity pathfinding to select the most straight path?

    // Or maybe create the path myself giving character goals to rach along the way?
    // Is it even possible to create path manually like that?

    void HighlightEdges(List<Edge> edges)
    {
        foreach (var edge in edges)
        {
            Vector3 start = edge.start;
            Vector3 end = edge.end;

            Debug.DrawLine(start, end, Color.red);
        }
    }
}
