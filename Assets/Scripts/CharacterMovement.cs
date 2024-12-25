using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class CharacterMovement : MonoBehaviour
{
    public float maxJumpHeight = 2.0f;

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
        if (!agent.isOnOffMeshLink || isCrossingLink)
            return;

        OffMeshLinkData linkData = agent.currentOffMeshLinkData;

        if (!linkData.valid)
            return;

        float linkHeightDifference = Mathf.Abs(linkData.endPos.y - linkData.startPos.y);

        if (linkHeightDifference > maxJumpHeight)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
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
