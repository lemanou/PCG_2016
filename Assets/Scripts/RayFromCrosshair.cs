using UnityEngine;
using System.Collections;

public class RayFromCrosshair : MonoBehaviour
{
    public static GameObject GOHitByRay;
    public Transform _cameraTransform;

    private RaycastHit rayhit = new RaycastHit();
    private Vector3 _forward;

    void FixedUpdate()
    {
        GOHitByRay = null;
        _forward = _cameraTransform.forward;
        //Debug.DrawRay(transform.position, _forward, Color.green);

        Ray mRay = new Ray(transform.position, _forward);

        if (Physics.Raycast(mRay, out rayhit, 2f))
        {
            GOHitByRay = rayhit.collider.gameObject;
            //print(GOHitByRay.name);
        }
    }
}