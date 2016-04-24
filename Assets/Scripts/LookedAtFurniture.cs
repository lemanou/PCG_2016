using UnityEngine;
using TETCSharpClient;
using TETCSharpClient.Data;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;
using System;
using UnityStandardAssets.Characters.FirstPerson;

// Should be placed on a UI object to move around and find what we are looking at 
// and for how long

public class LookedAtFurniture : MonoBehaviour, IGazeListener {

    private FirstPersonController _fpc;
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
    private List<string> _dhmdlist;
    private string _timeStamp, _trackerTime, _newTimeStamp, _oldTimeStamp;
    private DynamicHeatMapData _dhmdA, _dhmdB;

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
        _fpc = FindObjectOfType<FirstPersonController>();
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
        //if (_questsLookedAtDictCount != null)
        //    foreach (QuestItemScript qis in _questsLookedAtDictCount.Keys) {
        //        if (qis != null) {
        //            if (qis.CheckImage()) {
        //                _dontTrace = true;
        //                break;
        //            } else {
        //                _dontTrace = false;
        //            }
        //        }
        //    }

        //if (_dontTrace)
        //    return;

        Ray mRay = Camera.main.ScreenPointToRay(transform.position);
        //Debug.DrawRay(mRay.origin, mRay.direction * 100, Color.red);

        if (Physics.Raycast(mRay, out rayhit, 20f)) {
            _currentObject = rayhit.collider.gameObject;
            //Debug.Log(_currentObject.name); ;
            if (_currentObject != null) {
                if (_currentObject != _oldObj) {

                    if (_dhmdA.GetSwapper()) {
                        _dhmdA.SetStartTime(_trackerTime);
                        _dhmdB.SetEndTime(_trackerTime);
                        _dhmdB.SetPlayer(_fpc.gameObject);
                        _dhmdB.SetFurnitureName(_currentObject.name);
                    } else {
                        _dhmdB.SetStartTime(_trackerTime);
                        _dhmdA.SetEndTime(_trackerTime);
                        _dhmdA.SetPlayer(_fpc.gameObject);
                        _dhmdA.SetFurnitureName(_currentObject.name);
                    }

                    _endTime = Time.time; // Old object's end time
                    _tmpTime = _startTime; // Old object's start time
                    _startTime = Time.time; // New object's start time

                    _objToSave = _oldObj;
                    _oldObj = _currentObject;

                    if (_objToSave) {
                        if (_objsLookedAtDictTime.ContainsKey(_objToSave)) {
                            if (_endTime - _tmpTime > 0.5f) { // Lower margin for time looking at 

                                if (!_dhmdA.GetSwapper()) {
                                    //_dhmdA.PrintData("A");
                                    string _dm = FormatString(_dhmdA);
                                    _dhmdlist.Add(_dm);
                                } else {
                                    //_dhmdB.PrintData("B");
                                    string _dm = FormatString(_dhmdB);
                                    _dhmdlist.Add(_dm);
                                }
                                _dhmdB.SetSwapper(!_dhmdB.GetSwapper());
                                _dhmdA.SetSwapper(!_dhmdA.GetSwapper());

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

    private string FormatString(DynamicHeatMapData tmpdhmd) {
        string returnedString;

        GameObject _player = tmpdhmd.GetPlayer();
        string Position = _player.transform.position.x + "," + _player.transform.position.y + "," + _player.transform.position.z;
        string Rotation = _player.transform.rotation.x + "," + _player.transform.rotation.y + "," + _player.transform.rotation.z + "," + _player.transform.rotation.w;

        returnedString = tmpdhmd.GetFurnitureName() + "," + tmpdhmd.GetStartTime() + "," + tmpdhmd.GetEndTime() + "," + Position + "," + Rotation;

        return returnedString;
    }

    public void StartFindingObjects() {
        _dhmdA = new DynamicHeatMapData();
        _dhmdA.SetSwapper(true);
        _dhmdB = new DynamicHeatMapData();
        _dhmdB.SetSwapper(false);
        _dhmdB.SetStartTime(_trackerTime);
        _dhmdlist = new List<string>();

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
        SaveDynamicDataToCsv();
    }

    public void OnGazeUpdate(GazeData gazeData) {
        //Add frame to GazeData cache handler
        GazeDataValidator.Instance.Update(gazeData);
        _trackerTime = gazeData.TimeStampString;
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

    void SaveDynamicDataToCsv() {
        string filePath = Application.persistentDataPath + "/SavedFiles/LookedAtFurnitureDynamic For "
            + SceneManager.GetActiveScene().name + " "
            + _timeStamp + ".csv";
        string delimiter = ",";

        string[][] output = new string[_dhmdlist.Count + 1][]; // +1 for the header

        // Header of csv file
        output[0] = new string[] { "Name", "StartTime", "EndTime", "PlayerPOSX", "PlayerPOSY", "PlayerPOSZ", "PlayerROTX", "PlayerROTY", "PlayerROTZ", "PlayerROTW" };

        int _index = 0;
        foreach (var dobj in _dhmdlist) {
            output[_index + 1] = new string[] { dobj };
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

    public class DynamicHeatMapData {

        protected GameObject _player;
        protected string _furnitureName,
            _startTimeStamp, _endTimeStamp;
        protected bool _swapper;

        public DynamicHeatMapData() {
        }

        public DynamicHeatMapData(GameObject p, string fn, string st, string et) {
            _player = p;
            _furnitureName = fn;
            _startTimeStamp = st;
            _endTimeStamp = et;
        }

        public void SetSwapper(bool sw) {
            _swapper = sw;
        }

        public void SetPlayer(GameObject pl) {
            _player = pl;
        }

        public void SetFurnitureName(string fn) {
            _furnitureName = fn;
        }

        public void SetStartTime(string sts) {
            _startTimeStamp = sts;
        }

        public void SetEndTime(string ets) {
            _endTimeStamp = ets;
        }

        public bool GetSwapper() {
            return _swapper;
        }

        public GameObject GetPlayer() {
            return _player;
        }

        public string GetFurnitureName() {
            return _furnitureName;
        }

        public string GetStartTime() {
            return _startTimeStamp;
        }

        public string GetEndTime() {
            return _endTimeStamp;
        }

        public void PrintData(string objs) {
            Debug.Log(objs + " " + _furnitureName + " " + _startTimeStamp + " " + _endTimeStamp + " " + _player.name);
        }
    }
}