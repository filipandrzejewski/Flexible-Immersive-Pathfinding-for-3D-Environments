namespace FlexiblePathfindingSystem3D
{
    using UnityEngine;

    public class PlayerController : MonoBehaviour
    {
        // this class serves as an example on how to use the custom pathfinding system
        // it should be replaced with more suitable controller for the project
        public enum MouseButton
        {
            LeftClick = 0,
            RightClick = 1,
            MiddleClick = 2
        }
        public MouseButton controlMouseButton = MouseButton.RightClick;
        public PhysicalStatsLogic physicalStats;
        public Camera mainCamera;

        private void Start()
        {
            if (physicalStats == null)
            {
                physicalStats = GetComponent<PhysicalStatsLogic>();
                if (physicalStats == null)
                {
                    Debug.LogWarning($"{gameObject.name} player controller is missing a PhysicalStatsLogic component.");
                }
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown((int)controlMouseButton))
            {
                Ray reycastClick = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(reycastClick, out var hitInfo))
                {
                    NavLinkManager.Instance.RequestPath(physicalStats, hitInfo.point);
                }
            }

        }
    }
}
