namespace FlexiblePathfindingSystem3D
{
    using System;
    using UnityEngine;
    using Unity.AI.Navigation;

    [Serializable]
    public class LinkData
    {
        public float length;
        public float angle;
        public NavMeshLink linkComponent;
        public bool wasGenerated;
        public Vector3 linkObjectPosition;

        [SerializeField]
        private int ogCostModifier;

        public LinkData(Vector3 startPoint, Vector3 endPoint, NavMeshLink _linkComponent, bool generated)
        {
            if (_linkComponent == null)
            {
                Debug.LogWarning("No Link component has been passed!");
            }
            linkObjectPosition = startPoint;
            linkComponent = _linkComponent;
            length = Vector3.Distance(start, end);
            ogCostModifier = _linkComponent.costModifier;
            Vector3 direction = (end - start).normalized;
            Vector3 flat = new Vector3(direction.x, 0, direction.z);
            angle = Vector3.Angle(direction, flat);
            if (end.y < start.y) { angle = -angle; }

            wasGenerated = generated;
        }

        public Vector3 start
        {
            get => linkComponent.startPoint + linkObjectPosition;
        }

        public Vector3 end
        {
            get => linkComponent.endPoint + linkObjectPosition;
        }

        public void RevertCost()
        {
            linkComponent.costModifier = ogCostModifier;
        }

        public void LinkActive(bool activate)
        {
            linkComponent.transform.gameObject.SetActive(activate);
        }
    }
}
