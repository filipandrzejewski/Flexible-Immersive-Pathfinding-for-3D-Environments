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

    public void RequestPath(CharacterMovement character, Vector3 destination, Action<bool> onPathCalculated = null)
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
            if (link.length > 7) { link.linkComponent.costModifier = 999; }
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
            link.linkComponent.costModifier = link.ogCostModifier;
        }
    }

    private async void ProcessPathAsync(NavRequest request)
    {
        NavMeshPath path = new NavMeshPath();
        bool hasPath = false;
        Vector3 startPosition = request.character.GetAgent().transform.position;
        int areaMask = request.character.GetAgent().areaMask;

        // Use NavMesh.CalculatePath instead of agent.CalculatePath
        hasPath = await Task.Run(() =>
        {
            return NavMesh.CalculatePath(startPosition, request.destination, areaMask, path);
        });

        // Switch back to main thread to set the path
        await UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            if (hasPath)
            {
                request.character.GetAgent().SetPath(path); // Safe on the main thread
            }

            request.onPathCalculated?.Invoke(hasPath);
            ProcessNextRequest();
        });
    }

    public void UpdateLinks() // DELETE EMPTY LINKS HERE
    {
        NavMeshLink[] allLinks = FindObjectsOfType<NavMeshLink>();

        foreach (NavMeshLink link in allLinks)
        {
            bool linkFound = false;

            for (int i = 0; i < navLinks.Count; i++)
            {
                if (navLinks[i].MatchesLink(link))
                {
                    // Update existing link data
                    navLinks[i] = new LinkData(link.startPoint, link.endPoint, link);
                    linkFound = true;
                    break;
                }
            }

            if (!linkFound)
            {
                // Add new link data if no match is found
                LinkData newData = new LinkData(link.startPoint, link.endPoint, link);
                navLinks.Add(newData);
            }
        }

        // Remove any entries that no longer have a valid link reference
        navLinks.RemoveAll(data => data.linkComponent == null);

        #if UNITY_EDITOR
        EditorUtility.SetDirty(this); // Mark the object as dirty to save changes
        #endif
    }

    public void DeleteLinks()
    {
        navLinks.Clear();
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

        if (GUILayout.Button("Update Links"))
        {
            manager.UpdateLinks();
            Debug.Log("NavMesh Links have been updated.");
        }
        if (GUILayout.Button("Delete Links"))
        {
            manager.DeleteLinks();
            Debug.Log("NavMesh Links have been deleted.");
        }
    }
}
#endif

/// Struct for navigation requests.
public struct NavRequest
{
    public CharacterMovement character;
    public Vector3 destination;
    public Action<bool> onPathCalculated;

    public NavRequest(CharacterMovement character, Vector3 destination, Action<bool> onPathCalculated)
    {
        this.character = character;
        this.destination = destination;
        this.onPathCalculated = onPathCalculated;
    }
}
