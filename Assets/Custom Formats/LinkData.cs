namespace FlexiblePathfindingSystem3D
{
    using System;
    using UnityEngine;
    using Unity.AI.Navigation;

    [Serializable]
    public class LinkData
    {

        public Vector3 Start
        {
            get => linkComponent.startPoint + linkObjectPosition;
        }

        public Vector3 End
        {
            get => linkComponent.endPoint + linkObjectPosition;
        }

        public float length;

        //public float angle; // this is not necessary to be kept as a field. Angle calculations should be made in the navMeshLinkGenerator based on basic data above

        public NavMeshLink linkComponent;
        public Vector3 linkObjectPosition;
        [SerializeField]
        private int ogCostModifier;

        public bool wasGenerated;

        public LinkData(Vector3 origin, NavMeshLink _linkComponent, bool generated)
        {
            if (_linkComponent == null)
            {
                Debug.LogWarning("No Link component has been passed!");
            }
            
            linkComponent = _linkComponent;
            ogCostModifier = (int)_linkComponent.costModifier;
            linkObjectPosition = origin;

            length = Vector3.Distance(Start, End);
            // angle calculations:
            //Vector3 direction = (End - Start).normalized;
            //Vector3 flat = new Vector3(direction.x, 0, direction.z);
            //angle = Vector3.Angle(direction, flat);
            //if (End.y < Start.y) { angle = -angle; }

            wasGenerated = generated;
        }


        // after updating link position. This method can be substituted with parameters
        public void RecalculateParameters()
        {
            linkObjectPosition = linkComponent.transform.position;
            length = Vector3.Distance(Start, End);
            // angle calculations:
            //Vector3 direction = (End - Start).normalized;
            //Vector3 flat = new Vector3(direction.x, 0, direction.z);
            //angle = Vector3.Angle(direction, flat);
            //if (End.y < Start.y) { angle = -angle; }
        }

        public void RevertCost()
        {
            linkComponent.costModifier = ogCostModifier;
        }

        public void LinkActivate(bool activate)
        {
            linkComponent.transform.gameObject.SetActive(activate);
        }
    }
}
