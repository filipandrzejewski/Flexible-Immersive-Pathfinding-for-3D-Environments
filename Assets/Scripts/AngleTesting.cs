using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleTesting : MonoBehaviour
{

    public Transform pivot;
    public Vector3 pathToPivot;
    public Vector3 falloffDirection;

    // Start is called before the first frame update
    void Start()
    {

        falloffDirection = new Vector3(1, 0, 0);
        pathToPivot = transform.position - pivot.position;

    }

    // Update is called once per frame
    void Update()
    {
        
        pathToPivot = transform.position - pivot.position;
        Debug.Log(Vector3.Angle(pathToPivot, falloffDirection));

        if (Vector3.Angle(pathToPivot, falloffDirection) > 60)
        {
            Debug.DrawLine(transform.position, pivot.position, Color.red, 0.2f);
        }
        else
        {
            Debug.DrawLine(transform.position, pivot.position, Color.green, 0.2f);
        }
            
    }
}
