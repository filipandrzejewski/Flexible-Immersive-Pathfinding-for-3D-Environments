namespace FlexiblePathfindingSystem3D
{
    using UnityEngine;

    public class AngleTesting : MonoBehaviour
    {
        // This class is for testing purposes
        public Transform pivot;
        public Vector3 pathToPivot;
        public Vector3 falloffDirection;

        void Start()
        {

            falloffDirection = new Vector3(1, 0, 0);
            pathToPivot = transform.position - pivot.position;

        }
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
}
