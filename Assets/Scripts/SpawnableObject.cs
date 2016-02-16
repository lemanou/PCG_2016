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

    public bool CustomDebug = true;
    public Tag localTag = Tag.Short;
    public Placement localPlacement = Placement.NotSet;
    public Facing localFacing = Facing.North;

    public IntVector2 NeededSpaceSize;
    public int placementNumber;

    public List<SpawningBox> currentTriggerBoxes = new List<SpawningBox>();

    private int _timesCounter = 0;
    private bool _placementCheck = false;
    private Vector3 _roomBoundaries, _myBounds;
    private Renderer _myRenderer;

    public bool GetPlacementCheck() {
        return _placementCheck;
    }

    private void ChangeFacing() {
        switch (localFacing) {
            case Facing.North:
                localFacing = Facing.East;
                break;
            case Facing.East:
                localFacing = Facing.South;
                break;
            case Facing.South:
                localFacing = Facing.West;
                break;
            case Facing.West:
                localFacing = Facing.North;
                break;
        }
        _timesCounter = 0;
        Debug.Log("Changed facing for: " + gameObject.name);
    }

    void Start() {

        //GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;

        _myBounds = GetComponent<Collider>().bounds.size;
        _roomBoundaries = FindObjectOfType<Spawner>().GetBoundaries();
        _myRenderer = transform.GetComponent<Renderer>();

        if ((_roomBoundaries.x - 2) <= 0 || (_roomBoundaries.z - 2) <= 0 || (-_roomBoundaries.x + 2) >= 0 || (-_roomBoundaries.z + 2) >= 0) {
            if (CustomDebug)
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

        if (_timesCounter > 5) {
            ChangeFacing();
        }

        StartPlacement();
        FixApproximates();
        FindCollidingSpawnedBoxes();

        if (!CheckAllBoxes()) {
            _timesCounter++;
            _placementCheck = false;
            return;
        }

        if (CustomDebug)
            Debug.Log("Finally placed at: " + gameObject.name + " " + transform.position);
        _placementCheck = true;
        //GetComponent<Collider>().isTrigger = false;
        //GetComponent<Rigidbody>().isKinematic = false;       
    }

    private void FindCollidingSpawnedBoxes() {

        // First check all colliding spawnedBoxes
        Vector3 _center = _myRenderer.bounds.center;
        Vector3 _halfSize = _myRenderer.bounds.size / 2;
        Collider[] _colliders = Physics.OverlapBox(_center, _halfSize - _halfSize * 0.05f);
        List<SpawningBox> comparingList = new List<SpawningBox>();

        foreach (var obj in _colliders) {
            SpawningBox sbx = obj.GetComponent<SpawningBox>();
            if (sbx) {
                currentTriggerBoxes.Add(sbx);
                comparingList.Add(sbx);
                sbx.SetColBox(this);
            }
        }

        // Then double check and compare when to remove
        List<SpawningBox> removingList = new List<SpawningBox>();
        foreach (SpawningBox sbx in currentTriggerBoxes) {
            if (!comparingList.Contains(sbx)) {
                removingList.Add(sbx);
            }
        }

        foreach (SpawningBox sbx in removingList) {
            currentTriggerBoxes.Remove(sbx);
            if (sbx.Father == gameObject)
                sbx.ReSetColBox();
        }
    }

    private void StartPlacement() {
        switch (localPlacement) {
            case Placement.Middle:
                PlaceInMidRoom();
                break;
            case Placement.Wall:
                PlaceNearWall();
                break;
            case Placement.NotSet:
                Debug.LogWarning(gameObject.name + ": Placing not set, please check.");
                break;
        }
    }

    private void PlaceInMidRoom() {
        int offset = 2;
        Vector3 V = new Vector3();
        V.y = _myBounds.y - _myBounds.y / 2f;

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
        if (CustomDebug)
            Debug.Log("Trying out position: " + transform.position + " for " + gameObject.name);
    }

    private void PlaceNearWall() {
        int offset = 2;
        Vector3 V = new Vector3();
        V.y = _myBounds.y - _myBounds.y / 2f;

        switch (localFacing) {
            case Facing.North:
                V.x = UnityEngine.Random.Range(-_roomBoundaries.x + offset, _roomBoundaries.x - offset);
                V.z = _roomBoundaries.z;
                break;
            case Facing.East:
                V.x = _roomBoundaries.x;
                V.z = UnityEngine.Random.Range(-_roomBoundaries.z + offset, _roomBoundaries.z - offset);
                break;
            case Facing.South:
                V.x = UnityEngine.Random.Range(-_roomBoundaries.x + offset, _roomBoundaries.x - offset);
                V.z = -_roomBoundaries.z;
                break;
            case Facing.West:
                V.x = -_roomBoundaries.x;
                V.z = UnityEngine.Random.Range(-_roomBoundaries.z + offset, _roomBoundaries.z - offset);
                break;
        }

        transform.position = new Vector3(V.x, V.y, V.z);
        if (CustomDebug)
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

        Vector3 targetPos = transform.position;
        transform.position = new Vector3(Mathf.Round(targetPos.x) + 0.5f, targetPos.y, Mathf.Round(targetPos.z) + _myBounds.z / 2);
    }

    private bool CheckAllBoxes() {

        if (currentTriggerBoxes.Count == 0)
            return false;

        if (CustomDebug)
            Debug.Log("Checking " + currentTriggerBoxes.Count + " boxes in trigger list for: " + gameObject.name);

        foreach (var sbx in currentTriggerBoxes) {
            if (sbx.Father != gameObject) {
                if (CustomDebug)
                    Debug.Log(gameObject.name + ": wrong placement, need to recheck fixed position: " + transform.position);
                return false;
            }
        }
        if (CustomDebug)
            Debug.Log("Accepted position: " + transform.position + " for: " + gameObject.name);
        return true;
    }
}