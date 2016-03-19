using UnityEngine;
using TETCSharpClient;
using TETCSharpClient.Data;

public class BlinksDetector : MonoBehaviour, IGazeListener {

    private float _startTime = 0.0f,
        _blinkTime = 0.0f;
    private int _gazeState;

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

        if (_gazeState == 1 || _gazeState == 2 || _gazeState == 4 || _gazeState == 7) { // STATE_TRACKING_GAZE = 1; STATE_TRACKING_EYES = 2; STATE_TRACKING_PRESENCE = 4; STATE_TRACKING = 7
            _startTime = Time.time;
        } else if (_gazeState == 8 || _gazeState == 16) { // STATE_TRACKING_FAIL = 8; STATE_TRACKING_LOST = 16;
            _blinkTime = Time.time - _startTime;

            if (_blinkTime > 0.0f && _blinkTime < 0.08f)
                Debug.Log("Normal Blink");
            else if (_blinkTime > 0.08f)
                Debug.Log("Long Blink");
        }
    }

    private void OnApplicationQuit() {
        GazeManager.Instance.RemoveGazeListener(this);
        GazeManager.Instance.Deactivate();
    }

    public void OnGazeUpdate(GazeData gazeData) {
        _gazeState = gazeData.State;
    }
}
