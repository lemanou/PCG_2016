using UnityEngine;

/*
    This script shoots a ray 2 meters ahead of the camera to check for possible mouse interaction.
    It returns the gameobject GOHitByRay.
*/
public class RayFromCrosshair : MonoBehaviour {
    public static ClickableFurniture GOHitByRay;
    public Transform _cameraTransform;

    private RaycastHit rayhit = new RaycastHit();
    private Vector3 _forward;

    void FixedUpdate() {
        GOHitByRay = null;
        _forward = _cameraTransform.forward;
        //Debug.DrawRay(transform.position, _forward * 2, Color.red);

        Ray mRay = new Ray(transform.position, _forward);

        if (Physics.Raycast(mRay, out rayhit, 2f)) {
            GOHitByRay = rayhit.collider.gameObject.GetComponent<ClickableFurniture>();
            //print(GOHitByRay.name);
        }
    }
}