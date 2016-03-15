using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
/*
    This script places mini-objects on shelfsand tabletops.
    We instantiate mini-objects and place them within the bounds of their parent furniture.
    Special rules apply when placing mini-objects inside the armoire and when placing certain mini-objects.
    We instantiate mini-objects on shelves and parent mini-objects to their furniture.
    On Reset() we destroy all mini-objects and stop all coroutines.
*/
public class SpawnObjectsOnMe : MonoBehaviour {

    //[Range(1, 10)]
    private int _totalAmountOfMiniObjects = 1;

    public bool parentOfParent = false;
    public float generationStepDelay;
    public List<SpawnableMiniObject> MiniObjectsToPlace;

    private bool _placed = false;
    private int _placedMiniObjsCount = 0;
    private SpawnableBox[] _allBoxes;
    //private List<SpawnableBox> _possibleSpots = new List<SpawnableBox>();
    private List<SpawnableMiniObject> _placedMiniObjects = new List<SpawnableMiniObject>();
    private Dictionary<int, SpawnableMiniObject> _fullMiniObjsDict = new Dictionary<int, SpawnableMiniObject>();
    private static System.Random randM = new System.Random();
    private SpawnableObject _papa;

    private void CreateFullDict() {
        int countKey = 0;
        foreach (var sbx in MiniObjectsToPlace) {
            int x = sbx.maxPlacementNum;
            while (x > 0) {
                x--;
                countKey++;
                _fullMiniObjsDict.Add(countKey, sbx);
            }
        }
    }

    private IEnumerator MiniObjectSpawning() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        foreach (var item in _placedMiniObjects) {
            if (!item.GetPlacementCheck()) {
                Debug.Log("Skipping one turn. Waiting to place: " + item.name);
                yield return delay;
            }
        }

        while (_placedMiniObjsCount < _totalAmountOfMiniObjects) {

            _placedMiniObjsCount++;

            if (_papa.gameObject.name.Contains("armoire"))
                PlaceInArmoire();
            else
                PlaceDifferent();

            yield return delay;
        }
    }

    private void PlaceInArmoire() {
        // Get random key from Dictionary
        int newObjKey = _fullMiniObjsDict.ElementAt(randM.Next(0, _fullMiniObjsDict.Count)).Key;

        SpawnableMiniObject newSObj = Instantiate(_fullMiniObjsDict[newObjKey], transform.position, transform.rotation) as SpawnableMiniObject;
        newSObj.name += ": " + _placedMiniObjsCount;
        newSObj.transform.SetParent(gameObject.transform);

        _placedMiniObjects.Add(newSObj);
        // thus we cannot again
        _fullMiniObjsDict.Remove(newObjKey);
    }

    private void PlaceDifferent() {
        // Get random key from Dictionary
        int newObjKey = _fullMiniObjsDict.ElementAt(randM.Next(0, _fullMiniObjsDict.Count)).Key;
        // now we place this object
        float tempY = transform.parent.GetComponent<Renderer>().bounds.max.y;
        Vector3 tmpV = new Vector3(transform.position.x, tempY, transform.position.z);

        //Quaternion tmpR = _fullMiniObjsDict[newObjKey].transform.rotation;
        //tmpR = Quaternion.Euler(0, transform.rotation.y, 0);

        SpawnableMiniObject newSObj = Instantiate(_fullMiniObjsDict[newObjKey], tmpV, transform.rotation) as SpawnableMiniObject;
        newSObj.name += ": " + _placedMiniObjsCount;
        newSObj.transform.SetParent(gameObject.transform);

        if (newSObj.gameObject.name.Contains("PictureFrame")) {
            transform.eulerAngles = new Vector3(-15, transform.eulerAngles.y, transform.eulerAngles.z);
        } else if (newSObj.gameObject.name.Contains("book")) {
            tmpV = new Vector3(transform.position.x, tempY + 0.03f, transform.position.z);
            transform.position = tmpV;
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -90);
        } else if (newSObj.gameObject.name.Contains("paper")) {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, Random.Range(0, 359), transform.eulerAngles.z);
        } else if (newSObj.gameObject.name.Contains("Cloth")) {
            transform.localPosition = new Vector3(transform.localPosition.x, 0.83f, transform.localPosition.z);
        }
        if (gameObject.transform.parent.name.Contains("tableDinner"))
            transform.position = new Vector3(transform.position.x, 0.81f, transform.position.z);

        _placedMiniObjects.Add(newSObj);
        // thus we cannot again
        _fullMiniObjsDict.Remove(newObjKey);
    }

    private IEnumerator MiniObjectShelfSpawning() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        foreach (var item in _placedMiniObjects) {
            if (!item.GetPlacementCheck()) {
                Debug.Log("Skipping one turn. Waiting to place: " + item.name);
                yield return delay;
            }
        }

        while (_placedMiniObjsCount < _totalAmountOfMiniObjects) {

            _placedMiniObjsCount++;
            // Get random key from Dictionary
            int newObjKey = _fullMiniObjsDict.ElementAt(randM.Next(0, _fullMiniObjsDict.Count)).Key;

            // now we place this object
            SpawnableMiniObject newSObj = Instantiate(_fullMiniObjsDict[newObjKey], transform.position, transform.rotation * _fullMiniObjsDict[newObjKey].transform.rotation) as SpawnableMiniObject;
            newSObj.name += ": " + _placedMiniObjsCount;
            newSObj.transform.SetParent(gameObject.transform);
            _placedMiniObjects.Add(newSObj);
            // thus we cannot again
            _fullMiniObjsDict.Remove(newObjKey);

            if (newSObj.gameObject.name.Contains("paper"))
                transform.localPosition = new Vector3(transform.localPosition.x, 0.001f, transform.localPosition.z);

            if (gameObject.name.Contains("tableCloth"))
                newSObj.transform.localPosition = new Vector3(newSObj.transform.localPosition.x, newSObj.transform.localPosition.y - 0.38f, newSObj.transform.localPosition.z);

            yield return delay;
        }
    }

    private void Start() {
        if (SceneManager.GetActiveScene().name != "PCG") {
            _placed = true;
            return;
        }

        if (parentOfParent)
            _papa = gameObject.transform.parent.transform.parent.GetComponent<SpawnableObject>();
        else
            _papa = gameObject.transform.parent.GetComponent<SpawnableObject>();

        //int objSum = 0;

        //foreach (var item in MiniObjectsToPlace) {
        //    objSum += item.maxPlacementNum;
        //}

        //if (_totalAmountOfMiniObjects > objSum) {
        //    Debug.LogWarning("Sum of mini objects lower than the number asked to place. Exiting. " + gameObject.name);
        //    _placed = true;
        //    return;
        //}
    }

    void LateUpdate() {
        if (_papa == null) {
            Debug.Log("Check father: " + gameObject.name);
            return;
        }

        if (_papa.GetPlacementCheck() && !_placed) {
            CreateFullDict();
            if (parentOfParent)
                StartCoroutine(MiniObjectShelfSpawning());
            else
                StartCoroutine(MiniObjectSpawning());
            _placed = true;
        }
    }

    public void Reset() {
        if (parentOfParent)
            StopCoroutine(MiniObjectShelfSpawning());
        else
            StopCoroutine(MiniObjectSpawning());

        foreach (var obj in _placedMiniObjects) {
            Destroy(obj.gameObject);
        }

        _placedMiniObjects.Clear();
    }
}
