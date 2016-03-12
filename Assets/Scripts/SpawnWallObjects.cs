using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/*
    This script finds free spots to place furniture against the walls and then instantiates the furniture.
    On Reset() stops the coroutine and destroys all related objects.
*/
public class SpawnWallObjects : MonoBehaviour {

    [Range(0, 10)]
    public int totalAmountOfWO = 6;
    public float generationStepDelay;
    public List<SpawnableWallObject> WallObjsToPlace;

    private int _placedNumOfObjs = 0;
    private SpawnableBox[] _allBoxes;
    private List<SpawnableBox> _possibleSpots = new List<SpawnableBox>();
    private List<SpawnableWallObject> _placedObjs = new List<SpawnableWallObject>();
    private Dictionary<int, SpawnableWallObject> _fullObjDic = new Dictionary<int, SpawnableWallObject>();
    private System.Random randW = new System.Random();

    private bool CheckPossibleBoxes() {
        // Check for available spots
        foreach (var box in _allBoxes) {
            if ((box.GetBoxCondition() == SpawnableBox.BoxCondition.Free || box.GetBoxCondition() == SpawnableBox.BoxCondition.Short) &&
                (box.GetBoxLocation() == SpawnableBox.BoxLocation.North || box.GetBoxLocation() == SpawnableBox.BoxLocation.East || box.GetBoxLocation() == SpawnableBox.BoxLocation.South
                || box.GetBoxLocation() == SpawnableBox.BoxLocation.West)) {

                _possibleSpots.Add(box);
                //box.GetComponent<Renderer>().enabled = true;
                //box.GetComponent<Renderer>().material.color = Color.blue;
            }

        }

        //Debug.Log("Possible number of wall spots: " + _possibleSpots.Count);

        if (_possibleSpots.Count > 0)
            return true;
        else
            return false;
    }

    private void CreateFullDict() {
        int countKey = 0;
        foreach (SpawnableWallObject sbx in WallObjsToPlace) {
            int x = sbx.maxPlacementNum;
            while (x > 0) {
                x--;
                countKey++;
                _fullObjDic.Add(countKey, sbx);
            }
        }
    }

    private IEnumerator ObjectSpawning() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        foreach (var item in _placedObjs) {
            if (!item.GetPlacementCheck()) {
                Debug.Log("Skipping one turn. Waiting to place: " + item.name);
                yield return delay;
            }
        }

        while (_placedNumOfObjs < totalAmountOfWO) {

            _placedNumOfObjs++;
            // Get random key from Dictionary
            int newObjKey = _fullObjDic.ElementAt(randW.Next(0, _fullObjDic.Count)).Key;

            // now we place this object
            SpawnableWallObject newSObj = Instantiate(_fullObjDic[newObjKey]);
            newSObj.name += ": " + _placedNumOfObjs;
            _placedObjs.Add(newSObj);
            // thus we cannot again
            _fullObjDic.Remove(newObjKey);

            yield return delay;
        }
    }

    public void StartPlacement() {

        int objSum = 0;

        foreach (var item in WallObjsToPlace) {
            objSum += item.maxPlacementNum;
        }

        if (totalAmountOfWO > objSum) {
            Debug.LogWarning("Sum of wall objects lower than the number asked to place. Exiting.");
            return;
        }

        _allBoxes = FindObjectsOfType<SpawnableBox>();

        if (CheckPossibleBoxes()) {
            CreateFullDict();
            StartCoroutine(ObjectSpawning());
        } else {
            Debug.Log("No spots available for wall object placement.");
        }
    }

    public List<SpawnableBox> GetPossibleWallSpots() {
        return _possibleSpots;
    }

    public void RemoveSpot(SpawnableBox box) {
        _possibleSpots.Remove(box);
    }

    public void Reset() {
        StopCoroutine(ObjectSpawning());

        foreach (var obj in _placedObjs) {
            Destroy(obj.gameObject);
        }

        _placedObjs.Clear();
    }
}
