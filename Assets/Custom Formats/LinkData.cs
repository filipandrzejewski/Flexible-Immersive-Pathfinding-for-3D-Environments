namespace FlexiblePathfindingSystem3D
{
    using System;
    using UnityEngine;
    using Unity.AI.Navigation;

    [Serializable]
    public class LinkData
    {
        public Vector3 start;
        public Vector3 end;
        public float length;
        public float angle;
        public NavMeshLink linkComponent;
        public bool wasGenerated;

        [SerializeField]
        private int ogCostModifier;

        public LinkData(Vector3 startPoint, Vector3 endPoint, NavMeshLink _linkObject, bool generated)
        {
            if (_linkObject == null)
            {
                Debug.LogWarning("No Link component has been passed!");
            }
            this.start = startPoint;
            this.end = endPoint;
            this.length = Vector3.Distance(start, end);
            this.ogCostModifier = _linkObject.costModifier;
            Vector3 direction = (end - start).normalized;
            Vector3 flat = new Vector3(direction.x, 0, direction.z);
            this.angle = Vector3.Angle(direction, flat);
            if (end.y < start.y) { angle = -angle; }
            linkComponent = _linkObject;
            wasGenerated = generated;
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
