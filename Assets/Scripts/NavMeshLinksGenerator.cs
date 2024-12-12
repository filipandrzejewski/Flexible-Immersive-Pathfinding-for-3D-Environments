using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshLinksGenerator : MonoBehaviour
{
    public Transform linkPrefab;
    public GameObject debugPivotPointPrefab;
    public NavMeshSurface navMeshSurface;

    private float linkCheckOffset;
    private NavMeshBuildSettings navMeshSettings;

    private Transform allLinksGroup;
    public float minEdgeLength = 0.2f; // minimal distance to classify Edge (will not work on high res mesh with rounded corners, need to simplify edges in the calculations)
    private List<Edge> edges = new List<Edge>();
    private Vector3[] vertices; // every vertice of the nav mesh
    private int[] pairIndices; // connections between vertices paired by index in the vertices table

    private void Start()
    {
        FindEdges();
        allLinksGroup = new GameObject("AllLinksGroup").transform;
        GetCurrentNavMeshSettings();
        //CreateLinks();

        navMeshSurface.BuildNavMesh();

    }

    private void Update()
    {

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
            PairToEdge(i, i + 1, i + 2);
            PairToEdge(i + 1, i + 2, i);
            PairToEdge(i + 2, i, i + 1);
        }


        foreach (Edge edge in edges)
        {
            if (edge.hasPivotPoint)
            {
                Instantiate(debugPivotPointPrefab, edge.falloffPivotPoint, Quaternion.identity);
            }
            else
            {
                Instantiate(LinkController.Instance.debugCornerPointPrefab, edge.start, Quaternion.identity);
                Instantiate(LinkController.Instance.debugCornerPointPrefab, edge.end, Quaternion.identity);
            }
        }

        edges.RemoveAll(edge => !edge.hasPivotPoint);
    }

    private void PairToEdge(int n1, int n2, int n3) //N1 and N2 will be used for calculating the edge, N3 will be used to calculate the norlmal of the plane that the edge is connected to  
    {
        Vector3 point1 = vertices[pairIndices[n1]];
        Vector3 point2 = vertices[pairIndices[n2]];

        if (Vector3.Distance(point1, point2) < minEdgeLength)
        {
            return; // (will not work on high res mesh with rounded corners, need to simplify edges in the calculations)
        }

        Vector3 point3 = vertices[pairIndices[n3]];
        Vector3 surfaceVector = point3 - point1;

        Vector3 surfaceNormal = Vector3.Cross(point1 - point2, surfaceVector).normalized;


        Edge newEdge = new Edge(point1, point2, surfaceNormal);

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

    public void CreateLinks()
    {
        Debug.Log("Creating Links");
        if (linkPrefab == null) { return; }

        foreach (Edge edge in edges)
        {
            foreach (Edge targetEdge in edges)
            {
                if (edge == targetEdge) continue; // Skip self-comparison
                Vector3 direction = (targetEdge.falloffPivotPoint - edge.falloffPivotPoint).normalized;
                float distance = Vector3.Distance(targetEdge.falloffPivotPoint, edge.falloffPivotPoint);

                if (!Physics.Raycast(edge.falloffPivotPoint, direction, distance) && !Physics.Raycast(targetEdge.falloffPivotPoint, -direction, distance)) // no collisions detected both ways 
                {
                    // DRAW DEBUG LINES FOR CONNECTIONS ---------------------------------
                    Debug.DrawLine(edge.midPoint, targetEdge.midPoint, Color.green, 5.0f); // Draw green line
                    
                    Transform linkObject = Instantiate(linkPrefab.transform, edge.midPoint, Quaternion.identity); // prev: Quaternion.LookRotation(direction)
                    var link = linkObject.GetComponent<NavMeshLink>();

                    Vector3 globalEndPoint = targetEdge.midPoint;
                    Vector3 localEndPoint = linkObject.InverseTransformPoint(globalEndPoint);

                    link.endPoint = localEndPoint;
                    link.UpdateLink();
                    linkObject.transform.SetParent(allLinksGroup);




                }
                else
                {
                    // DRAW DEBUG LINES FOR CONNECTIONS ---------------------------------
                    Debug.DrawLine(edge.midPoint, targetEdge.midPoint, Color.red, 5.0f); // Draw red line
                }

                // NEW This is working! Adjustment needed: prevent links from connecting on wide angles from edge normal. Could also implement edge normal as an Edge variable and maybe make it 3d so then later I could play with the steapness of edges.

            }
        } 
    }


    // To make connections properely I would have to distinguish between specific areas as well as concave or convex edges (inside or outside of areas)
    // Maybe the easier method would be to connect everything with everything in range in order to just force unity pathfinding to select the most straight path?

    // Or maybe create the path myself giving character goals to rach along the way?
    // Is it even possible to create path manually like that?

    void HighlightEdges()
    {
        foreach (var edge in edges)
        {
            Vector3 start = edge.start;
            Vector3 end = edge.end;

            Debug.DrawLine(start, end, Color.red, 5f);
        }
    }

    public void HighlightEdgeDirections()
    {
        foreach (var edge in edges)
        {
            Vector3 start = edge.midPoint;
            Vector3 end = edge.midPoint + edge.falloffDirection * 2;

            Debug.DrawLine(start, end, Color.blue, 5f);
        }

    }

    public void ClearLinksAndEdges()
    {
        foreach (Transform child in allLinksGroup.transform)
        {
            Destroy(child.gameObject);
        }

        edges.Clear();
    }
}

[CustomEditor(typeof(NavMeshLinksGenerator))]
public class EdgeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NavMeshLinksGenerator navMeshLinks = (NavMeshLinksGenerator)target;

        if (GUILayout.Button("Clear Links and Edges"))
        {
            navMeshLinks.ClearLinksAndEdges();
        }

        if (GUILayout.Button("Create Links"))
        {
            navMeshLinks.CreateLinks();
        }

        if (GUILayout.Button("Highlight Directions"))
        {
            navMeshLinks.HighlightEdgeDirections();
        }
    }
}
