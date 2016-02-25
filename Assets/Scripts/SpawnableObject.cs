using System;
using System.Collections.Generic;
using System.Linq;
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

    private List<SpawningBox> _currentTriggerBoxes = new List<SpawningBox>();
    private SpawningBox[] _allBoxes;
    private int _timesCounter = 0, _secondCounter = 0;
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
        _secondCounter++;
        //Debug.Log("Changed facing for: " + gameObject.name + " to " + localFacing);
    }

    void Start() {

        //GetComponent<Collider>().isTrigger = true;
        //GetComponent<Rigidbody>().isKinematic = true;

        _myBounds = GetComponent<Collider>().bounds.size;
        _roomBoundaries = FindObjectOfType<Spawner>().GetBoundaries();
        _myRenderer = transform.GetComponent<Renderer>();

        if ((_roomBoundaries.x - 2) <= 0 || (_roomBoundaries.z - 2) <= 0 || (-_roomBoundaries.x + 2) >= 0 || (-_roomBoundaries.z + 2) >= 0) {
            Debug.LogWarning("Room too small for obj: " + gameObject.name + ". Self-Destruction!");
            Destroy(gameObject);
        }

        if (localPlacement == Placement.NotSet) {
            Debug.LogWarning("Missing Placement, destroying: " + gameObject.name);
            Destroy(gameObject);
        }

        if (placementNumber == 0) {
            Debug.LogWarning("Missing Placement number, destroying: " + gameObject.name);
            Destroy(gameObject);
        }

        _allBoxes = FindObjectsOfType<SpawningBox>();
    }

    private void LateUpdate() {

        if (_placementCheck == true)
            return;

        if (_secondCounter > 5) {
            Debug.LogWarning("No space for " + gameObject.name + " disabling.");

            // First empty the boxes
            foreach (SpawningBox sbx in _currentTriggerBoxes) {
                if (sbx.GetFather() == gameObject)
                    sbx.ReSetColBox();
            }
            _placementCheck = true;
            gameObject.SetActive(false);
        } else {
            Checker();
        }
    }

    private void Checker() {

        if (_timesCounter > 4) {
            ChangeFacing();
        }

        StartPlacement();
        CorrectPlacement();
        FindCollidingSpawnedBoxes();

        if (!CheckAllBoxes()) {
            _timesCounter++;
            _placementCheck = false;
            return;
        }

        //Debug.Log("Finally placed at: " + gameObject.name + " " + transform.position);
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
                _currentTriggerBoxes.Add(sbx);
                comparingList.Add(sbx);
                sbx.SetColBox(this);
            }
        }

        // Then double check and compare when to remove, for everytime the object gets moved
        List<SpawningBox> removingList = new List<SpawningBox>();
        foreach (SpawningBox sbx in _currentTriggerBoxes) {
            if (!comparingList.Contains(sbx)) {
                removingList.Add(sbx);
            }
        }

        foreach (SpawningBox sbx in removingList) {
            _currentTriggerBoxes.Remove(sbx);
            if (sbx.GetFather() == gameObject)
                sbx.ReSetColBox();
        }
    }

    private void StartPlacement() {
        switch (localPlacement) {
            case Placement.Middle:
                //PlaceInMidRoom();
                PlaceInMidRoomVersionTwo();
                break;
            case Placement.Wall:
                PlaceNearWall();
                break;
            case Placement.NotSet:
                Debug.LogWarning(gameObject.name + ": Placing not set, please check.");
                break;
        }
    }

    /*
    private void PlaceInMidRoom() {
        int offset = 2;
        Vector3 V = new Vector3();
        //V.y = _myBounds.y - _myBounds.y / 2f;
        V.y = transform.position.y;

        // Get a random value for placement based on facing
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
        //Debug.Log("Trying out position: " + transform.position + " for " + gameObject.name);

        // Randomize y rotation
        System.Random r = new System.Random();
        List<int> myValues = new List<int>(new int[] { 0, 90, 180, 270 });
        IEnumerable<int> oneRandom = myValues.OrderBy(x => r.Next()).Take(1);
        transform.rotation = Quaternion.Euler(0, oneRandom.First(), 0);
    }
    */

    private void PlaceInMidRoomVersionTwo() {
        // Get a random box for placement based on facing
        SpawningBox objToUse = null;
        switch (localFacing) {
            case Facing.North:
                objToUse = _allBoxes.Where(
                    sbx => sbx.GetBoxLocation() == SpawningBox.BoxLocation.Middle && sbx.gameObject.transform.position.z > 0
                    ).First(sbx => sbx.GetBoxCondition() == SpawningBox.BoxCondition.Free);
                break;
            case Facing.East:
                objToUse = _allBoxes.Where(
                    sbx => sbx.GetBoxLocation() == SpawningBox.BoxLocation.Middle && sbx.gameObject.transform.position.x > 0
                    ).First(sbx => sbx.GetBoxCondition() == SpawningBox.BoxCondition.Free);
                break;
            case Facing.South:
                objToUse = _allBoxes.Where(
                    sbx => sbx.GetBoxLocation() == SpawningBox.BoxLocation.Middle && sbx.gameObject.transform.position.z <= 0
                    ).First(sbx => sbx.GetBoxCondition() == SpawningBox.BoxCondition.Free);
                break;
            case Facing.West:
                objToUse = _allBoxes.Where(
                     sbx => sbx.GetBoxLocation() == SpawningBox.BoxLocation.Middle && sbx.gameObject.transform.position.x <= 0
                     ).First(sbx => sbx.GetBoxCondition() == SpawningBox.BoxCondition.Free);
                break;
        }

        Debug.Log(gameObject.name + " found: " + objToUse.name);

        Vector3 V = new Vector3();
        V.x = objToUse.transform.position.x;
        //V.y = _myBounds.y - _myBounds.y / 2f;
        V.y = transform.position.y;
        V.z = objToUse.transform.position.z;

        transform.position = new Vector3(V.x, V.y, V.z);
        //Debug.Log("Trying out position: " + transform.position + " for " + gameObject.name);

        // Randomize y rotation
        System.Random r = new System.Random();
        List<int> myValues = new List<int>(new int[] { 0, 90, 180, 270, 270 });
        IEnumerable<int> oneRandom = myValues.OrderBy(x => r.Next()).Take(1);
        transform.rotation = Quaternion.Euler(0, oneRandom.First(), 0);
    }


    private void PlaceNearWall() {
        int offset = 2;
        Vector3 V = new Vector3();
        //V.y = _myBounds.y - _myBounds.y / 2f;
        V.y = transform.position.y;

        // Get a random value for placement based on facing
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
        //Debug.Log("Trying out position: " + transform.position + " for " + gameObject.name);
    }

    private void CorrectPlacement() {

        // Allign on Spawning boxes based on bounds
        Vector3 targetPos = transform.position;
        transform.position = new Vector3(Mathf.Round(targetPos.x) + _myBounds.x / 2, targetPos.y, Mathf.Round(targetPos.z) + _myBounds.z / 2);

        // Allign on wall
        if (localPlacement == Placement.Wall) {
            switch (localFacing) {
                case Facing.North:
                    // Increase z by bounds <and> y rotation 180
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + _myBounds.z);
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case Facing.East:
                    // Decrease x by bounds/10, z by bounds/2 <and> y rotation 270
                    transform.position = new Vector3(transform.position.x - _myBounds.x / 10, transform.position.y, transform.position.z - _myBounds.z / 2);
                    transform.rotation = Quaternion.Euler(0, 270, 0);
                    break;
                case Facing.South:
                    // Decrease z by 1 SpawningBox <and> y rotation 0
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Facing.West:
                    // position.x = x - 2 + bounds/9, z by bounds/2 <and> y rotation 90
                    float x = (transform.position.x - 2) + (_myBounds.x / 9);
                    transform.position = new Vector3(x, transform.position.y, transform.position.z - _myBounds.z / 2);
                    transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                default:
                    break;
            }
        }
    }

    private bool CheckAllBoxes() {

        if (_currentTriggerBoxes.Count == 0)
            return false;

        //Debug.Log("Checking " + currentTriggerBoxes.Count + " boxes in trigger list for: " + gameObject.name);

        foreach (var sbx in _currentTriggerBoxes) {
            if (sbx.GetFather() != gameObject) {
                //Debug.Log(gameObject.name + ": wrong placement, need to recheck fixed position: " + transform.position);
                return false;
            }
            if (localPlacement == Placement.Middle)
                if (sbx.GetBoxLocation() != SpawningBox.BoxLocation.Middle)
                    return false;
        }
        //Debug.Log("Accepted position: " + transform.position + " for: " + gameObject.name);
        return true;
    }
}