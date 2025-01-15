using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

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
            //Debug.Log($"Crossing a Link! --> {agent.currentOffMeshLinkData.endPos}");
            //Debug.DrawLine(transform.position, agent.currentOffMeshLinkData.endPos, Color.red, 3f);

            if (Vector3.Distance(transform.position, agent.currentOffMeshLinkData.endPos) > maxJumpDistance)
            {
                agent.isStopped = true;
                agent.ResetPath();
                NavMeshHit hit;

                if (NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }

                isCrossingLink = false;
                return;
            }

            if (agent.currentOffMeshLinkData.endPos.y - transform.position.y > maxJumpHeight)
            {
                agent.isStopped = true;
                agent.ResetPath();
                NavMeshHit hit;

                if (NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }

                isCrossingLink = false;
                return;
            }

            if (agent.currentOffMeshLinkData.endPos.y - transform.position.y < -maxDropDistance)
            {
                agent.isStopped = true;
                agent.ResetPath();
                NavMeshHit hit;

                if (NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }

                isCrossingLink = false;
                return;

                //agent.CompleteOffMeshLink();
                //agent.isStopped = true;
                //agent.ResetPath();
                //isCrossingLink = false;
                //return;
            }
        }

        //if (!agent.isOnOffMeshLink || isCrossingLink)
        //    return;

        //OffMeshLinkData linkData = agent.currentOffMeshLinkData;

        //if (!linkData.valid)
        //    return;

        //float linkHeightDifference = Mathf.Abs(linkData.endPos.y - linkData.startPos.y);

        //if (linkHeightDifference > maxJumpHeight)
        //{
        //    agent.isStopped = true;
        //    agent.ResetPath();
        //}
    }

    public void SetDestination(Vector3 destination)
    {
        if (agent == null)
            return;

        agent.isStopped = false;
        NavLinkManager.Instance.RequestPath(this, destination);
    }

    public NavMeshAgent GetAgent()
    {
        return agent;
    }
}
