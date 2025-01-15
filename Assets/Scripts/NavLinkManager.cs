using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Threading.Tasks;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavLinkManager : MonoBehaviour
{
    [SerializeField]
    public static NavLinkManager Instance { get; private set; }

    [SerializeField]
    public List<LinkData> navLinks = new List<LinkData>();

    //public bool isAsyncProcessingEnabled = true; //async queueing
    private Queue<NavRequest> requestQueue = new Queue<NavRequest>();
    private bool isProcessing = false;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log(navLinks);
    }

    public void AutoAssignInstance()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void RequestPath(PhysicalStatsLogic character, Vector3 destination, Action<bool> onPathCalculated = null)
    {
        NavRequest newRequest = new NavRequest(character, destination, onPathCalculated);
        requestQueue.Enqueue(newRequest);

        if (!isProcessing)
        {
            ProcessNextRequest();
        }
    }

    private void ProcessNextRequest()
    {
        if (requestQueue.Count == 0)
        {
            isProcessing = false;
            return;
        }

        isProcessing = true;
        NavRequest currentRequest = requestQueue.Dequeue();

        //if (isAsyncProcessingEnabled)
        //{
        //    ProcessPathAsync(currentRequest);
        //}

        ProcessPathSync(currentRequest);
        ProcessNextRequest(); // Do all requests

    }

    private void ProcessPathSync(NavRequest request)
    {
        foreach (LinkData link in navLinks)
        {
            if (link.length > request.character.maxJumpDistance) 
            { 
                link.linkComponent.costModifier = 9999;
                continue;
            }

            if (link.end.y - link.start.y > request.character.maxJumpHeight)
            {
                link.linkComponent.costModifier = 9999;
                continue;
            }

            if (link.end.y - link.start.y < -request.character.maxDropDistance)
            {
                link.linkComponent.costModifier = 9999;
                continue;
            }
        }
        NavMeshPath path = new NavMeshPath();
        bool hasPath = request.character.GetAgent().CalculatePath(request.destination, path);

        if (hasPath)
        {
            request.character.GetAgent().SetPath(path);
        }

        request.onPathCalculated?.Invoke(hasPath);

        foreach (LinkData link in navLinks)
        {
            link.RevertCost();
        }
    }

    //private async void ProcessPathAsync(NavRequest request)
    //{
    //    NavMeshPath path = new NavMeshPath();
    //    bool hasPath = false;
    //    Vector3 startPosition = request.character.GetAgent().transform.position;
    //    int areaMask = request.character.GetAgent().areaMask;

    //    // Use NavMesh.CalculatePath instead of agent.CalculatePath
    //    hasPath = await Task.Run(() =>
    //    {
    //        return NavMesh.CalculatePath(startPosition, request.destination, areaMask, path);
    //    });

    //    // Switch back to main thread to set the path
    //    await UnityMainThreadDispatcher.Instance.Enqueue(() =>
    //    {
    //        if (hasPath)
    //        {
    //            request.character.GetAgent().SetPath(path); // Safe on the main thread
    //        }

    //        request.onPathCalculated?.Invoke(hasPath);
    //        ProcessNextRequest();
    //    });
    //}

    public int UpdateLinks()
    {
        NavMeshLink[] allLinks = FindObjectsOfType<NavMeshLink>();

        int newLinksRecognized = 0;

        foreach (NavMeshLink link in allLinks)
        {
            bool linkMatchedExisting = false;

            for (int i = 0; i < navLinks.Count; i++)
            {
                if (navLinks[i].linkComponent == link)
                {
                    linkMatchedExisting = true;
                    break;
                }
            }

            if (!linkMatchedExisting) // Create new entry for existing link
            {
                LinkData newData = new LinkData(link.startPoint, link.endPoint, link, false);
                newLinksRecognized += 1;
                navLinks.Add(newData);
            }
        }

        // Cleanup linkData with no actuall reference to link object
        navLinks.RemoveAll(data => data.linkComponent == null);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif

        return newLinksRecognized;
    }

    public void DeleteLink(NavMeshLink delete)
    {
        navLinks.RemoveAll(linkData => linkData.linkComponent == delete);
    }

    public List<LinkData> GetLinkDataList()
    {
        return navLinks;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NavLinkManager))]
public class NavLinkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NavLinkManager manager = (NavLinkManager)target;
        NavMeshLinksGenerator generator = manager.GetComponent<NavMeshLinksGenerator>();

        if (generator == null)
        {
            EditorGUILayout.HelpBox("NavMeshLinksGenerator component not found on the same GameObject. Make sure the manager object holds both NavLinkGenerator and NavLinkManager scripts.", MessageType.Error);
            return;
        }

        if (GUILayout.Button("Create Links"))
        {
            generator.CreateLinks();
        }
        if (GUILayout.Button("Delete Links"))
        {
            generator.DeleteLinks();
        }
        if (GUILayout.Button("Highlight Links and Edges"))
        {
            generator.HighlightAll();
        }
        if (GUILayout.Button("Update Links"))
        {
            Debug.Log($"NavMesh Links have been updated.");
            manager.UpdateLinks();
            
        }

        if (GUILayout.Button("Auto Assign Components"))
        {
            manager.AutoAssignInstance();
            Debug.Log("NavMesh Links have been updated.");
        }

    }
}
#endif

/// Struct for navigation requests.
public struct NavRequest
{
    public PhysicalStatsLogic character;
    public Vector3 destination;
    public Action<bool> onPathCalculated;

    public NavRequest(PhysicalStatsLogic character, Vector3 destination, Action<bool> onPathCalculated)
    {
        this.character = character;
        this.destination = destination;
        this.onPathCalculated = onPathCalculated;
    }
}
