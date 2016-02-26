using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnChairs : MonoBehaviour {

    [Range(0, 8)]
    public int totalAmountOfChairs = 4;

    public float generationStepDelay;
    public SpawnableChair SpawningChairPrefab;

    private bool _check = true;
    private int _placedNumOfChairs = 0;
    private SpawningBox[] _allBoxes;
    private List<SpawnableChair> _placedChairs = new List<SpawnableChair>();

    void Start() {
        _allBoxes = FindObjectsOfType<SpawningBox>();
    }

    private bool CheckPossibleBoxes() {
        // Check for available spots
        foreach (var box in _allBoxes) {
            if (box.GetBoxCondition() == SpawningBox.BoxCondition.ChairSpot)
                return true;
        }
        return false;
    }

    private IEnumerator ChairSpawning() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        foreach (var item in _placedChairs) {
            if (!item.GetPlacementCheck()) {
                Debug.Log("Skipping one turn. Waiting to place: " + item.name);
                yield return delay;
            }
        }

        while (_placedNumOfChairs < totalAmountOfChairs) {

            _placedNumOfChairs++;

            SpawnableChair newSObj = Instantiate(SpawningChairPrefab);
            newSObj.name += ": " + _placedNumOfChairs;
            _placedChairs.Add(newSObj);

            yield return delay;
        }
    }

    public void StartChairPlacement() {
        if (CheckPossibleBoxes()) {
            _check = true;
            StartCoroutine(ChairSpawning());
        } else {
            _check = false;
            Debug.Log("No spots available for chair placement.");
        }
    }

    private void LateUpdate() {
        if (_check) {
            if (_placedNumOfChairs >= totalAmountOfChairs) {
                bool tmp = true;

                foreach (var chair in _placedChairs)
                    if (!chair.GetPlacementCheck())
                        tmp = false;

                // now we have placed all requested objects and we can delete the boxes
                if (tmp) {
                    _check = false;
                    Spawner sp = FindObjectOfType<Spawner>();
                    sp.DeleteAllBoxes();
                    //Debug.Log("E.N.D.");
                }
            }
        }
    }

    public void Reset() {
        StopCoroutine(ChairSpawning());

        foreach (var obj in _placedChairs) {
            Destroy(obj.gameObject);
        }

        _placedChairs.Clear();
    }
}
