namespace FlexiblePathfindingSystem3D
{
    using UnityEngine;
    using Unity.AI.Navigation;
    using UnityEditor;

    [ExecuteInEditMode]
    public class SmartNavMeshLink : NavMeshLink
    {
        private void OnDestroy()
        {
            if (NavLinkManager.Instance == null || NavLinkManager.Instance.navLinks == null)
            {
                Debug.LogWarning($"NavLinkManager or navLinks list is not initialized. Could properely remove link: {gameObject.name}. Click update links to fully remove.");
                return;
            }

            int removedCount = NavLinkManager.Instance.navLinks.RemoveAll(linkData => linkData.linkComponent == this);

            if (removedCount > 0)
            {
                Debug.Log($"Removed {removedCount} link(s) associated with {this} from NavLinkManager.");
                EditorUtility.SetDirty(NavLinkManager.Instance);

            }
            else
            {
                Debug.LogWarning($"No matching link found for {gameObject.name} in NavLinkManager.");
            }
        }
    }
}
