using UnityEngine;
using TETCSharpClient;
using TETCSharpClient.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine.SceneManagement;

public class BlinksDetector : MonoBehaviour, IGazeListener {

    private GazeData _gazeData = null;
    private string _timeStamp;
    private float _startTime = 0.0f,
        _blinkTime = 0.0f;
    private int _gazeState,
        _framesWithValidPosition = 1,
        _count = 1;
    private Point3D _oldValidUserPos,
        _currentValidUserPos;
    private bool readyForNextBlink;
    private string _timeSUnity, _trackerTime;
    private List<Blinks> _blinksList;
    private bool _countBlinks;
    private double _curDistance, _oldDistance;

    private void Start() {
        _countBlinks = true;
        _startTime = Time.time;
        _blinksList = new List<Blinks>();
        _timeStamp = string.Format("{0:yyyy-MM-dd HH.mm.ss}", DateTime.Now);
        //activate C# TET client, default port
        GazeManager.Instance.Activate
        (
            GazeManager.ApiVersion.VERSION_1_0,
            GazeManager.ClientMode.Push
        );

        //register for gaze updates
        GazeManager.Instance.AddGazeListener(this);
    }

    private void Update() {

        if (_countBlinks == false)
            return;
        //Debug.Log(_gazeState);
        _currentValidUserPos = GazeDataValidator.Instance.GetLastValidUserPosition();

        if (_currentValidUserPos == _oldValidUserPos)
            _framesWithValidPosition++;
        else
            _framesWithValidPosition = 1;

        _oldValidUserPos = _currentValidUserPos;

        if (_framesWithValidPosition < 10) { // if the user has not moved for 10 frames

            if (CheckOneClosedEye()) {
                //Debug.LogWarning("One eye closed");
                readyForNextBlink = false;
                return;
            }

            _curDistance = GazeDataValidator.Instance.GetLastValidUserDistance();
            _curDistance = Math.Round(_curDistance, 3); // Keep two decimals
            if (_curDistance != _oldDistance) {
                _count++;
            } else {
                _count = 1;
            }

            _oldDistance = _curDistance;

            if (_count > 30) { // half a second? 
                //Debug.LogWarning("You moving WAAAAY too often, " + "Current Distance= " + _curDistance);
                readyForNextBlink = false;
                return;
            }

            if (CanSeeEyes()) {
                _startTime = Time.time;
                if (!CheckOpenEyes()) {
                   // Debug.Log("One Eye blink?");
                    return;
                }
                if (readyForNextBlink) {
                    //Debug.Log("Time: " + _blinkTime);
                    readyForNextBlink = false;
                    if (_blinkTime > 0.0f && _blinkTime <= 0.5f) {
                        _timeSUnity = Time.time.ToString();
                        Blinks blink = new Blinks("NormalBlink", _trackerTime, _timeSUnity);
                        _blinksList.Add(blink);
                        //Debug.Log("Normal Blink " + _blinksList.Count.ToString());
                    } else if (_blinkTime > 0.5f && _blinkTime <= 2.5f) {
                        _timeSUnity = Time.time.ToString();
                        Blinks blink = new Blinks("LongBlink", _trackerTime, _timeSUnity);
                        _blinksList.Add(blink);
                        //Debug.Log("Long Blink " + _blinksList.Count.ToString());
                    }
                }
            } else if (CheckClosedEyes()) {
                readyForNextBlink = true;
                _blinkTime = Time.time - _startTime;
            }
        } else {
            //Debug.LogWarning("Not Looking at screen");
            readyForNextBlink = false;
        }
    }

    private bool CheckOpenEyes() {
        if (_gazeData == null)
            return false;

        if (_gazeData.RightEye.PupilSize > 0 && _gazeData.RightEye.PupilCenterCoordinates.X > 0 && _gazeData.RightEye.PupilCenterCoordinates.Y > 0 &&
            _gazeData.LeftEye.PupilSize > 0 && _gazeData.LeftEye.PupilCenterCoordinates.X > 0 && _gazeData.LeftEye.PupilCenterCoordinates.Y > 0)
            return true;
        return false;
    }

    private bool CheckOneClosedEye() {
        if (_gazeData == null)
            return false;

        if (_gazeData.RightEye.PupilSize == 0 || _gazeData.RightEye.PupilCenterCoordinates.X == 0 || _gazeData.RightEye.PupilCenterCoordinates.Y == 0 ||
            _gazeData.LeftEye.PupilSize == 0 || _gazeData.LeftEye.PupilCenterCoordinates.X == 0 || _gazeData.LeftEye.PupilCenterCoordinates.Y == 0)
            if (!CheckClosedEyes())
                return true;
        return false;
    }

    private bool CheckClosedEyes() {
        if (_gazeData == null)
            return false;

        if (_gazeData.RightEye.PupilSize == 0 && _gazeData.RightEye.PupilCenterCoordinates.X == 0 && _gazeData.RightEye.PupilCenterCoordinates.Y == 0 &&
            _gazeData.LeftEye.PupilSize == 0 && _gazeData.LeftEye.PupilCenterCoordinates.X == 0 && _gazeData.LeftEye.PupilCenterCoordinates.Y == 0)
            return true;
        return false;
    }

    private bool CanSeeEyes() {
        // STATE_TRACKING_GAZE = 1; STATE_TRACKING_EYES = 2; STATE_TRACKING_PRESENCE = 4; STATE_TRACKING = 7
        if (_gazeState == 7) //{//_gazeState == 1 || _gazeState == 2 || _gazeState == 4 || 
            if (CheckOpenEyes())
                return true;
        //} else if (_gazeState == 8 || _gazeState == 16) {// STATE_TRACKING_FAIL = 8; STATE_TRACKING_LOST = 16;
        //    return false;
        //}
        return false;
    }

    //private void OnApplicationQuit() {
    //    GazeManager.Instance.RemoveGazeListener(this);
    //    GazeManager.Instance.Deactivate();
    //    _countBlinks = false;
    //    Savecsv();
    //}

    public void Quiting() {
        GazeManager.Instance.RemoveGazeListener(this);
        GazeManager.Instance.Deactivate();

        _countBlinks = false;
        Savecsv();
    }

    public void OnGazeUpdate(GazeData gazeData) {
        //Add frame to GazeData cache handler
        GazeDataValidator.Instance.Update(gazeData);
        _gazeData = gazeData;
        _gazeState = gazeData.State;
        _trackerTime = gazeData.TimeStampString;
    }


    void Savecsv() {
        string filePath = Application.persistentDataPath + "/SavedFiles/Blinks For "
            + SceneManager.GetActiveScene().name + " "
            + _timeStamp + ".csv";
        string delimiter = ",";

        string[][] output = new string[_blinksList.Count + 1][]; // +1 for the header

        // Header of csv file
        output[0] = new string[] { "BlinkType", "TimeTracker" }; // , "TimeUnity"

        for (int i = 0; i < _blinksList.Count; i++) {

            output[i + 1] = new string[] { _blinksList[i].GetBlinkType(),
                _blinksList[i].GetTrackerTime()}; // , _blinksList[i].GetUnityTime()
        }

        int length = output.GetLength(0);
        StringBuilder sb = new StringBuilder();

        for (int index = 0; index < length; index++) {
            if (output[index] != null)
                sb.AppendLine(string.Join(delimiter, output[index]));
        }

        File.WriteAllText(filePath, sb.ToString());
    }

    private class Blinks {
        private string type, trackerTime, unityTime;

        public string GetBlinkType() { return type; }
        public string GetTrackerTime() { return trackerTime; }
        public string GetUnityTime() { return unityTime; }

        public Blinks(string type, string trackerTime, string unityTime) {
            this.type = type;
            this.trackerTime = trackerTime;
            this.unityTime = unityTime;
        }
    }
}
