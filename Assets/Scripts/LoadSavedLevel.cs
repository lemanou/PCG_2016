using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using System;

public class LoadSavedLevel : MonoBehaviour {

    public TextAsset csvFile; // Reference of CSV file
    public GameObject[] Objects;

    private bool _found;
    private char lineSeperater = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate chracter
    private List<string> quests = new List<string>();
    private Canvas _canvas;
    private FirstPersonController _player;

    void Start() {
        // Has to be spawned first to parent the quests under it
        _canvas = Instantiate(Resources.Load("Canvas", typeof(Canvas)), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as Canvas;
        ReadCsvAndSpawn();
        AssignQuests();

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("2")) {
            LoadStaticHeatMap lsh = FindObjectOfType<LoadStaticHeatMap>();
            if (lsh != null)
                lsh.StartColoring();
        }
        StartGame();
    }

    private void StartGame() {
        FindObjectOfType<Camera>().gameObject.SetActive(false);

        _player = Instantiate(Resources.Load("FPSController", typeof(FirstPersonController)), new Vector3(3.5f, 1.0f, 0.0f), Quaternion.identity) as FirstPersonController;
        _player.transform.Rotate(new Vector3(0, 1, 0), -90);

        NumberDialScript tmpDial = _player.transform.GetChild(0).GetComponentInChildren<NumberDialScript>();
        tmpDial.gameObject.SetActive(false);

        ClickableFurniture[] _allCF = FindObjectsOfType<ClickableFurniture>();
        foreach (var item in _allCF) {
            if (item.gameObject.name == "door")
                item.numberDialAttached = tmpDial.gameObject;
        }

        Camera _cam = _player.gameObject.GetComponent<Transform>().GetChild(0).GetComponent<Camera>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.worldCamera = _cam;

        LookedAtFurniture laf = FindObjectOfType<LookedAtFurniture>();
        if (laf != null)
            laf.StartFindingObjects();
        //Debug.Log("E.N.D.");
    }

    private void AssignQuests() {
        ClickableFurniture[] objsInScene = FindObjectsOfType<ClickableFurniture>();
        QuestItemScript[] questsInScene = FindObjectsOfType<QuestItemScript>();

        foreach (var item in quests) {
            char[] myC = { '_' };
            string[] s = item.Split(myC);
            string objToFind = s[0];
            string questToAssign = s[1];
            bool placed = false;

            foreach (var o in objsInScene) {
                if (o.name == item) {
                    //Debug.Log("!!!" + item + "!!!");
                    foreach (var q in questsInScene) {

                        string qname = q.name;
                        int c = qname.IndexOf("("); // cut everything after (
                        if (c > 0)
                            qname = qname.Substring(0, c);

                        if (qname == questToAssign) {
                            //Debug.Log("!!!" + questToAssign + "!!!");
                            o.questItemAttached = q;
                            q.transform.SetParent(_canvas.transform.FindChild("QuestItemHolder").transform);
                            RectTransform _rectTrans = q.GetComponent<RectTransform>();
                            if (_rectTrans != null) {
                                q.transform.localScale = Vector3.one;
                                _rectTrans.sizeDelta = Vector2.zero;
                                _rectTrans.anchoredPosition = Vector2.zero;
                            }
                            placed = true;
                            break;
                        }
                    }
                    break;
                }
            }

            if (!placed)
                Debug.LogWarning("Could not place quest: " + questToAssign + " to: " + objToFind);
        }
    }

    // Read data from CSV file
    private void ReadCsvAndSpawn() {
        string[] records = csvFile.text.Split(lineSeperater);
        foreach (string record in records) {
            bool questB = false;
            string objNameFull = "";
            //Debug.Log("NewRow");
            string[] fields = record.Split(fieldSeperator);

            if (fields.Length == 8) {
                string csvObjName = fields[0];

                if (csvObjName.Contains("_Q")) {
                    char[] myC = { '_' };
                    string[] s = csvObjName.Split(myC);
                    //Debug.Log("~~~~~~Found: " + s[0] + " had the " + s[1]);
                    objNameFull = csvObjName;
                    quests.Add(objNameFull); // first add the full name to the list 
                    csvObjName = s[0]; // then use the trimmed
                    questB = true;
                }

                float posx = float.Parse(fields[1]);
                float posy = float.Parse(fields[2]);
                float posz = float.Parse(fields[3]);
                Vector3 pos = new Vector3(posx, posy, posz);

                float rotx = float.Parse(fields[4]);
                float roty = float.Parse(fields[5]);
                float rotz = float.Parse(fields[6]);
                float rotw = float.Parse(fields[7]);
                Quaternion rot = new Quaternion(rotx, roty, rotz, rotw);

                //Debug.Log(name + " " + pos + " " + rot);
                _found = false;
                foreach (var obj in Objects) {
                    if (obj.name == csvObjName) {
                        //Debug.Log("Found object in array, " + name);
                        _found = true;

                        GameObject go = Instantiate(obj, pos, rot) as GameObject;
                        if (questB)
                            go.name = objNameFull;

                        break;
                    }
                }
                if (_found == false) {
                    Debug.LogWarning("Didn't find obj, " + csvObjName);
                }
            }

        }
    }
}