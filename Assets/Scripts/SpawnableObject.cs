using UnityEngine;

public class SpawnableObject : MonoBehaviour {

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.GetComponent<SpawnableObject>() != null) {
            //Spawner tmp = FindObjectOfType<Spawner>();
            //tmp.RemoveDestroyedObject(this);
            //Destroy(gameObject);
        }
    }
}
