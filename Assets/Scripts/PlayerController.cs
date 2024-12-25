using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : CharacterMovement
{
    public NavMeshAgent nvAgent;
    public Camera mainCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray reycastClick = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(reycastClick, out var hitInfo))
            {
                NavLinkManager.Instance.RequestPath(this, hitInfo.point);
                //nvAgent.SetDestination(hitInfo.point);
            }
        }
        
    }
}
