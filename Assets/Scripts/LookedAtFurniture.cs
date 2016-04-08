using UnityEngine;
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

    private bool _dontTrace = true;
    private float _endTime = 0.0f,
        _startTime = 0.0f,
        _tmpTime = 0.0f;
    private RectTransform _rectTrans;
    private RaycastHit rayhit = new RaycastHit();
    private Vector3 _forward = Vector3.zero;
    private GameObject _oldObj = null,
        _objToSave = null,
        _currentObject = null;
    private Dictionary<GameObject, float> _objsLookedAtDictTime;
    private Dictionary<GameObject, int> _objsLookedAtDictCount;
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
        _rectTrans.position = Vector3.MoveTowards(_rectTrans.position, tmpTarget, d * 1.5f);
    }

    void FixedUpdate() {
        if (_dontTrace)
            return;

        _forward = Camera.main.transform.forward;
        Vector3 screenToWorldVector = Camera.main.ScreenToWorldPoint(transform.position);
        //Debug.DrawRay(screenToWorldVector, _forward * 20, Color.green);

        Ray mRay = new Ray(screenToWorldVector, _forward);

        if (Physics.Raycast(mRay, out rayhit, 20f)) {
            _currentObject = rayhit.collider.gameObject;
            //Debug.Log(_currentObject.name);;
            if (_currentObject != null) {
                if (_currentObject != _oldObj) {
                    _endTime = Time.time; // Old object's end time
                    _tmpTime = _startTime; // Old object's start time
                    _startTime = Time.time; // New object's start time

                    _objToSave = _oldObj;
                    _oldObj = _currentObject;

                    if (_objToSave) {
                        if (_objsLookedAtDictTime.ContainsKey(_objToSave)) {
                            if (_endTime - _tmpTime > 0.5f) { // Lower margin for time looking at furniture
                                _objsLookedAtDictTime[_objToSave] += _endTime - _tmpTime;
                                _objsLookedAtDictCount[_objToSave] += 1;
                                //Debug.Log("Saving time: " + _objsLookedAtDictTime[_objToSave] + " of GO: " + _objToSave);
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
        GameObject[] objs = FindObjectsOfType<GameObject>();

        foreach (var obj in objs) {
            _objsLookedAtDictTime.Add(obj, 0.0f);
            _objsLookedAtDictCount.Add(obj, 0);
        }
        //Debug.Log(_objsLookedAtDictTime.Count);
        _dontTrace = false;
    }

    //private void OnApplicationQuit() {
    //    GazeManager.Instance.RemoveGazeListener(this);
    //    GazeManager.Instance.Deactivate();

    //    Savecsv();
    //}

    public void Quiting() {
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

        string[][] output = new string[_objsLookedAtDictCount.Count + 1][]; // +1 for the header

        // Header of csv file
        output[0] = new string[] { "Name", "Time", "Count", "POSX", "POSY", "POSZ", "ROTX", "ROTY", "ROTZ", "ROTW" };

        int _index = 0;
        foreach (var key in _objsLookedAtDictCount.Keys) {

            if (key == null)
                continue;

            string tmpName = key.name;
            float tmpTime = _objsLookedAtDictTime[key];
            int tmpCount = _objsLookedAtDictCount[key];

            output[_index + 1] = new string[] { tmpName, tmpTime.ToString(), tmpCount.ToString(), key.transform.position.ToString(), key.transform.rotation.ToString() };
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
