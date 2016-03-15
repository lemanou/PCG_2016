using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
/*
    This script divides the fake and valid quests into their corresponding dictionaries.
    We place quests on available furniture and parent the quests to the canvas and reset scaling to 1.
    The reason to use canvas as parent, is due to the quests using canvas renderers, to always put them on top. 
    On Reset() we stop all coroutines and destroy all quests.
*/
public class SpawnQuests : MonoBehaviour {

    [Range(1, 5)]
    public int totalAmountOfSets = 2;
    public float generationStepDelay = 0.1f;

    private int _placedQuestCount = 0;
    private static System.Random rand = new System.Random();
    private Canvas _canvas;
    private ClickableFurniture[] _allPossibleTargets;
    private QuestItemScript[] _allQuests;
    private List<QuestItemScript> _fakeQAs = new List<QuestItemScript>();
    private List<QuestItemScript> _validQAs = new List<QuestItemScript>();
    private List<QuestItemScript> _placedQuests = new List<QuestItemScript>();
    private List<ClickableFurniture> _possibleForQuests = new List<ClickableFurniture>();
    private Dictionary<string, int> _fullValidDic = new Dictionary<string, int>();
    private Dictionary<string, int> _fullFakeDic = new Dictionary<string, int>();

    public void StartPlacingQuests() {
        _allPossibleTargets = FindObjectsOfType<ClickableFurniture>();
        foreach (var item in _allPossibleTargets) {
            if (item.questItemAttached == null && !item.name.Contains("door"))
                _possibleForQuests.Add(item);
        }

        if (_possibleForQuests.Count < totalAmountOfSets * 3) { //  * 2
            Debug.LogWarning("The amount of clickable furniture is less than the required quests asked to place.");
            return;
        }

        //Debug.Log("Furniture: " + _possibleForQuests.Count + " requested quests (x3x2): " + totalAmountOfSets);
        //foreach (var item in _allPossibleTargets) {
        //    Debug.Log(item.name);
        //}

        ListShuffle(_possibleForQuests);
        LoadAllQuests();

        // Check sets - valid
        for (int i = 0; i < _validQAs.Count; i++) {
            QuestItemScript q = Instantiate(_validQAs[i]);
            // replace the prefabs with the Instantiated objects, to control them
            _validQAs[i] = q;
            string testKey = q.GetNameChar();
            q.gameObject.SetActive(false);

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
        for (int i = 0; i < _fakeQAs.Count; i++) {
            QuestItemScript q = Instantiate(_fakeQAs[i]);
            // replace the prefabs with the Instantiated objects, to control them
            _fakeQAs[i] = q;
            string testKey = q.GetNameChar();
            q.gameObject.SetActive(false);

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

        _canvas = FindObjectOfType<Canvas>();

        StartCoroutine(TimeToPlace());
    }

    private void LoadAllQuests() {
        _allQuests = Resources.LoadAll<QuestItemScript>("Quests");

        foreach (var quest in _allQuests) {
            if (quest.questNumber != -1)
                _validQAs.Add(quest);
            else
                _fakeQAs.Add(quest);
        }
        //Debug.Log("Found a total of : " + _allQuests.Length + " quests, of which " + _validQAs.Count + " valid, and " + _fakeQAs.Count + " fake.");
    }

    private IEnumerator TimeToPlace() {
        WaitForSeconds delay = new WaitForSeconds(generationStepDelay);

        PlaceValid();
        _placedQuestCount++;

        while (_placedQuestCount < totalAmountOfSets) {

            PlaceFake();
            _placedQuestCount++;
            yield return delay;
        }
    }

    private void PlaceFake() {
        // Get random key from Dictionary
        string testKey = _fullFakeDic.ElementAt(rand.Next(0, _fullFakeDic.Count)).Key;

        // now we set these already instantiated object
        foreach (QuestItemScript q in _fakeQAs) {
            if (q.GetNameChar() == testKey) {
                int r = UnityEngine.Random.Range(0, _possibleForQuests.Count);
                ListShuffle(_possibleForQuests);
                ClickableFurniture tmp = _possibleForQuests[r]; // _allPossibleTargets.Where(cf => cf.questItemAttached == null).FirstOrDefault();
                if (tmp != null) {
                    //Debug.Log("Placing fake " + q.name + " to " + tmp.name);
                    q.transform.SetParent(_canvas.transform.FindChild("QuestItemHolder").transform);
                    RectTransform _rectTrans = q.GetComponent<RectTransform>();
                    if (_rectTrans != null)
                    {
                        q.transform.localScale = Vector3.one;
                        _rectTrans.sizeDelta = Vector2.zero;
                        _rectTrans.anchoredPosition = Vector2.zero;
                    }
                    tmp.questItemAttached = q;
                    _placedQuests.Add(q);
                    q.gameObject.SetActive(true);
                    _possibleForQuests.Remove(tmp);
                    if (_fullFakeDic.ContainsKey(testKey))
                        _fullFakeDic.Remove(testKey);
                } else {
                    Debug.LogWarning("Problem when placing: " + q.name);
                }
            }
        }
    }

    private void PlaceValid() {
        // Get random key from Dictionary
        string testKey = _fullValidDic.ElementAt(rand.Next(0, _fullValidDic.Count)).Key;
        List<QuestItemScript> tmpLQSs = new List<QuestItemScript>();

        // now we set these already instantiated object
        foreach (QuestItemScript q in _validQAs) {
            if (q.GetNameChar() == testKey) {
                int r = UnityEngine.Random.Range(0, _possibleForQuests.Count);
                ListShuffle(_possibleForQuests);
                ClickableFurniture tmp = _possibleForQuests[r]; //_allPossibleTargets.Where(cf => cf.questItemAttached == null).FirstOrDefault();
                if (tmp != null) {
                    //Debug.Log("Placing valid " + q.name + " to " + tmp.name);
                    q.transform.SetParent(_canvas.transform.FindChild("QuestItemHolder").transform);
                    RectTransform _rectTrans = q.GetComponent<RectTransform>();
                    if (_rectTrans != null)
                    {
                        q.transform.localScale = Vector3.one;
                        _rectTrans.sizeDelta = Vector2.zero;
                        _rectTrans.anchoredPosition = Vector2.zero;
                    }
                    tmp.questItemAttached = q;
                    _placedQuests.Add(q);
                    q.gameObject.SetActive(true);
                    _possibleForQuests.Remove(tmp);
                    //q.AddQuestNumber();
                } else {
                    Debug.LogWarning("Problem when placing: " + q.name);
                }
            } else {
                tmpLQSs.Add(q);
            }
        }

        foreach (var item in tmpLQSs) {
            _validQAs.Remove(item);
        }

        tmpLQSs.ForEach(nq => Destroy(nq.gameObject));
    }

    public void Reset() {
        StopCoroutine(TimeToPlace());

        foreach (var item in _fakeQAs) {
            Destroy(item.gameObject);
        }

        _fakeQAs.Clear();

        foreach (var item in _validQAs) {
            Destroy(item.gameObject);
        }

        _validQAs.Clear();

        _placedQuests.Clear();
    }

    private void ListShuffle(List<ClickableFurniture> myList) {
        System.Random rng = new System.Random();
        int n = myList.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            ClickableFurniture value = myList[k];
            myList[k] = myList[n];
            myList[n] = value;
        }
    }
}