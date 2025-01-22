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
            if (NavLinkManager.Instance == null || NavLinkManager.Instance.GetLinkDataList() == null)
            {
                Debug.LogWarning($"NavLinkManager or navLinks list is not initialized. Could properely remove link: {gameObject.name}. \nClick Update Links to fully remove and use Auto Assign button to properely initialize NavLinkManager.");
                return;
            }

            int removedCount = NavLinkManager.Instance.DeleteLink(this);

            if (removedCount > 0)
            {
                Debug.Log($"Removed {removedCount} link(s) associated with {this} from NavLinkManager.");
                EditorUtility.SetDirty(NavLinkManager.Instance);

            }
        }
    }
}
