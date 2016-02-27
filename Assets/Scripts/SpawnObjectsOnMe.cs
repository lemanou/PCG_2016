using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class SpawnObjectsOnMe : MonoBehaviour {

    [Range(1, 10)]
    public int totalAmountOfMiniObjects = 6;
    public float generationStepDelay;
    public List<SpawnableMiniObject> MiniObjectsToPlace;

    private int _placedMiniObjsCount = 0;
    private SpawnableBox[] _allBoxes;
    private List<SpawnableBox> _possibleSpots = new List<SpawnableBox>();
    private List<SpawnableMiniObject> _placedMiniObjects = new List<SpawnableMiniObject>();
    private Dictionary<int, SpawnableMiniObject> _fullMiniObjsDict = new Dictionary<int, SpawnableMiniObject>();
    private static System.Random randM = new System.Random();

    private void CreateFullDict() {
        int countKey = 0;
        foreach (var sbx in MiniObjectsToPlace) {
            int x = sbx.maxPlacementNum;
            while (x > 0) {
                x--;
                countKey++;
                _fullMiniObjsDict.Add(countKey, sbx);
            }
        }
    }

    private IEnumerator MiniObjectSpawning() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        foreach (var item in _placedMiniObjects) {
            if (!item.GetPlacementCheck()) {
                Debug.Log("Skipping one turn. Waiting to place: " + item.name);
                yield return delay;
            }
        }

        while (_placedMiniObjsCount < totalAmountOfMiniObjects) {

            _placedMiniObjsCount++;
            // Get random key from Dictionary
            int newObjKey = _fullMiniObjsDict.ElementAt(randM.Next(0, _fullMiniObjsDict.Count)).Key;

            // now we place this object
            SpawnableMiniObject newSObj = Instantiate(_fullMiniObjsDict[newObjKey], transform.position, Quaternion.identity) as SpawnableMiniObject;
            newSObj.name += ": " + _placedMiniObjsCount;
            newSObj.transform.SetParent(gameObject.transform);
            _placedMiniObjects.Add(newSObj);
            // thus we cannot again
            _fullMiniObjsDict.Remove(newObjKey);

            yield return delay;
        }
    }

    private void Start() {

        if (SceneManager.GetActiveScene().name != "ScriptTester") {
            return;
        }

        int objSum = 0;

        foreach (var item in MiniObjectsToPlace) {
            objSum += item.maxPlacementNum;
        }

        if (totalAmountOfMiniObjects > objSum) {
            Debug.LogWarning("Sum of mini objects lower than the number asked to place. Exiting. " + gameObject.name);
            return;
        }

        CreateFullDict();
        StartCoroutine(MiniObjectSpawning());
    }    

    public void Reset() {
        StopCoroutine(MiniObjectSpawning());

        foreach (var obj in _placedMiniObjects) {
            Destroy(obj.gameObject);
        }

        _placedMiniObjects.Clear();
    }
}
