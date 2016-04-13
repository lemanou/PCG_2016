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
            // Skip specific objects
            if (tmpName.Contains("Character") || tmpName.Contains("Controller") || tmpName.Contains("ImageEyeTracker") || tmpName.Contains("Shelf")
                || tmpName.Contains("candleStick") || tmpName.Contains("Point light") || tmpName.Contains("TutorialPaper") || tmpName.Contains("QuestItemHolder")
                || tmpName.Contains("BlackBorderText") || tmpName.Contains("ImageCrosshair") || tmpName.Contains("BlackBorderTop") || tmpName.Contains("BlackBorderBottom")
                || tmpName.Contains("Text") || tmpName.Contains("ImageLoadingScreen") || tmpName.Contains("Canvas") || tmpName.Contains("Spawner")
                || tmpName.Contains("GameManager") || tmpName.Contains("FireWood") || tmpName.Contains("EventSystem") || tmpName.Contains("particle")
                || tmpName.Contains("SP1") || tmpName.Contains("SP2") || tmpName.Contains("SP3") || tmpName.Contains("SPD") || tmpName.Contains("SPBK")
                || tmpName.Contains("Particle") || tmpName.Contains("SPFP") || tmpName.Contains("SPA") || tmpName.Contains("spoon") || tmpName.Contains("fork") || tmpName.Contains("knife")
                || tmpName.Contains("wall") || tmpName.Contains("Wall") || tmpName.Contains("door") || tmpName.Contains("Raycaster") || tmpName.Contains("audio")
                || tmpName.Contains("bookA") || tmpName.Contains("bookB") || tmpName.Contains("bookC") || tmpName.Contains("bookD") || tmpName.Contains("bookStackBlueStanding")) {
                continue;
            }

            if (tmpName == "TableCloth")
                continue;

            float tmpTime = _objsLookedAtDictTime[key];
            int tmpCount = _objsLookedAtDictCount[key];

            string Position = key.transform.position.x + "," + key.transform.position.y + "," + key.transform.position.z;
            string Rotation = key.transform.rotation.x + "," + key.transform.rotation.y + "," + key.transform.rotation.z + "," + key.transform.rotation.w;

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
