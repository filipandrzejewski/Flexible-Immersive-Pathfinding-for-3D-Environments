namespace FlexiblePathfindingSystem3D
{
    using UnityEngine;
    using UnityEngine.AI;

    public class PhysicalStatsLogic : MonoBehaviour
    {
        public float maxJumpHeight = 6.0f;
        public float maxJumpDistance = 12.0f;
        public float maxDropDistance = 10.0f;

        private NavMeshAgent agent;
        private bool isCrossingLink = false;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError($"{gameObject.name} character is missing a NavMeshAgent component.");
            }
        }

        void Update()
        {
            if (isCrossingLink && !agent.isOnOffMeshLink)
            {
                isCrossingLink = false;
            }

            if (agent.isOnOffMeshLink && !isCrossingLink)
            {
                isCrossingLink = true;

                // this code represents a moment where the character just entered a navigation link

                //this is a safeguard method for the system. Particulary usefull when using the 9999 weights method. If the 9999 weights method is not in use this function can be disabled for faster calculation
                //if (!QuickLinkValid())
                //{
                //    StopAndRepath();
                //}
            }
        }

        public bool QuickLinkValid()
        {
            // validates the link the character is currently stepping on
            if (agent.currentOffMeshLinkData.endPos != null)
            {
                if (Vector3.Distance(agent.currentOffMeshLinkData.startPos, agent.currentOffMeshLinkData.endPos) > maxJumpDistance)
                {
                    return false;
                }

                if (agent.currentOffMeshLinkData.endPos.y - agent.currentOffMeshLinkData.startPos.y > maxJumpHeight)
                {
                    return false;
                }

                if (agent.currentOffMeshLinkData.endPos.y - agent.currentOffMeshLinkData.startPos.y < -maxDropDistance)
                {
                    return false;
                }
            }
            return true;
        }

        public void StopAndRepath()
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.Warp(agent.currentOffMeshLinkData.startPos);

            // The warp method produces a noticable jitter in character motion, sometimes even apearing at the end of invalid link just for split second.
        }

        public NavMeshAgent GetAgent()
        {
            return agent;
        }
    }
}
