using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
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
