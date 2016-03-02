using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnableObject : MonoBehaviour {

    public enum Tag {
        Short,
        Tall,
        Carpet
    }

    public enum Placement {
        NotSet,
        Middle,
        Wall
    }

    public enum Facing {
        North,
        East,
        South
    }

    public Tag localTag = Tag.Short;
    public Placement localPlacement = Placement.NotSet;
    public int maxPlacementNum = 1;

    private bool _placementCheck = false;
    private int _timesCounter = 0,
        _secondCounter = 0;
    private Facing _localFacing = Facing.North;
    private Renderer _myRenderer;
    private SpawnableBox[] _allBoxes;
    private Vector3 _roomBoundaries,
        _myBounds,
        _checkVector;
    private List<SpawnableBox> _currentTriggerBoxes = new List<SpawnableBox>();
    private static System.Random _rndO = new System.Random(); // so that all objects share the same

    public bool GetPlacementCheck() {
        return _placementCheck;
    }

    private void ChangeFacing() {
        switch (_localFacing) {
            case Facing.North:
                _localFacing = Facing.East;
                break;
            case Facing.East:
                _localFacing = Facing.South;
                break;
            case Facing.South:
                _localFacing = Facing.North;
                break;
        }
        _timesCounter = 0;
        _secondCounter++;
        //Debug.Log("Changed facing for: " + gameObject.name + " to " + localFacing);
    }

    private void Start() {

        if (SceneManager.GetActiveScene().name != "ScriptTester") {
            _placementCheck = true;
            return;
        }

        //GetComponent<Collider>().isTrigger = true;
        //GetComponent<Rigidbody>().isKinematic = true;

        _roomBoundaries = FindObjectOfType<Spawner>().GetBoundaries();
        _myRenderer = transform.GetComponent<Renderer>();
        _myBounds = _myRenderer.bounds.size;
        _checkVector = new Vector3(0, 0, 0);

        if ((_roomBoundaries.x - 2) <= 0 || (_roomBoundaries.z - 2) <= 0 || (-_roomBoundaries.x + 2) >= 0 || (-_roomBoundaries.z + 2) >= 0) {
            Debug.LogWarning("Room too small for obj: " + gameObject.name + ". Self-Destruction!");
            Destroy(gameObject);
        }

        if (localPlacement == Placement.NotSet) {
            Debug.LogWarning("Missing Placement, destroying: " + gameObject.name);
            Destroy(gameObject);
        }

        if (maxPlacementNum == 0) {
            Debug.LogWarning("Missing Placement number, destroying: " + gameObject.name);
            Destroy(gameObject);
        }

        _allBoxes = FindObjectsOfType<SpawnableBox>();

        // Get a random starting facing for wall objects
        if (localPlacement == Placement.Wall) {
            Array values = Enum.GetValues(typeof(Facing));
            _localFacing = (Facing)values.GetValue(_rndO.Next(values.Length));
        }
    }

    private void ArrayShuffle(SpawnableBox[] sbxs) {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < sbxs.Length; t++) {
            SpawnableBox tmp = sbxs[t];
            int r = UnityEngine.Random.Range(t, sbxs.Length);
            sbxs[t] = sbxs[r];
            sbxs[r] = tmp;
        }
    }

    private void ListShuffle(List<int> myList) {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < myList.Count; t++) {
            int tmp = myList[t];
            int r = UnityEngine.Random.Range(t, myList.Count);
            myList[t] = myList[r];
            myList[r] = tmp;
        }
    }

    private void LateUpdate() {

        if (_placementCheck == true)
            return;

        // to counter the worst cast where no box is found and the object is placed at 0,0,0
        if (transform.position == _checkVector) {
            //Debug.LogWarning("Could not find box, restart for: " + gameObject.name);
            _placementCheck = false;
            // Now you can try placing again
            Place();
        }

        if (_secondCounter > 5) {
            Debug.LogWarning("No space for " + gameObject.name + " disabling.");

            // First empty the boxes
            foreach (SpawnableBox sbx in _currentTriggerBoxes) {
                Resetter(sbx);
            }
            _placementCheck = true; // used to determine when we are done with each piece of furniture
            gameObject.SetActive(false);
        } else {
            // Now you can place
            Place();
        }
    }

    private void Place() {
        // Now you can place
        if (localTag == Tag.Carpet) {
            FindLocationToPlaceCarpet();
        } else if (localTag == Tag.Short || localTag == Tag.Tall) {
            FindLocationToPlaceFurniture();
        }
    }

    private void FindLocationToPlaceCarpet() {

        if (_timesCounter > 4)
            ChangeFacing();

        ArrayShuffle(_allBoxes);
        PlaceInMidRoomVersionTwo(GetMidRoomBox());
        FindAllCollidingBoxes();

        if (!CheckAllBoxes()) {
            _timesCounter++;
            _placementCheck = false;
            return;
        }

        _placementCheck = true;
    }

    private void FindLocationToPlaceFurniture() {

        if (_timesCounter > 6)
            ChangeFacing();

        ArrayShuffle(_allBoxes);
        StartPlacement();
        FindAllCollidingBoxes();
        WallCorrections();

        if (!CheckAllBoxes()) {
            _timesCounter++;
            _placementCheck = false;
            return;
        }

        _placementCheck = true;
        if (gameObject.name.Contains("tableDinner") || gameObject.name.Contains("DeskWithDrawers")) {
            HoldSpaceForChairs();
        }

        //GetComponent<Collider>().isTrigger = false;
        //GetComponent<Rigidbody>().isKinematic = false;   
        //Debug.Log("Finally placed at: " + gameObject.name + " " + transform.position);    
    }

    private void HoldSpaceForChairs() {
        // First check all colliding spawnedBoxes - a bit larger this time though
        Collider[] _colliders = Physics.OverlapSphere(transform.position, 1.0f);

        foreach (var obj in _colliders) {
            SpawnableBox sbx = obj.GetComponent<SpawnableBox>();
            if (sbx)
                if (sbx.transform.position.x != gameObject.transform.position.x && sbx.transform.position.z != gameObject.transform.position.z) {
                    if (sbx.GetFurniture() == gameObject || sbx.GetFurniture() == null) {
                        _currentTriggerBoxes.Add(sbx);
                        sbx.HoldForChair(this);
                    }
                }
        }
    }

    private void WallCorrections() {

        if (localPlacement == Placement.Wall) {
            foreach (SpawnableBox sbx in _currentTriggerBoxes) {
                if (sbx.name == "Spawned Box 0, 0") // South
                    transform.position = new Vector3(transform.position.x + 0.07f, transform.position.y, transform.position.z);
                else if (sbx.name == "Spawned Box 7, 0" && !gameObject.name.Contains("fireplace"))
                    transform.position = new Vector3(transform.position.x - 0.07f, transform.position.y, transform.position.z);
                else if (sbx.name == "Spawned Box 0, 7" && !gameObject.name.Contains("fireplace")) // North
                    transform.position = new Vector3(transform.position.x + 0.07f, transform.position.y, transform.position.z);
                else if (sbx.name == "Spawned Box 7, 7")
                    transform.position = new Vector3(transform.position.x - 0.07f, transform.position.y, transform.position.z);
            }

            // Need to recheck the boxes now
            FindAllCollidingBoxes();
        }
    }

    private void FindAllCollidingBoxes() {

        // First check all colliding spawnedBoxes
        Vector3 _center = _myRenderer.bounds.center;
        Vector3 _halfSize = _myRenderer.bounds.size / 2;
        Collider[] _colliders = Physics.OverlapBox(_center, _halfSize - _halfSize * 0.05f);

        List<SpawnableBox> comparingList = new List<SpawnableBox>();

        foreach (var obj in _colliders) {
            SpawnableBox sbx = obj.GetComponent<SpawnableBox>();
            if (sbx) {
                _currentTriggerBoxes.Add(sbx);
                comparingList.Add(sbx);

                if (localTag == Tag.Carpet)
                    sbx.SetCarpet(this);
                else if (localTag == Tag.Tall || localTag == Tag.Short)
                    sbx.SetFurniture(this);
            }
        }

        // Then double check and compare when to remove, for everytime the object gets moved
        List<SpawnableBox> removingList = new List<SpawnableBox>();
        foreach (SpawnableBox sbx in _currentTriggerBoxes) {
            if (!comparingList.Contains(sbx)) {
                removingList.Add(sbx);
            }
        }

        foreach (SpawnableBox sbx in removingList) {
            _currentTriggerBoxes.Remove(sbx);
            Resetter(sbx);
        }
    }

    private void Resetter(SpawnableBox sbx) {
        if (sbx.GetFurniture() == gameObject)
            sbx.ReSetFurniture();
        else if (sbx.GetCarpet() == gameObject)
            sbx.ReSetCarpet();
    }

    private void StartPlacement() {
        switch (localPlacement) {
            case Placement.Middle:
                PlaceInMidRoomVersionTwo(GetMidRoomBox());
                break;
            case Placement.Wall:
                PlaceNearWallVersionTwo();
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

    private SpawnableBox GetMidRoomBox() {
        // Get a random box for placement 
        // We don't care anymore for mid room facing
        SpawnableBox objToUse = null;

        if (localTag == Tag.Carpet) {
            objToUse = _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.Middle).FirstOrDefault();
            if (objToUse == null)
                _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.Middle).LastOrDefault();
        } else {
            objToUse = _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.Middle).FirstOrDefault(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.Free);
            if (objToUse == null)
                _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.Middle).LastOrDefault(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.Free);
        }
        return objToUse;
    }

    private void PlaceInMidRoomVersionTwo(SpawnableBox objToUse) {

        if (objToUse != null) {
            Vector3 V = new Vector3();
            V.x = objToUse.transform.position.x;
            //V.y = _myBounds.y - _myBounds.y / 2f;
            V.y = transform.position.y;
            V.z = objToUse.transform.position.z;

            transform.position = V;

            // Randomize y rotation
            // Giving an extra 270 for leverage
            List<int> myValues = new List<int>(new int[] { 0, 90, 180, 270, 270 });
            ListShuffle(myValues);
            IEnumerable<int> oneRandom = myValues.OrderBy(x => _rndO.Next()).Take(1);
            transform.rotation = Quaternion.Euler(0, oneRandom.First(), 0);

            // Allign on Spawning boxes based on bounds and rotation
            Vector3 targetPos = transform.position;
            if (transform.rotation.eulerAngles.y == 0) {
                transform.position = new Vector3(targetPos.x + _myBounds.x, targetPos.y, targetPos.z);
            } else if (transform.rotation.eulerAngles.y == 90) {
                transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z - _myBounds.z);
            } else if (transform.rotation.eulerAngles.y == 180) {
                transform.position = new Vector3(targetPos.x - _myBounds.x, targetPos.y, targetPos.z);
            } else if (transform.rotation.eulerAngles.y == 270) {
                transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z + _myBounds.z);
            }
        }
    }

    /*
    private void PlaceNearWall() {
        int offset = 2;
        Vector3 V = new Vector3();
        //V.y = _myBounds.y - _myBounds.y / 2f;
        V.y = transform.position.y;

        // Get a random value for placement based on facing
        switch (_localFacing) {
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
    */

    private void PlaceNearWallVersionTwo() {
        // Get a random box for placement based on facing
        SpawnableBox objToUse = null;
        switch (_localFacing) {
            case Facing.North:
                objToUse = _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.North).FirstOrDefault(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.Free);
                if (objToUse == null)
                    _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.North).LastOrDefault(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.Free);
                break;
            case Facing.East:
                objToUse = _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.East).FirstOrDefault(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.Free);
                if (objToUse == null)
                    _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.East).LastOrDefault(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.Free);
                break;
            case Facing.South:
                objToUse = _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.South).FirstOrDefault(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.Free);
                if (objToUse == null)
                    _allBoxes.Where(sbx => sbx.GetBoxLocation() == SpawnableBox.BoxLocation.South).LastOrDefault(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.Free);
                break;
        }

        if (objToUse != null) {
            Vector3 V = new Vector3();
            V.x = objToUse.transform.position.x;
            //V.y = _myBounds.y - _myBounds.y / 2f;
            V.y = transform.position.y;
            V.z = objToUse.transform.position.z;

            transform.position = V;
            //Debug.Log("Trying out position: " + transform.position + " for " + gameObject.name);

            Vector3 targetPos = transform.position;

            // Placement corrections
            switch (_localFacing) {
                case Facing.North:
                    // Align to boxes
                    transform.position = new Vector3(Mathf.Round(targetPos.x) - _myBounds.x / 2, targetPos.y, Mathf.Round(targetPos.z) - _myBounds.z / 2);
                    // Fix due to wall indent
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.07f);
                    // Correct facing
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    break;
                case Facing.East:
                    // Align to boxes
                    transform.position = new Vector3(Mathf.Round(targetPos.x) - _myBounds.x / 5, targetPos.y, Mathf.Round(targetPos.z));
                    // Correct facing
                    transform.rotation = Quaternion.Euler(0, 270, 0);
                    // Need to move the Dresser a bit
                    if (gameObject.name.Contains("Dresser")) {
                        transform.position = new Vector3(transform.position.x - _myBounds.x / 5, transform.position.y, transform.position.z);
                    }
                    break;
                case Facing.South:
                    // Align to boxes
                    transform.position = new Vector3(Mathf.Round(targetPos.x) + _myBounds.x / 2, targetPos.y, Mathf.Round(targetPos.z) + _myBounds.z / 2);
                    // Fix due to wall indent
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.07f);
                    // Correct facing
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
            }
        }
    }

    private bool CheckAllBoxes() {

        if (_currentTriggerBoxes.Count == 0)
            return false;

        //Debug.Log("Checking " + currentTriggerBoxes.Count + " boxes in trigger list for: " + gameObject.name);

        foreach (var sbx in _currentTriggerBoxes) {

            if (localTag == Tag.Carpet) {
                if (sbx.GetCarpet() != gameObject) {
                    //Debug.Log(gameObject.name + ": wrong placement, recheck fixed position box: " + sbx.name);
                    return false;
                }
            } else if (localTag == Tag.Short || localTag == Tag.Tall) {
                if (sbx.GetFurniture() != gameObject) {
                    //Debug.Log(gameObject.name + ": wrong placement, need to recheck fixed position: " + transform.position);
                    return false;
                }
            }
            // Check if we are using a different type of box
            if (localPlacement == Placement.Middle) {
                if (sbx.GetBoxLocation() != SpawnableBox.BoxLocation.Middle)
                    return false;
            } else if (localPlacement == Placement.Wall)
                if (sbx.GetBoxLocation() == SpawnableBox.BoxLocation.Middle)
                    return false;
        }
        //Debug.Log("Accepted position: " + transform.position + " for: " + gameObject.name);
        return true;
    }
}