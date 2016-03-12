using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
    This script finds empty walls to place wall objects on and aligns the objects accordingly.
    Large paintings are placed in the middle between two available wall spots.
*/
public class SpawnableWallObject : MonoBehaviour {

    public int maxPlacementNum;

    private enum ObjSize { Small, Large, NotSet }
    private ObjSize _eSize = ObjSize.NotSet;
    private bool _placed = false;
    private Renderer _myRenderer;
    private Vector3 _myBounds;
    private SpawnWallObjects _swo;
    private List<SpawnableBox> _possibleSpots = new List<SpawnableBox>();
    private SpawnableBox _usedSpotOne = null, _usedSpotTwo = null;

    void Start() {
        if (SceneManager.GetActiveScene().name != "PCG") {
            _placed = true;
            return;
        }

        _myRenderer = transform.GetComponent<Renderer>();
        _myBounds = _myRenderer.bounds.size;
        //Debug.Log("Size: " + _myBounds + " name: " + gameObject.name);

        if (_myBounds.x > 1)
            _eSize = ObjSize.Large; // two boxes needed
        else
            _eSize = ObjSize.Small; // one box needed
    }

    void Update() {
        if (!_placed) {
            if (_eSize == ObjSize.Small) {
                FindSpot();
            } else if (_eSize == ObjSize.Large) {
                FindLargeSpot();
            } else {
                Debug.LogWarning("aaah? not set???" + gameObject.name);
            }
            _placed = true;
        } else {
            if (transform.position.x == 0 && transform.position.z == 0) {
                Debug.LogWarning(gameObject.name + " was stuck mid room. trying again.");
                if (_usedSpotOne != null) {
                    _usedSpotOne.SetWallObject(null);
                    _swo.AddSpot(_usedSpotOne);
                }
                if (_usedSpotTwo != null) {
                    _usedSpotTwo.SetWallObject(null);
                    _swo.AddSpot(_usedSpotTwo);
                }
                //gameObject.SetActive(false);
                _placed = false;
            }
        }
    }

    private void FindSpot() {

        _swo = FindObjectOfType<SpawnWallObjects>();
        _possibleSpots = _swo.GetPossibleWallSpots();

        SpawnableBox objToUse = _possibleSpots.Where(sbx => sbx.GetWallObject() == null).FirstOrDefault();
        if (objToUse != null) {
            _swo.RemoveSpot(objToUse);
            objToUse.SetWallObject(this);
            AlignOnCorrectWall(objToUse);
        } else {
            //Debug.LogWarning("No space for: " + gameObject.name + " disabling.");
            gameObject.SetActive(false);
        }
    }

    private void AlignOnCorrectWall(SpawnableBox objToUse) {
        SpawnableBox.BoxLocation loc = objToUse.GetBoxLocation();
        switch (loc) {
            case SpawnableBox.BoxLocation.North:
                transform.position = new Vector3(objToUse.transform.position.x, transform.position.y, objToUse.transform.position.z + 0.49f); // +/- the amount to reach the wall surface
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case SpawnableBox.BoxLocation.East:
                transform.position = new Vector3(objToUse.transform.position.x + 0.49f, transform.position.y, objToUse.transform.position.z);
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case SpawnableBox.BoxLocation.South:
                transform.position = new Vector3(objToUse.transform.position.x, transform.position.y, objToUse.transform.position.z - 0.49f);
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;
            case SpawnableBox.BoxLocation.West:
                transform.position = new Vector3(objToUse.transform.position.x - 0.49f, transform.position.y, objToUse.transform.position.z);
                transform.rotation = Quaternion.Euler(0, 270, 0);
                break;
        }
    }

    private void FindLargeSpot() {
        _swo = FindObjectOfType<SpawnWallObjects>();
        _possibleSpots = _swo.GetPossibleWallSpots();

        SpawnableBox objToUse = _possibleSpots.Where(sbx => sbx.GetWallObject() == null).FirstOrDefault();
        if (objToUse != null) {
            PlaceLargeWallObj(objToUse);
        } else {
            objToUse = _possibleSpots.Where(sbx => sbx.GetWallObject() == null).LastOrDefault();
            if (objToUse != null) {
                PlaceLargeWallObj(objToUse);
            } else {
               // Debug.LogWarning("2: No space for: " + gameObject.name + " disabling.");
                gameObject.SetActive(false);
            }
        }
    }

    private void PlaceLargeWallObj(SpawnableBox objToUse) {
        SpawnableBox extraSpotToUse = FindCollidingFreeBoxes(objToUse);
        //Debug.Log(extraSpotToUse.name + " compared with: " + objToUse.name);
        if (extraSpotToUse != objToUse) {
            _swo.RemoveSpot(extraSpotToUse);
            _usedSpotTwo = extraSpotToUse;
            extraSpotToUse.SetWallObject(this);
            _swo.RemoveSpot(objToUse);
            _usedSpotOne = objToUse;
            objToUse.SetWallObject(this);
            AlignOnCorrectWall(objToUse);
            CorrectPositionBasedOnNewSpot(objToUse, extraSpotToUse);
        } else {
            //Debug.LogWarning("1: No space for: " + gameObject.name + " disabling.");
            gameObject.SetActive(false);
        }
    }

    private void CorrectPositionBasedOnNewSpot(SpawnableBox objToUse, SpawnableBox extraSpotToUse) {
        Vector3 placedPos = objToUse.transform.position;
        Vector3 targetPos = extraSpotToUse.transform.position;
        float offset = 0.5f;
        SpawnableBox.BoxLocation loc = objToUse.GetBoxLocation();
        switch (loc) {
            case SpawnableBox.BoxLocation.North:
                //Debug.Log("North: " + gameObject.name + " initial pos: " + transform.position);
                if (targetPos.x > placedPos.x)
                    transform.position = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
                else if (targetPos.x < placedPos.x)
                    transform.position = new Vector3(transform.position.x - offset, transform.position.y, transform.position.z);
                break;
            case SpawnableBox.BoxLocation.East:
                //Debug.Log("East: " + gameObject.name + " initial pos: " + transform.position);
                if (targetPos.z > placedPos.z)
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + offset);
                else if (targetPos.z < placedPos.z)
                    transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - offset);
                break;
            case SpawnableBox.BoxLocation.South:
                //Debug.Log("South: " + gameObject.name + " initial pos: " + transform.position);
                if (targetPos.x > placedPos.x)
                    transform.position = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
                else if (targetPos.x < placedPos.x)
                    transform.position = new Vector3(transform.position.x - offset, transform.position.y, transform.position.z);
                break;
            case SpawnableBox.BoxLocation.West:
                Debug.LogWarning("West wall too occupied for large painting. Error in correcting placement for: " + gameObject.name);
                break;
            default:
                Debug.LogWarning("Error in correcting placement for: " + gameObject.name);
                break;
        }
    }

    private SpawnableBox FindCollidingFreeBoxes(SpawnableBox box) {
        // First check all colliding spawnedBoxes for neighbors
        Collider[] _colliders = Physics.OverlapSphere(box.transform.position, box.GetComponent<Collider>().bounds.size.x);
        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.transform.position = box.transform.position;
        //sphere.transform.localScale = new Vector3(box.GetComponent<Collider>().bounds.size.x, box.GetComponent<Collider>().bounds.size.y, box.GetComponent<Collider>().bounds.size.z);
        // If we found some
        if (_colliders.Length > 2) {
            foreach (var obj in _colliders) {
                SpawnableBox sbx = obj.GetComponent<SpawnableBox>();
                if (sbx) {
                    //Debug.Log(box.name + " collides with: " + sbx.name);
                    if (sbx.name != box.name && sbx.GetBoxLocation() == box.GetBoxLocation() && sbx.GetWallObject() == null
                            && sbx.GetBoxCondition() != SpawnableBox.BoxCondition.Occupied && sbx.GetBoxCondition() != SpawnableBox.BoxCondition.Tall) {
                        //Debug.Log(box.name + " found possible placement: " + sbx.name);
                        return sbx; // return 1 spots only everytime
                    }
                }
            }
        }

        return box;
    }

    public bool GetPlacementCheck() {
        return _placed;
    }
}
