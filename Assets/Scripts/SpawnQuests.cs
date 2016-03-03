using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class SpawnQuests : MonoBehaviour {

    [Range(1, 10)]
    public int totalAmountOfSets = 2;

    public float generationStepDelay = 0.1f;
    public List<QuestItemScript> validQAs;
    public List<QuestItemScript> fakeQAs;

    private int _placedQuestCount = 0;
    private List<QuestItemScript> _placedQuests = new List<QuestItemScript>();
    private ClickableFurniture[] _allPossibleTargets;
    private Dictionary<string, int> _fullValidDic = new Dictionary<string, int>();
    private Dictionary<string, int> _fullFakeDic = new Dictionary<string, int>();
    private static System.Random rand = new System.Random();

    public void StartPlacingQuests() {
        _allPossibleTargets = FindObjectsOfType<ClickableFurniture>();

        if (_allPossibleTargets.Length * 2 < totalAmountOfSets) {
            Debug.LogWarning("The amount of clickable furniture is less than the required quests asked to place.");
            return;
        }

        // Check sets - valid
        for (int i = 0; i < validQAs.Count; i++) {
            QuestItemScript q = Instantiate(validQAs[i]);
            // replace the prefabs with the Instantiated objects, to control them
            validQAs[i] = q;
            q.gameObject.SetActive(false);

            string testKey = q.GetNameChar();

            if (_fullValidDic.ContainsKey(testKey)) {
                _fullValidDic[testKey] += 1;
            } else {
                _fullValidDic.Add(testKey, 1);
            }

            if (_fullValidDic[testKey] > 3) {
                Debug.LogWarning("More than 3 quest items with letter: " + testKey + ". Please recheck. Quest spawning canceled.");
                return;
            }
        }

        // Check sets - fake
        for (int i = 0; i < fakeQAs.Count; i++) {
            QuestItemScript q = Instantiate(fakeQAs[i]);
            // replace the prefabs with the Instantiated objects, to control them
            fakeQAs[i] = q;

            string testKey = q.GetNameChar();

            if (_fullFakeDic.ContainsKey(testKey)) {
                _fullFakeDic[testKey] += 1;
            } else {
                _fullFakeDic.Add(testKey, 1);
            }

            if (_fullFakeDic[testKey] > 3) {
                Debug.LogWarning("More than 3 quest items with letter: " + testKey + ". Please recheck. Quest spawning canceled.");
                return;
            }
        }

        StartCoroutine(TimeToPlace());
    }


    private IEnumerator TimeToPlace() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        PlaceValid();
        _placedQuestCount++;

        while (_placedQuestCount < totalAmountOfSets) {

            _placedQuestCount++;
            PlaceFake();
            yield return delay;
        }
    }

    private void PlaceFake() {
        // Get random key from Dictionary
        string testKey = _fullFakeDic.ElementAt(rand.Next(0, _fullValidDic.Count)).Key;

        // now we set these already instantiated object
        foreach (var q in fakeQAs) {
            if (q.GetNameChar() == testKey) {
                ClickableFurniture tmp = _allPossibleTargets.Where(cf => cf.questItemAttached == null).FirstOrDefault();
                if (tmp != null) {
                    Debug.Log("Setting " + q.name + " to " + tmp.name);
                    q.transform.SetParent(tmp.transform);
                    tmp.questItemAttached = q;
                    _placedQuests.Add(q);
                    q.gameObject.SetActive(true);
                } else {
                    Debug.LogWarning("Problem when placing: " + q.name);
                }
            }
        }
    }

    private void PlaceValid() {
        // Get random key from Dictionary
        string testKey = _fullValidDic.ElementAt(rand.Next(0, _fullValidDic.Count)).Key;

        // now we set these already instantiated object
        foreach (var q in validQAs) {
            if (q.GetNameChar() == testKey) {
                ClickableFurniture tmp = _allPossibleTargets.Where(cf => cf.questItemAttached == null).FirstOrDefault();
                if (tmp != null) {
                    Debug.Log("Setting " + q.name + " to " + tmp.name);
                    q.transform.SetParent(tmp.transform);
                    tmp.questItemAttached = q;
                    _placedQuests.Add(q);
                    q.gameObject.SetActive(true);
                } else {
                    Debug.LogWarning("Problem when placing: " + q.name);
                }
            }
        }
    }

    public void Reset() {
        StopCoroutine(TimeToPlace());

        foreach (var obj in _placedQuests) {
            Destroy(obj.gameObject);
        }

        _placedQuests.Clear();
    }
}