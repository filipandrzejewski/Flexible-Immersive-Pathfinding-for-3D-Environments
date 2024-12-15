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

    public float maxLinkDistance = 16;
    public float shortLinkDistance = 2;

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

        // DEBUG POINTS ----------------------------------------------
        //foreach (Edge edge in edges)
        //{
        //    if (edge.hasPivotPoint)
        //    {
        //        foreach (Vector3 falloffPoint in edge.falloffPoint)
        //        {
        //            Instantiate(debugPivotPointPrefab, falloffPoint, Quaternion.identity);
        //        }
        //    }
        //    else
        //    {
        //        Instantiate(LinkController.Instance.debugCornerPointPrefab, edge.start, Quaternion.identity);
        //        Instantiate(LinkController.Instance.debugCornerPointPrefab, edge.end, Quaternion.identity);
        //    }
        //}

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
                if (edge == targetEdge) { continue; }

                int startPointIndex = 0;
                int endPointIndex = 0;

                if (ValidConnectionExists(edge, targetEdge, out startPointIndex, out endPointIndex))
                {
                    Transform linkObject = Instantiate(linkPrefab.transform, edge.connectionPoint[startPointIndex], Quaternion.identity); // prev: Quaternion.LookRotation(direction) apparently rotation of link does not matter at all?
                    var link = linkObject.GetComponent<NavMeshLink>();

                    Vector3 startPoint = edge.connectionPoint[startPointIndex];

                    Vector3 globalEndPoint = targetEdge.connectionPoint[endPointIndex];
                    Vector3 localEndPoint = linkObject.InverseTransformPoint(globalEndPoint);

                    link.endPoint = localEndPoint;
                    link.UpdateLink();
                    linkObject.transform.SetParent(allLinksGroup);
                }

                // NEW This is working! Adjustment needed: prevent links from connecting on wide angles from edge normal. Could also implement edge normal as an Edge variable and maybe make it 3d so then later I could play with the steapness of edges.

            }
        } 
    }

    public bool ValidConnectionExists(Edge edge, Edge targetEdge, out int beginIndex, out int endIndex)
    {
        for (int i = 0; i < edge.falloffPoint.Count; i ++)
        {
            for (int j = 0; j < targetEdge.falloffPoint.Count; j++)
            {
                Vector3 direction = (targetEdge.falloffPoint[j] - edge.falloffPoint[i]).normalized;
                float distance = Vector3.Distance(targetEdge.falloffPoint[j], edge.falloffPoint[i]);

                if (maxLinkDistance > 0 & distance > maxLinkDistance) { continue; } // skip connections that are physically too long | 0 -> maxLinkDistance ignored
                if (shortLinkDistance > 0 & distance < shortLinkDistance) // loosen the angle restrictions on very short links | 0 -> shortLinkDistance ignored
                {
                    if (Vector3.Angle(direction, Vector3.Cross(edge.falloffDirection, -Vector3.Cross(edge.falloffDirection, Vector3.up))) > 65 &
                    Vector3.Angle(direction, Vector3.ProjectOnPlane(edge.falloffDirection, Vector3.up)) > 89) { continue; } 

                    if (Vector3.Angle(-direction, Vector3.Cross(targetEdge.falloffDirection, -Vector3.Cross(targetEdge.falloffDirection, Vector3.up))) > 65 &
                        Vector3.Angle(-direction, Vector3.ProjectOnPlane(targetEdge.falloffDirection, Vector3.up)) > 89) { continue; } 
                }
                else
                {
                    if (Vector3.Angle(direction, Vector3.Cross(edge.falloffDirection, -Vector3.Cross(edge.falloffDirection, Vector3.up))) > 30 &
                    //                                                                    ^ flat vector perpendicular to direction (-) Anticlockwise - because Unity is using lefthand model
                    Vector3.Angle(direction, Vector3.ProjectOnPlane(edge.falloffDirection, Vector3.up)) > 65) { continue; } //skip sharp connections with selected edge (both regarding to pararell direction pointing upwards and flattened direction in regards to the floor)

                    //if (Vector3.Angle(direction, edge.falloffDirection) > 65 &
                    //    Vector3.Angle(direction, Vector3.ProjectOnPlane(edge.falloffDirection, Vector3.up)) > 85) { continue; } //skip sharp connections with selected edge (both regarding to its direction and flattened direction in regards to the floor)

                    if (Vector3.Angle(-direction, Vector3.Cross(targetEdge.falloffDirection, -Vector3.Cross(targetEdge.falloffDirection, Vector3.up))) > 30 &
                        Vector3.Angle(-direction, Vector3.ProjectOnPlane(targetEdge.falloffDirection, Vector3.up)) > 65) { continue; } //skip same sharp connections with target edge 
                }


                if (!Physics.Raycast(edge.falloffPoint[i], direction, distance) && !Physics.Raycast(targetEdge.falloffPoint[j], -direction, distance)) // no collisions detected on a way between falloff points both ways
                {
                    // DRAW DEBUG LINES FOR CONNECTIONS ---------------------------------
                    Debug.DrawLine(edge.falloffPoint[i], targetEdge.falloffPoint[j], Color.green, 5.0f);
                    beginIndex = i;
                    endIndex = j;

                    return true;
                }
                else
                {
                    // DRAW DEBUG LINES FOR CONNECTIONS ---------------------------------
                    Debug.DrawLine(edge.falloffPoint[i], targetEdge.falloffPoint[j], Color.red, 5.0f);
                }

            }
        }
        beginIndex = 0;
        endIndex = 0;
        return false;
        
    }


    // To make connections properely I would have to distinguish between specific areas as well as concave or convex edges (inside or outside of areas)
    // Maybe the easier method would be to connect everything with everything in range in order to just force unity pathfinding to select the most straight path?

    // Or maybe create the path myself giving character goals to rach along the way?
    // Is it even possible to create path manually like that?

    void HighlightEdges()
    {
        foreach (Edge edge in edges)
        {
            Vector3 start = edge.start;
            Vector3 end = edge.end;

            Debug.DrawLine(start, end, Color.red, 5f);
        }
    }

    public void HighlightEdgeDirections()
    {
        foreach (Edge edge in edges)
        {
            foreach (Vector3 connectionPoint in edge.connectionPoint)
            {
                Vector3 start = connectionPoint;
                Vector3 end = connectionPoint + edge.falloffDirection * 2;
                Debug.DrawLine(start, end, Color.blue, 5f);

                Vector3 start2 = connectionPoint;
                Vector3 end2 = connectionPoint + Vector3.Cross(edge.falloffDirection, Vector3.Cross(edge.falloffDirection, Vector3.up)) * 2;
                Debug.DrawLine(start2, end2, Color.yellow, 5f);
            }
        }
    }

    public void DeleteLinks()
    {
        foreach (Transform child in allLinksGroup.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

[CustomEditor(typeof(NavMeshLinksGenerator))]
public class EdgeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NavMeshLinksGenerator navMeshLinks = (NavMeshLinksGenerator)target;

        if (GUILayout.Button("Delete Links"))
        {
            navMeshLinks.DeleteLinks();
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
