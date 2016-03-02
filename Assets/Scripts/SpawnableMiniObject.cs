using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnableMiniObject : MonoBehaviour {

    public int maxPlacementNum = 2;

    private bool _placed = true; //false;
    //private Renderer _myRenderer;
    //private Vector3 _myBounds;
    //private static System.Random _rndMO = new System.Random(); // so that all objects share the same

    //void Start() {
    //    if (SceneManager.GetActiveScene().name != "ScriptTester") {
    //        _placed = true;
    //        return;
    //    }

    //    _myRenderer = transform.GetComponent<Renderer>();
    //    if (_myRenderer != null)
    //        _myBounds = _myRenderer.bounds.size;
    //    else
    //        _myBounds = gameObject.GetComponent<Collider>().bounds.size;
    //    //Debug.Log("Size: " + _myBounds + " name: " + gameObject.name);
    //}

    //void LateUpdate() {
    //    if (!_placed) {
    //        FindSpot();
    //        _placed = true;
    //    }
    //}

    //private void FindSpot() {

    //    GetPlacedOnParentRandomly();

    //    if (!CheckForCollidingMiniObjs()) {
    //        Debug.LogWarning("No space for: " + gameObject.name + " disabling.");
    //        gameObject.SetActive(false);
    //    }
    //}

    //private void GetPlacedOnParentRandomly() {
    //    float xMin = transform.parent.GetComponent<Renderer>().bounds.min.x;
    //    float xMax = transform.parent.GetComponent<Renderer>().bounds.max.x;
    //    float y = transform.parent.GetComponent<Renderer>().bounds.max.y;
    //    float zMin = transform.parent.GetComponent<Renderer>().bounds.min.z;
    //    float zMax = transform.parent.GetComponent<Renderer>().bounds.max.z;

    //    transform.position = new Vector3(Random.Range(xMin, xMax), y, Random.Range(zMin, zMax));
    //    //Debug.Log(gameObject.name + " " + transform.position);

    //    // Randomize y rotation
    //    Debug.Log("parent fwd: " + transform.parent.forward);
    //    transform.rotation = Quaternion.Euler(0, transform.parent.transform.rotation.y, 0);
    //}

    //private bool CheckForCollidingMiniObjs() {
    //    // First check all colliding spawnedBoxes for neighbors
    //    Collider[] _colliders = Physics.OverlapSphere(transform.position, (_myBounds.x / 2) * 0.5f);
    //    // If we found some
    //    foreach (var obj in _colliders) {
    //        SpawnableMiniObject smo = obj.GetComponent<SpawnableMiniObject>();
    //        if (smo) {
    //            if (smo.gameObject != gameObject) {
    //                //Debug.Log(gameObject.name + " collides with: " + smo.name + " disabling.");
    //                return false; // return 2 spots only everytime
    //            }
    //        }
    //    }
    //    return true;
    //}

    public bool GetPlacementCheck() {
        return _placed;
    }
}
