using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Spawner : MonoBehaviour {

    public GameObject RoomPrefab;
    public float generationStepDelay;
    public List<SpawnableObject> ObjectsToPlace;

    private GameObject _roomInstance;
    private List<SpawnableObject> _placedObjects = new List<SpawnableObject>();

    public IEnumerator Spawn() {

        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        _roomInstance = Instantiate(RoomPrefab) as GameObject;

        Vector3 RoomSize = _roomInstance.GetComponent<Collider>().bounds.size;
        Vector3 RoomBoundaries;
        RoomBoundaries.x = (RoomSize.x / 2f) - 1;
        RoomBoundaries.y = 0;
        RoomBoundaries.z = (RoomSize.z / 2f) - 1;

        foreach (var obj in ObjectsToPlace) {

            yield return delay;

            SpawnableObject tmp = Instantiate(obj) as SpawnableObject;
            Vector3 tmpBounds = tmp.GetComponent<Collider>().bounds.size;

            Vector3 tmpPosition =
                new Vector3(UnityEngine.Random.Range(-RoomBoundaries.x, RoomBoundaries.x), 
                tmpBounds.y - tmpBounds.y / 2f, UnityEngine.Random.Range(-RoomBoundaries.z, RoomBoundaries.z));

            tmpPosition = CollisionCheck(tmpPosition, obj);

            tmp.transform.position = tmpPosition;
            tmp.transform.rotation = Quaternion.identity;
            _placedObjects.Add(tmp);
        }

    }

    private Vector3 CollisionCheck(Vector3 tmpPosition, SpawnableObject obj) {
        Vector3 tmpV = tmpPosition;

        Collider[] hitColliders = Physics.OverlapSphere(tmpPosition, obj.GetComponent<Collider>().bounds.size.x);

        if (hitColliders.Length > 0) {
            foreach (var col in hitColliders) {
                Debug.Log("Colliding with: " + col.name);
            }
        }

        return tmpV;
    }

    public void RemoveDestroyedObject(SpawnableObject sobj) {
        _placedObjects.Remove(sobj);
        Debug.LogWarning("Deleting colliding: " + sobj.name);
        //RestartGame(); // from GameManager
    }

    public void Reset() {
        foreach (var obj in _placedObjects) {
            Destroy(obj.gameObject);
        }

        _placedObjects.Clear();

        Destroy(_roomInstance);
        Destroy(this.gameObject);
    }
}
