using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(SpawnQuests))]
[RequireComponent(typeof(SpawnableChair))]
[RequireComponent(typeof(SpawnWallObjects))]

/*
    This script checks if there's enough furniture to fulfill the level settings.
    We destroy everything in the scene, change camera and stop coroutines on Reset().
    The tile boxes are created and a dictionary for all furniture to be placed.
    We instantiate furniture and repeat for carpets.
    SpawnChairs.cs calls DeleteAllBoxes(), as the chairs are the last furniture to be placed.
        -Which creates the canvas and quests and calls StartGame().
            -Which creates the player and changes camera.
    In LateUpdate() we make sure that the wall objects and chairs are placed after the furniture.
*/
public class Spawner : MonoBehaviour {

    [Range(1, 20)]
    public int totalAmountOfFurniture = 12;
    [Range(0, 8)]
    public int totalAmountOfCarpets = 4;

    public float generationStepDelay = 0.1f;
    public GameObject RoomPrefab;
    public SpawnableBox SpawningBoxPrefab;
    public List<SpawnableObject> FurnitureToPlace,
        CarpetsToPlace;

    private bool _once = true;
    private int _placedFurnitureCount = 0,
        _placedCarpetsCount = 0;
    private IntVector2 _size;
    private Canvas _canvas;
    private FirstPersonController _player;
    private SpawnableBox[,] _boxes;
    private GameObject _roomInstance;
    private List<SpawnableObject> _placedFurniture = new List<SpawnableObject>(),
        _placedCarpets = new List<SpawnableObject>();
    private Dictionary<int, SpawnableObject> _fullFurnitureDic = new Dictionary<int, SpawnableObject>(),
        _fullCarpetsDic = new Dictionary<int, SpawnableObject>();
    private Vector3 _roomSize, _roomBoundaries;
    private static System.Random rand = new System.Random();
    private Camera _firstCam;

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

        // we have to spawn the canvas early to use it for the quest spawning
        _canvas = Instantiate(Resources.Load("Canvas", typeof(Canvas)), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as Canvas;

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

        if (_firstCam != null)
            _firstCam.gameObject.SetActive(true);

        if (_player != null)
            Destroy(_player.gameObject);
        if (_canvas != null)
            Destroy(_canvas.gameObject);
        if (_roomInstance != null)
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

            // now we place this object
            SpawnableObject newSObj = Instantiate(_fullFurnitureDic[newObjKey]);
            newSObj.name += ": " + _placedFurnitureCount;
            _placedFurniture.Add(newSObj);
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
        StopCoroutine(CreateSpawningCarpetsStepThree());

        var children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);
        children.ForEach(child => Destroy(child)); //  child.GetComponent<Renderer>().enabled = false



        // After finishing with all object placement we can start placing quests
        SpawnQuests qc = GetComponent<SpawnQuests>();
        qc.StartPlacingQuests();

        StartGame();
    }

    private void StartGame() {
        _firstCam = FindObjectOfType<Camera>();
        _firstCam.gameObject.SetActive(false);

        _player = Instantiate(Resources.Load("FPSController", typeof(FirstPersonController)), new Vector3(3.5f, 1.0f, 0.0f), Quaternion.identity) as FirstPersonController;
        NumberDialScript tmpDial = _player.transform.GetChild(0).GetComponentInChildren<NumberDialScript>();
        tmpDial.gameObject.SetActive(false);
        ClickableFurniture[] _allCF = FindObjectsOfType<ClickableFurniture>();
        foreach (var item in _allCF) {
            if (item.gameObject.name == "door")
                item.numberDialAttached = tmpDial.gameObject;
        }

        Camera _cam = _player.gameObject.GetComponent<Transform>().GetChild(0).GetComponent<Camera>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.worldCamera = _cam;

        LookedAtFurniture mwe = FindObjectOfType<LookedAtFurniture>();
        if (mwe != null)
            mwe.StartFindingObjects();
        //Debug.Log("E.N.D.");
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

                if (tmp) {
                    _once = false;
                    // The chairs are the last furniture to spawn
                    gameObject.GetComponent<SpawnWallObjects>().StartPlacement();
                    gameObject.GetComponent<SpawnChairs>().StartChairPlacement();
                }
            }
        }
    }
}
