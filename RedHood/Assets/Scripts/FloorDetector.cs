using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDetector : MonoBehaviour
{
    [SerializeField] private Transform[] _rayOrigins;

    [SerializeField] private float _rayLength = 1.5f;

    [SerializeField] LayerMask _groundMask;




    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        foreach (Transform t in _rayOrigins)
        {

            Gizmos.DrawRay(t.position, Vector3.down);

        }
    }

    public Vector3 AverageHeight()
    {
        int hitCount = 0;
        Vector3 combinedPosition = Vector3.zero;
        RaycastHit hit;

        foreach(Transform t in _rayOrigins) // runs through all rays on the Player ...
        {
            if(Physics.Raycast(t.position, Vector3.down,out hit, _rayLength, _groundMask)) // if one of them hits an object with the layer mask "Ground"...
            {
                hitCount++; // ... it adds to hitCount
                combinedPosition += hit.point; //hit.point = position in the world where the raycast touched the collider
            }
        }

        Vector3 averagePosition = Vector3.zero;

        if(hitCount > 0) // if there is more than one hit...
        {
            averagePosition = combinedPosition / hitCount; // calculate the average of all hit positions
        }

        return averagePosition;

    }
}
