using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class SpawnableChair : MonoBehaviour {

    private bool _placed = false;
    private GameObject _target = null;
    private SpawnableBox[] _allBoxes;
    private Quaternion _lookRotation;
    private Vector3 _direction;

    void Start() {
        if (SceneManager.GetActiveScene().name != "ScriptTester") {
            _placed = true;
            return;
        }
        _allBoxes = FindObjectsOfType<SpawnableBox>();
    }

    void Update() {
        if (!_placed) {
            SpawnableBox objToUse = _allBoxes.Where(sbx => sbx.GetBoxCondition() == SpawnableBox.BoxCondition.ChairSpot).FirstOrDefault();
            if (objToUse != null) {
                _target = objToUse.GetFurniture();
                objToUse.SetChair(this);
                transform.position = objToUse.transform.position;
                transform.LookAt(_target.transform.position, Vector3.up);
            } else {
                Debug.LogWarning("No space for: " + gameObject.name + " disabling.");
                gameObject.SetActive(false);
            }
            _placed = true;
        }
        //Debug.DrawRay(transform.position, transform.forward, Color.red);
    }

    public bool GetPlacementCheck() {
        return _placed;
    }
}
