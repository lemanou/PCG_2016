using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Spawner : MonoBehaviour {

    [Range(1, 20)]
    public int totalAmountOfFurniture = 12;

    public float generationStepDelay;
    public GameObject RoomPrefab;
    public SpawningBox SpawningBoxPrefab;
    public List<SpawnableObject> ObjectsToPlace;

    private bool _once = true;
    private int _placedObjCount = 0;
    private IntVector2 _size;
    private SpawningBox[,] _boxes;
    private GameObject _roomInstance;
    private List<SpawnableObject> _placedObjects = new List<SpawnableObject>();
    private Dictionary<int, SpawnableObject> _fullDic = new Dictionary<int, SpawnableObject>();
    private Vector3 _roomSize, _roomBoundaries;

    public Vector3 GetBoundaries() {
        return _roomBoundaries;
    }

    public void Spawn() {

        int sum = 0;

        foreach (var item in ObjectsToPlace) {
            sum += item.maxPlacementNum;
        }

        if (totalAmountOfFurniture > sum) {
            Debug.LogWarning("Sum of furniture lower than the objects asked to place. Exiting.");
            return;
        }

        _roomInstance = Instantiate(RoomPrefab) as GameObject;

        GetRoomBoundariesStepZero();
        SpawnBoxesStepOne();
        CreateFullList();
        StartCoroutine(CreateSpawningObjectsStepTwo());
    }

    public void Reset() {
        StopCoroutine(CreateSpawningObjectsStepTwo());

        foreach (var obj in _placedObjects) {
            Destroy(obj.gameObject);
        }

        _placedObjects.Clear();

        Destroy(_roomInstance);
        Destroy(gameObject);
    }

    private void GetRoomBoundariesStepZero() {
        _roomSize = _roomInstance.GetComponent<Collider>().bounds.size;
        _roomBoundaries.x = (_roomSize.x / 2f) - 1;
        _roomBoundaries.y = 0;
        _roomBoundaries.z = (_roomSize.z / 2f) - 1;

        // Set the size of the array based on the room size
        _size.x = (int)_roomSize.x;
        _size.z = (int)_roomSize.z;
    }

    private void SpawnBoxesStepOne() {
        _boxes = new SpawningBox[_size.x, _size.z];
        for (int x = 0; x < _size.x; x++) {
            for (int z = 0; z < _size.z; z++) {
                CreateBox(new IntVector2(x, z));
            }
        }
    }

    private void CreateBox(IntVector2 coordinates) {
        SpawningBox newBox = Instantiate(SpawningBoxPrefab) as SpawningBox;
        _boxes[coordinates.x, coordinates.z] = newBox;
        newBox.LocalCoordinates = coordinates;
        newBox.name = "Spawned Box " + coordinates.x + ", " + coordinates.z;
        newBox.transform.parent = transform;
        newBox.transform.localPosition =
            new Vector3(coordinates.x - _size.x * 0.5f + 0.5f, 0f, coordinates.z - _size.z * 0.5f + 0.5f);
    }

    private void CreateFullList() {
        int countKey = 0;
        foreach (SpawnableObject sbx in ObjectsToPlace) {
            int x = sbx.maxPlacementNum;
            while (x > 0) {
                x--;
                countKey++;
                _fullDic.Add(countKey, sbx);
            }
        }
        //Debug.Log("Original: " + ObjectsToPlace.Count + " Full: " + _fullDic.Count);
    }

    private IEnumerator CreateSpawningObjectsStepTwo() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        foreach (var item in _placedObjects) {
            if (!item.GetPlacementCheck()) {
                Debug.Log("Skipping one turn. Waiting to place: " + item.name);
                yield return delay;
            }
        }

        while (_placedObjCount < totalAmountOfFurniture) {

            _placedObjCount++;
            // Get random key from Dictionary
            System.Random rand = new System.Random();
            int newObjKey = _fullDic.ElementAt(rand.Next(0, _fullDic.Count)).Key;

            // now we place this object, thus we cannot again
            SpawnableObject newSObj = Instantiate(_fullDic[newObjKey]);
            newSObj.name += ": " + _placedObjCount;
            _placedObjects.Add(newSObj);

            _fullDic.Remove(newObjKey);
            //Debug.Log("Count: " + _placedObjCount + " Left in dictionary: " + _fullDic.Count);
            yield return delay;
        }
    }

    private void DeleteAllBoxes() {
        StopCoroutine(CreateSpawningObjectsStepTwo());

        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child)); // child.GetComponent<Renderer>().enabled = false
    }

    private void LateUpdate() {
        if (_once) {
            if (_placedObjCount >= totalAmountOfFurniture) {
                bool tmp = true;
                foreach (var item in _placedObjects) {
                    if (!item.GetPlacementCheck()) {
                        tmp = false;
                    }
                }
                // now we have placed all requested objects and we can delete the boxes
                if (tmp) {
                    _once = false;
                    DeleteAllBoxes();
                }
            }
        }
    }
}
