namespace FlexiblePathfindingSystem3D
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AI;
    using Unity.AI.Navigation;
    using System;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif


    [ExecuteInEditMode]
    public class NavLinkManager : MonoBehaviour
    {
        [SerializeField]
        public static NavLinkManager Instance { get; private set; }

        [SerializeField]
        private List<LinkData> navLinks = new();

        //public bool isAsyncProcessingEnabled = true; //async queueing, not implemented
        private Queue<NavRequest> requestQueue = new();
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
            UpdateLinks();
        }

        public void RequestPath(PhysicalStatsLogic character, Vector3 destination, Action<bool> onPathCalculated = null)
        {
            NavRequest newRequest = new(character, destination, onPathCalculated);
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
            List<LinkData> invalidLinks = new();
            foreach (LinkData link in navLinks)
            {
                if (link.length > request.character.maxJumpDistance) 
                {
                    link.LinkActivate(false);
                    invalidLinks.Add(link);
                    continue;
                }

                if (link.End.y - link.Start.y > request.character.maxJumpHeight)
                {
                    link.LinkActivate(false);
                    invalidLinks.Add(link);
                    continue;
                }

                if (link.End.y - link.Start.y < -request.character.maxDropDistance)
                {
                    link.LinkActivate(false);
                    invalidLinks.Add(link);
                    continue;
                }
            }
            NavMeshPath path = new();
            bool hasPath = request.character.GetAgent().CalculatePath(request.destination, path);

            if (hasPath)
            {
                request.character.GetAgent().SetPath(path);
            }

            request.onPathCalculated?.Invoke(hasPath);

            foreach (LinkData link in invalidLinks)
            {
                link.LinkActivate(true);
            }
        }

        //For async queueing deactivating links will be problematic. Possibly use weights instead:
        /*
         * if (link.length > request.character.maxJumpDistance) 
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
         */


        //private async void ProcessPathAsync(NavRequest request)
        //{
        //    NavMeshPath path = new NavMeshPath();
        //    bool hasPath = false;
        //    Vector3 startPosition = request.character.GetAgent().transform.position;
        //    int areaMask = request.character.GetAgent().areaMask;

        //    hasPath = await Task.Run(() =>
        //    {
        //        return NavMesh.CalculatePath(startPosition, request.destination, areaMask, path);
        //    });

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


        //public int HighlightLinkDirections()
        //{
        //    int numberOfHighlightedLinks = 0;
        //    return numberOfHighlightedLinks;

        //}

        public void AddLink(LinkData newLink)
        {
            navLinks.Add(newLink);
        }

        public (int, int) UpdateLinks()
        {
            // Cleanup linkData with no actuall reference to link object
            int deletedLinks = navLinks.RemoveAll(data => data.linkComponent == null);

            foreach (LinkData link in navLinks)
            {
                link.RecalculateParameters();
            }

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

                if (!linkMatchedExisting) // No match for linkComponent means this entry does not yet exist within the list
                {
                    // link's start and edn points have to be transformed into global coordinates
                    LinkData newData = new(link.transform.position, link, false);
                    newLinksRecognized += 1;
                    navLinks.Add(newData);
                }
            }

    #if UNITY_EDITOR
            EditorUtility.SetDirty(this);
    #endif

            return (newLinksRecognized, deletedLinks);
        }

        public void DeleteAllLinks()
        {
            for (int i = navLinks.Count - 1; i >= 0; i--)
            {
                LinkData link = navLinks[i];

                if (!link.wasGenerated) { continue; }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (link.linkComponent != null)
                    {
                        DeleteLink(link.linkComponent);
                        DestroyImmediate(link.linkComponent.gameObject);
                    }
                }
                else
                {
                    if (link.linkComponent != null)
                    {
                        Destroy(link.linkComponent.gameObject);
                    }
                }
#else
            if (link.linkComponent != null)
            {
                Destroy(link.linkComponent.gameObject);
            }
#endif
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public int DeleteLink(NavMeshLink delete)
        {
            return navLinks.RemoveAll(linkData => linkData.linkComponent == delete);
        }

        public List<LinkData> GetLinkDataList()
        {
            return navLinks;
        }

        public void AutoAssignInstance()
        {
            if (Instance == null)
            {
                Instance = this;
            }
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
                manager.DeleteAllLinks();
            }
            if (GUILayout.Button("Highlight"))
            {
                Debug.Log($"Highlighted {generator.HighlightEdgeDirections()} valid Edge connections and their directions (Blue).");
                // potentially highlight link directions here as well
            }
            if (GUILayout.Button("Update Links"))
            {
                Debug.Log($"Updating NavMesh Links.");
                (int, int) result = manager.UpdateLinks();
                Debug.Log($"New links found and indexed: {result.Item1},\n Deleted links without reference: {result.Item2}");
            
            }

            if (GUILayout.Button("Auto Assign Components"))
            {
                Debug.Log("Assigning manager components.");
                manager.AutoAssignInstance();
                Debug.Log($"All components present = { generator.AutoAssign()}");
                
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
}
