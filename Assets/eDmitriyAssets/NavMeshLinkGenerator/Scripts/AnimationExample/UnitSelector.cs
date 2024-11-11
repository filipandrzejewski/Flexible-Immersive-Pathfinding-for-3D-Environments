using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class UnitSelector : MonoBehaviour
{
    public NavMeshAgent navMeshAgent;




    void Update()
    {
        if( Input.GetMouseButtonDown( 0 ) )
        {
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
            RaycastHit hit;

            if( Physics.Raycast( ray, out hit ) )
            {
                if( navMeshAgent != null )
                {
                    navMeshAgent.SetDestination( hit.point );
                    //navMeshAgent.GetComponent< Agent >().LookAtPos = hit.point;
                }
            }
        }
    }


}
