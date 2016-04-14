using UnityEngine;
using UnityEngine.SceneManagement;

public class PickPositionForDinnerCloth : MonoBehaviour {

    public Transform[] points;
    public GameObject cloth;

    void Start() {
        if (SceneManager.GetActiveScene().name != "PCG") {
            gameObject.SetActive(false);
            return;
        }

        int r = Random.Range(0, points.Length - 1);
        cloth.transform.position = new Vector3(points[r].transform.position.x, 1.135f, points[r].transform.position.z);
        cloth.SetActive(true);
        foreach (var p in points) {
            p.gameObject.SetActive(false);
        }
    }
}
