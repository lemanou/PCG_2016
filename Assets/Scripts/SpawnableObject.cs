using UnityEngine;

public class SpawnableObject : MonoBehaviour {

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.GetComponent<SpawnableObject>() != null) {
            GameManager tmp = FindObjectOfType<GameManager>();
            tmp.RemoveDestroyedObject(this);
            Destroy(gameObject);
        }
    }
}
