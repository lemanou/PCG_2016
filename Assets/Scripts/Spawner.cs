using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Spawner : MonoBehaviour {

    public GameObject RoomPrefab;
    public float generationStepDelay;
    public List<SpawnableObject> ObjectsToPlace;

    private IntVector2 _size;
    public SpawningBox SpawningBoxPrefab;
    private SpawningBox[,] _boxes;

    private GameObject _roomInstance;
    private List<SpawnableObject> _placedObjects = new List<SpawnableObject>();
    private Vector3 _roomSize, _roomBoundaries;

    public Vector3 GetBoundaries() {
        return _roomBoundaries;
    }

    public Vector3 GetRoomSize() {
        return _roomSize;
    }

    public void Spawn() {

        _roomInstance = Instantiate(RoomPrefab) as GameObject;

        GetRoomBoundariesStepZero();
        SpawnBoxesStepOne();
        StartCoroutine(CreateSpawningObjectsStepTwo());
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

    public void SpawnBoxesStepOne() {
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

    private IEnumerator CreateSpawningObjectsStepTwo() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        // Sort to start placing by the largest object first
        List<SpawnableObject> SortedList = ObjectsToPlace.OrderByDescending(o =>
            o.gameObject.GetComponent<Renderer>().bounds.size.x * o.gameObject.GetComponent<Renderer>().bounds.size.z).ToList();

        foreach (SpawnableObject obj in SortedList) {

            int x = obj.placementNumber;
            while (x > 0) {
                yield return delay;
                x--;
                SpawnableObject newObject = Instantiate(obj) as SpawnableObject;
                _placedObjects.Add(newObject);
            }
        }
    }

    public void Reset() {
        StopAllCoroutines();

        foreach (var obj in _placedObjects) {
            Destroy(obj.gameObject);
        }

        _placedObjects.Clear();

        Destroy(_roomInstance);
        Destroy(gameObject);
    }

    //public void RemoveDestroyedObject(SpawnableObject sobj) {
    //    _placedObjects.Remove(sobj);
    //    Debug.LogWarning("Deleting colliding: " + sobj.name);
    //    //RestartGame(); 
    //}
}
