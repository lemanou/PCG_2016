using UnityEngine;
using TETCSharpClient;
using TETCSharpClient.Data;
using System;

public class BlinksDetector : MonoBehaviour, IGazeListener {

    private float _startTime = 0.0f,
        _blinkTime = 0.0f;
    private int _gazeState,
        _count = 1;
    private Point3D _oldValidUserPos,
        _currentValidUserPos;
    private bool readyForNextBlink;

    private void Start() {
        _startTime = Time.time;

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
        //Debug.Log(_gazeState);
        _currentValidUserPos = GazeDataValidator.Instance.GetLastValidUserPosition();

        if (_currentValidUserPos == _oldValidUserPos)
            _count++;
        else
            _count = 1;

        _oldValidUserPos = _currentValidUserPos;

        if (_count < 10) {
            if (CanSeeEyes()) {
                _startTime = Time.time;

                //Debug.Log("Time: " + _blinkTime);
                if (readyForNextBlink) {
                    readyForNextBlink = false;
                    if (_blinkTime > 0.0f && _blinkTime <= 0.5f)
                        Debug.Log("Normal Blink");
                    else if (_blinkTime > 0.5f && _blinkTime <= 2.5f)
                        Debug.Log("Long Blink");
                }

            } else if (!CanSeeEyes()) {
                readyForNextBlink = true;
                _blinkTime = Time.time - _startTime;
            }
        } else {
            Debug.Log("Not Looking at screen");
        }
    }

    private bool CanSeeEyes() {
        // STATE_TRACKING_GAZE = 1; STATE_TRACKING_EYES = 2; STATE_TRACKING_PRESENCE = 4; STATE_TRACKING = 7
        if (_gazeState == 7) //_gazeState == 1 || _gazeState == 2 || _gazeState == 4 || 
            return true;
        else if (_gazeState == 8 || _gazeState == 16) // STATE_TRACKING_FAIL = 8; STATE_TRACKING_LOST = 16;
            return false;
        return false;
    }

    private void OnApplicationQuit() {
        GazeManager.Instance.RemoveGazeListener(this);
        GazeManager.Instance.Deactivate();
    }

    public void OnGazeUpdate(GazeData gazeData) {
        _gazeState = gazeData.State;
        //Add frame to GazeData cache handler
        GazeDataValidator.Instance.Update(gazeData);
    }
}
