using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public GameObject Room;
    public List<SpawnableObject> ObjectsToPlace;

    private List<SpawnableObject> _placedObjects = new List<SpawnableObject>();

    private void Start() {
        BeginGame();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            RestartGame();
        }
    }

    private void BeginGame() {

        Vector3 RoomSize = Room.GetComponent<Collider>().bounds.size;
        Vector3 RoomBoundaries;
        RoomBoundaries.x = (RoomSize.x / 2f) - 1;
        RoomBoundaries.y = 0;
        RoomBoundaries.z = (RoomSize.z / 2f) - 1;

        foreach (var obj in ObjectsToPlace) {
            SpawnableObject tmp = Instantiate(obj) as SpawnableObject;
            Vector3 tmpBounds = tmp.GetComponent<Collider>().bounds.size;

            Vector3 tmpPosition =
                new Vector3(Random.Range(-RoomBoundaries.x, RoomBoundaries.x), tmpBounds.y - tmpBounds.y / 2f, Random.Range(-RoomBoundaries.z, RoomBoundaries.z));

            tmp.transform.position = tmpPosition;
            tmp.transform.rotation = Quaternion.identity;
            _placedObjects.Add(tmp);
        }
    }

    public void RemoveDestroyedObject(SpawnableObject sobj) {
        _placedObjects.Remove(sobj);
        Debug.LogWarning("Restarting as an object has been deleted");
        RestartGame();
    }

    private void RestartGame() {

        foreach (var obj in _placedObjects) {
            Destroy(obj.gameObject);
        }

        _placedObjects.Clear();
        BeginGame();
    }
}
