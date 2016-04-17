﻿using UnityEngine;
using UnityEngine.UI;
using TETCSharpClient;
using TETCSharpClient.Data;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;
using System;

// Should be placed on a UI object to move around and find what we are looking at 
// and for how long

public class LookedAtFurniture : MonoBehaviour, IGazeListener {

    private bool _dontTrace = true, _dontWork = false;
    private float _endTime = 0.0f,
        _startTime = 0.0f,
        _tmpTime = 0.0f;
    private RectTransform _rectTrans;
    private RaycastHit rayhit = new RaycastHit();
    //private Vector3 _forward = Vector3.zero;
    private GameObject _oldObj = null,
        _objToSave = null,
        _currentObject = null;
    private Dictionary<GameObject, float> _objsLookedAtDictTime;
    private Dictionary<GameObject, int> _objsLookedAtDictCount;
    private Dictionary<QuestItemScript, int> _questsLookedAtDictCount;
    private string _timeStamp;

    private void Start() {
        // Get time stamp for file name
        _timeStamp = string.Format("{0:yyyy-MM-dd HH.mm.ss}", DateTime.Now);
        //activate C# TET client, default port
        GazeManager.Instance.Activate
        (
            GazeManager.ApiVersion.VERSION_1_0,
            GazeManager.ClientMode.Push
        );

        //register for gaze updates
        GazeManager.Instance.AddGazeListener(this);

        _rectTrans = transform.GetComponent<RectTransform>();

        // The canvas is spawned before all the objects in the PCG level, so that function will be called when all has been placed.
        if (SceneManager.GetActiveScene().name == "scene") {
            StartFindingObjects();
        }
    }

    private void Update() {
        Vector3 tmpTarget = GazeDataValidator.Instance.GetLastValidSmoothedUnityGazeCoordinate();
        float d = Vector3.Distance(_rectTrans.position, tmpTarget);
        _rectTrans.position = Vector3.MoveTowards(_rectTrans.position, tmpTarget, d * 2f);
    }

    void FixedUpdate() {
        if (_dontWork)
            return;

        // if a quest item is on, don't trace, they trace themselves
        if (_questsLookedAtDictCount != null)
            foreach (QuestItemScript qis in _questsLookedAtDictCount.Keys) {
                if (qis != null) {
                    if (qis.CheckImage()) {
                        _dontTrace = true;
                        break;
                    } else {
                        _dontTrace = false;
                    }
                }
            }

        if (_dontTrace)
            return;

        Ray mRay = Camera.main.ScreenPointToRay(transform.position);
        //Debug.DrawRay(mRay.origin, mRay.direction * 100, Color.red);

        if (Physics.Raycast(mRay, out rayhit, 20f)) {
            _currentObject = rayhit.collider.gameObject;
            //Debug.Log(_currentObject.name); ;
            if (_currentObject != null) {
                if (_currentObject != _oldObj) {
                    _endTime = Time.time; // Old object's end time
                    _tmpTime = _startTime; // Old object's start time
                    _startTime = Time.time; // New object's start time

                    _objToSave = _oldObj;
                    _oldObj = _currentObject;

                    if (_objToSave) {
                        if (_objsLookedAtDictTime.ContainsKey(_objToSave)) {
                            if (_endTime - _tmpTime > 0.5f) { // Lower margin for time looking at 
                                _objsLookedAtDictTime[_objToSave] += _endTime - _tmpTime;
                                _objsLookedAtDictCount[_objToSave] += 1;
                                Debug.Log("Saving time: " + _objsLookedAtDictTime[_objToSave] + " of GO: " + _objToSave);
                            }
                        }
                    }
                }
            }
        }

    }

    public void StartFindingObjects() {
        _objsLookedAtDictTime = new Dictionary<GameObject, float>();
        _objsLookedAtDictCount = new Dictionary<GameObject, int>();
        _questsLookedAtDictCount = new Dictionary<QuestItemScript, int>();
        GameObject[] objs = FindObjectsOfType<GameObject>();

        foreach (var obj in objs) {
            QuestItemScript qis = obj.GetComponent<QuestItemScript>();
            if (qis != null) {
                _questsLookedAtDictCount.Add(qis, 0);
            } else {
                _objsLookedAtDictTime.Add(obj, 0.0f);
                _objsLookedAtDictCount.Add(obj, 0);
            }
        }
        //Debug.Log(_objsLookedAtDictTime.Count);
        //Debug.Log(_questsLookedAtDictTime.Count);
        _dontTrace = false;
    }

    //private void OnApplicationQuit() {
    //    GazeManager.Instance.RemoveGazeListener(this);
    //    GazeManager.Instance.Deactivate();

    //    Savecsv();
    //}

    public void Quiting() {
        _dontWork = true;
        GazeManager.Instance.RemoveGazeListener(this);
        GazeManager.Instance.Deactivate();
        Savecsv();
    }

    public void OnGazeUpdate(GazeData gazeData) {
        //Add frame to GazeData cache handler
        GazeDataValidator.Instance.Update(gazeData);
    }


    void Savecsv() {
        string filePath = Application.persistentDataPath + "/SavedFiles/LookedAtFurniture For "
            + SceneManager.GetActiveScene().name + " "
            + _timeStamp + ".csv";
        string delimiter = ",";

        string[][] output = new string[_objsLookedAtDictCount.Count + 1 + _questsLookedAtDictCount.Count][]; // +1 for the header

        // Header of csv file
        output[0] = new string[] { "Name", "Time", "Count", "POSX", "POSY", "POSZ", "ROTX", "ROTY", "ROTZ", "ROTW" };

        int _index = 0;
        foreach (var key in _objsLookedAtDictCount.Keys) {

            if (key == null)
                continue;

            string objName = key.name;
            // Skip specific objects
            if (objName.Contains("Character") || objName.Contains("Controller") || objName.Contains("ImageEyeTracker") || objName.Contains("Shelf")
                || objName.Contains("candleStick") || objName.Contains("Point light") || objName.Contains("TutorialPaper") || objName.Contains("QuestItemHolder")
                || objName.Contains("BlackBorderText") || objName.Contains("ImageCrosshair") || objName.Contains("BlackBorderTop") || objName.Contains("BlackBorderBottom")
                || objName.Contains("Text") || objName.Contains("ImageLoadingScreen") || objName.Contains("Canvas") || objName.Contains("Spawner")
                || objName.Contains("GameManager") || objName.Contains("FireWood") || objName.Contains("EventSystem") || objName.Contains("particle")
                || objName.Contains("SP1") || objName.Contains("SP2") || objName.Contains("SP3") || objName.Contains("SPD") || objName.Contains("SPBK")
                || objName.Contains("Particle") || objName.Contains("SPFP") || objName.Contains("SPA") || objName.Contains("spoon") || objName.Contains("fork") || objName.Contains("knife")
                || objName.Contains("wall") || objName.Contains("Wall") || objName.Contains("door") || objName.Contains("Raycaster") || objName.Contains("audio")
                || objName.Contains("bookA") || objName.Contains("bookB") || objName.Contains("bookD") || objName.Contains("bookStackBlueStanding")) {
                continue;
            }

            objName = objName.TrimEnd(' '); // Remove space from the end

            if (objName == "bookC" || objName == "TableCloth" || objName == "PlatesStack" || objName == "ClothForArmoire" || objName == "PlateWithCloth") // objects already in list as single
                continue;

            float tmpTime = _objsLookedAtDictTime[key];
            int tmpCount = _objsLookedAtDictCount[key];

            string Position = key.transform.position.x + "," + key.transform.position.y + "," + key.transform.position.z;
            string Rotation = key.transform.rotation.x + "," + key.transform.rotation.y + "," + key.transform.rotation.z + "," + key.transform.rotation.w;

            output[_index + 1] = new string[] { objName, tmpTime.ToString(), tmpCount.ToString(), Position, Rotation };
            _index++;
        }

        // Add the quest time and count
        foreach (QuestItemScript qis in _questsLookedAtDictCount.Keys) {

            if (qis == null)
                continue;

            string tmpName = qis.name;
            float tmpTime = qis.GetTimeRead();
            int tmpCount = qis.GetCountRead();

            string Position = qis.transform.position.x + "," + qis.transform.position.y + "," + qis.transform.position.z;
            string Rotation = qis.transform.rotation.x + "," + qis.transform.rotation.y + "," + qis.transform.rotation.z + "," + qis.transform.rotation.w;

            output[_index + 1] = new string[] { tmpName, tmpTime.ToString(), tmpCount.ToString(), Position, Rotation };
            _index++;
        }

        int length = output.GetLength(0);
        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++) {
            if (output[index] != null)
                sb.AppendLine(string.Join(delimiter, output[index]));
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}
