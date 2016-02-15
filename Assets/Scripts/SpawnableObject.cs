using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableObject : MonoBehaviour {

    public enum Tag {
        Short,
        Tall
    }

    public enum Placement {
        NotSet,
        Middle,
        Wall
    }

    public enum Facing {
        North,
        East,
        South,
        West
    }

    public Tag localTag = Tag.Short;
    public Placement localPlacement = Placement.NotSet;
    public Facing localFacing = Facing.North;

    public IntVector2 NeededSpaceSize;
    public int placementNumber;

    public List<SpawningBox> currentTriggerBoxes = new List<SpawningBox>();

    private bool _placementCheck = false;
    private Vector3 _roomBoundaries;

    void Start() {

        //GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;

        _roomBoundaries = FindObjectOfType<Spawner>().GetBoundaries();

        if ((_roomBoundaries.x - 2) <= 0 || (_roomBoundaries.z - 2) <= 0 || (-_roomBoundaries.x + 2) >= 0 || (-_roomBoundaries.z + 2) >= 0) {
            Debug.LogWarning("Room too small for obj: " + gameObject.name + ". Self-Destruction!");
            Destroy(gameObject);
        }
    }

    private void LateUpdate() {

        if (_placementCheck == true)
            return;

        Checker();
    }

    private void Checker() {

        StartPlacement();
        FixApproximates();

        if (!CheckAllBoxes()) {
            _placementCheck = false;
            return;
        }

        Debug.Log("Finally placed at: " + gameObject.name + " " + transform.position);
        //GetComponent<Collider>().isTrigger = false;
        //GetComponent<Rigidbody>().isKinematic = false;
        _placementCheck = true;

    }

    private void StartPlacement() {
        switch (localPlacement) {
            case Placement.Middle:
                PlaceInMidRoom();
                break;
            case Placement.Wall:
                //PlaceNearWall(RoomBoundaries);
                Debug.Log("not made yet");
                break;
            case Placement.NotSet:
                Debug.LogWarning(gameObject.name + ": Placing not set, please check.");
                break;
        }
    }

    private void PlaceInMidRoom() {

        int offset = 2;
        Vector3 V = new Vector3();
        Vector3 tmpBounds = GetComponent<Collider>().bounds.size;
        V.y = tmpBounds.y - tmpBounds.y / 2f;

        switch (localFacing) {
            case Facing.North:
                V.x = UnityEngine.Random.Range(-_roomBoundaries.x + offset, _roomBoundaries.x - offset);
                V.z = UnityEngine.Random.Range(0, _roomBoundaries.z - offset);
                break;
            case Facing.East:
                V.x = UnityEngine.Random.Range(0, _roomBoundaries.x - offset);
                V.z = UnityEngine.Random.Range(-_roomBoundaries.z + offset, _roomBoundaries.z - offset);
                break;
            case Facing.South:
                V.x = UnityEngine.Random.Range(-_roomBoundaries.x + offset, _roomBoundaries.x - offset);
                V.z = UnityEngine.Random.Range(-_roomBoundaries.z + offset, 0);
                break;
            case Facing.West:
                V.x = UnityEngine.Random.Range(-_roomBoundaries.x + offset, 0);
                V.z = UnityEngine.Random.Range(-_roomBoundaries.z + offset, _roomBoundaries.z - offset);
                break;
        }

        transform.position = new Vector3(V.x, V.y, V.z);
        Debug.Log("Trying out position: " + transform.position + " for " + gameObject.name);
    }

    private void FixApproximates() {
        //float gridSize = (_roomBoundaries.x + 1) * 2;
        //float offset = 0.5f;
        //Vector3 V = transform.position;

        //V -= Vector3.one * offset;
        //V /= gridSize;
        //V = new Vector3(Mathf.Round(V.x), Mathf.Round(V.y), Mathf.Round(V.z));
        //V *= gridSize;
        //V += Vector3.one * offset;

        //V.y = transform.position.y;

        //transform.position = V;
        //transform.rotation = Quaternion.identity;

        //if (currentTriggerBoxes.Count > 0) {
        //    Debug.Log(gameObject.name + ": LtW: " + transform.localToWorldMatrix + " WtL: " + transform.worldToLocalMatrix);
        //    Debug.Log(currentTriggerBoxes[0].name + ": LtW: " + currentTriggerBoxes[0].transform.localToWorldMatrix + " WtL: " + currentTriggerBoxes[0].transform.worldToLocalMatrix);
        //}

        // Just align it based on one of the cubes.
        //if (currentTriggerBoxes.Count > 0) {
        //    Debug.Log("applying fix");
        //    Matrix4x4 test = currentTriggerBoxes[0].transform.localToWorldMatrix;
        //    transform.position = new Vector3(test[0, 3], transform.position.y, test[1, 2]);

        //    CheckAllBoxes();
        //}

        Vector3 tmpBounds = GetComponent<Collider>().bounds.size;
        Vector3 currentPos = transform.position;
        transform.position = new Vector3(Mathf.Round(currentPos.x) + 0.5f, currentPos.y, Mathf.Round(currentPos.z) + tmpBounds.z / 2);
    }

    private bool CheckAllBoxes() {

        if (currentTriggerBoxes.Count == 0)
            return false;

        Debug.Log("checking " + currentTriggerBoxes.Count + " boxes in trigger list for: " + gameObject.name);

        foreach (var sbx in currentTriggerBoxes) {
            if (sbx.Father != gameObject.name) {
                Debug.Log(gameObject.name + ": wrong placement, rechecking position.");
                return false;
            }
        }
        return true;
    }

    private void PlaceNearWall(Vector3 RoomBoundaries) {
        throw new NotImplementedException();
    }
}
