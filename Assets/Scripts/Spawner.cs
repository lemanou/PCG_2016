using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SpawnableChair))]
[RequireComponent(typeof(SpawnWallObjects))]

public class Spawner : MonoBehaviour {

    [Range(1, 20)]
    public int totalAmountOfFurniture = 12;
    [Range(0, 8)]
    public int totalAmountOfCarpets = 4;

    public float generationStepDelay;
    public GameObject RoomPrefab;
    public SpawnableBox SpawningBoxPrefab;
    public List<SpawnableObject> FurnitureToPlace,
        CarpetsToPlace;

    private bool _once = true;
    private int _placedFurnitureCount = 0,
        _placedCarpetsCount = 0;
    private IntVector2 _size;
    private SpawnableBox[,] _boxes;
    private GameObject _roomInstance;
    private List<SpawnableObject> _placedFurniture = new List<SpawnableObject>(),
        _placedCarpets = new List<SpawnableObject>();
    private Dictionary<int, SpawnableObject> _fullFurnitureDic = new Dictionary<int, SpawnableObject>(),
        _fullCarpetsDic = new Dictionary<int, SpawnableObject>();
    private Vector3 _roomSize, _roomBoundaries;
    private static System.Random rand = new System.Random();

    public Vector3 GetBoundaries() {
        return _roomBoundaries;
    }

    public void Spawn() {

        int furnitureSum = 0;

        foreach (var item in FurnitureToPlace) {
            furnitureSum += item.maxPlacementNum;
        }

        if (totalAmountOfFurniture > furnitureSum) {
            Debug.LogWarning("Sum of furniture lower than the number asked to place. Exiting.");
            return;
        }

        int carpetSum = 0;

        foreach (var item in CarpetsToPlace) {
            carpetSum += item.maxPlacementNum;
        }

        if (totalAmountOfCarpets > carpetSum) {
            Debug.LogWarning("Sum of carpets lower than the number asked to place. Exiting.");
            return;
        }

        _roomInstance = Instantiate(RoomPrefab) as GameObject;

        GetRoomBoundariesStepZero();
        SpawnBoxesStepOne();
        CreateFullFurnitureDic();
        StartCoroutine(CreateSpawningObjectsStepTwo());
        CreateFullCarpetsDic();
        StartCoroutine(CreateSpawningCarpetsStepThree());
    }

    public void Reset() {
        StopCoroutine(CreateSpawningObjectsStepTwo());
        StopCoroutine(CreateSpawningCarpetsStepThree());

        foreach (var obj in _placedFurniture) {
            Destroy(obj.gameObject);
        }

        _placedFurniture.Clear();

        foreach (var obj in _placedCarpets) {
            Destroy(obj.gameObject);
        }

        _placedCarpets.Clear();

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
        _boxes = new SpawnableBox[_size.x, _size.z];
        for (int x = 0; x < _size.x; x++) {
            for (int z = 0; z < _size.z; z++) {
                CreateBox(new IntVector2(x, z));
            }
        }
    }

    private void CreateBox(IntVector2 coordinates) {
        SpawnableBox newBox = Instantiate(SpawningBoxPrefab) as SpawnableBox;
        _boxes[coordinates.x, coordinates.z] = newBox;
        newBox.LocalCoordinates = coordinates;
        newBox.name = "Spawned Box " + coordinates.x + ", " + coordinates.z;
        newBox.transform.parent = transform;
        newBox.transform.localPosition =
            new Vector3(coordinates.x - _size.x * 0.5f + 0.5f, 0f, coordinates.z - _size.z * 0.5f + 0.5f);
    }

    private void CreateFullFurnitureDic() {
        int countKey = 0;
        foreach (SpawnableObject sbx in FurnitureToPlace) {
            int x = sbx.maxPlacementNum;
            while (x > 0) {
                x--;
                countKey++;
                _fullFurnitureDic.Add(countKey, sbx);
            }
        }
        //Debug.Log("Original: " + ObjectsToPlace.Count + " Full: " + _fullDic.Count);
    }

    private IEnumerator CreateSpawningObjectsStepTwo() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        foreach (var item in _placedFurniture) {
            if (!item.GetPlacementCheck()) {
                Debug.Log("Skipping one turn. Waiting to place: " + item.name);
                yield return delay;
            }
        }

        while (_placedFurnitureCount < totalAmountOfFurniture) {

            _placedFurnitureCount++;
            // Get random key from Dictionary
            int newObjKey = _fullFurnitureDic.ElementAt(rand.Next(0, _fullFurnitureDic.Count)).Key;

            //if (_fullFurnitureDic[newObjKey].gameObject.name.Contains("tableDinner")) {
            // now we place this object
            SpawnableObject newSObj = Instantiate(_fullFurnitureDic[newObjKey]);
            newSObj.name += ": " + _placedFurnitureCount;
            _placedFurniture.Add(newSObj);
            //}
            // thus we cannot again
            _fullFurnitureDic.Remove(newObjKey);
            //Debug.Log("Count: " + _placedObjCount + " Left in dictionary: " + _fullDic.Count);
            yield return delay;
        }
    }

    private void CreateFullCarpetsDic() {
        int countKey = 0;
        foreach (SpawnableObject sbx in CarpetsToPlace) {
            int x = sbx.maxPlacementNum;
            while (x > 0) {
                x--;
                countKey++;
                _fullCarpetsDic.Add(countKey, sbx);
            }
        }
        //Debug.Log("Original: " + ObjectsToPlace.Count + " Full: " + _fullDic.Count);
    }

    private IEnumerator CreateSpawningCarpetsStepThree() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        foreach (var carpet in _placedCarpets) {
            if (!carpet.GetPlacementCheck()) {
                Debug.Log("Skipping one turn. Waiting to place: " + carpet.name);
                yield return delay;
            }
        }

        while (_placedCarpetsCount < totalAmountOfCarpets) {

            _placedCarpetsCount++;
            // Get random key from Dictionary
            int newObjKey = _fullCarpetsDic.ElementAt(rand.Next(0, _fullCarpetsDic.Count)).Key;

            // now we place this object
            SpawnableObject newSCarpet = Instantiate(_fullCarpetsDic[newObjKey]);
            newSCarpet.name += ": " + _placedCarpetsCount;
            _placedCarpets.Add(newSCarpet);
            // thus we cannot again
            _fullCarpetsDic.Remove(newObjKey);
            //Debug.Log("Count: " + _placedObjCount + " Left in dictionary: " + _fullDic.Count);
            yield return delay;
        }
    }

    public void DeleteAllBoxes() {
        StopCoroutine(CreateSpawningObjectsStepTwo());

        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child)); // child.GetComponent<Renderer>().enabled = false
    }

    private void LateUpdate() {
        if (_once) {
            if (_placedFurnitureCount >= totalAmountOfFurniture && _placedCarpetsCount >= totalAmountOfCarpets) {

                bool tmp = true;

                foreach (var item in _placedFurniture) {
                    if (!item.GetPlacementCheck()) {
                        tmp = false;
                    }
                }

                foreach (var item in _placedCarpets) {
                    if (!item.GetPlacementCheck()) {
                        tmp = false;
                    }
                }

                // now we have placed all requested furniture and we can delete the boxes
                if (tmp) {
                    _once = false;
                    //DeleteAllBoxes(); // The chairs decide the end now
                    //Debug.Log("E.N.D.");
                    gameObject.GetComponent<SpawnWallObjects>().StartPlacement();
                    gameObject.GetComponent<SpawnChairs>().StartChairPlacement();
                }
            }
        }
    }
}
