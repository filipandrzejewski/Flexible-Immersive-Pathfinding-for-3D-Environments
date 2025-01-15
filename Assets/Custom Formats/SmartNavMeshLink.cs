using UnityEngine;
using Unity.AI.Navigation;
using UnityEditor;


public class SmartNavMeshLink : NavMeshLink
{
    private void OnDestroy()
    {
        if (NavLinkManager.Instance == null || NavLinkManager.Instance.navLinks == null)
        {
            //Debug.LogWarning($"NavLinkManager or navLinks is not initialized. Could not remove link: {gameObject.name}");
            return;
        }

        int removedCount = NavLinkManager.Instance.navLinks.RemoveAll(linkData => linkData.linkComponent == this);

        if (removedCount > 0)
        {
            Debug.Log($"Removed {removedCount} link(s) associated with {this} from NavLinkManager.");
#if UNITY_EDITOR
            EditorUtility.SetDirty(NavLinkManager.Instance); // Ensure changes are saved in Editor
#endif
        }
        else
        {
            Debug.LogWarning($"No matching link found for {gameObject.name} in NavLinkManager.");
        }
    }
}
