using UnityEngine;
using TETCSharpClient;
using TETCSharpClient.Data;

public class MoveWithEyes : MonoBehaviour, IGazeListener {

    private RectTransform _rTrans;
    private Vector3 _lastPos = Vector3.zero;

    private void Start() {
        //activate C# TET client, default port
        GazeManager.Instance.Activate
        (
            GazeManager.ApiVersion.VERSION_1_0,
            GazeManager.ClientMode.Push
        );

        //register for gaze updates
        GazeManager.Instance.AddGazeListener(this);

        _rTrans = transform.GetComponent<RectTransform>();
    }

    private void Update() {
        _rTrans.position = GetGazeScreenPosition();
    }

    private void OnApplicationQuit() {
        GazeManager.Instance.RemoveGazeListener(this);
        GazeManager.Instance.Deactivate();
    }

    private Vector3 GetGazeScreenPosition() {
        Point2D gp = GazeDataValidator.Instance.GetLastValidSmoothedGazeCoordinates();

        if (gp != null) {
            Point2D sp = UnityGazeUtils.GetGazeCoordsToUnityWindowCoords(gp);
            _lastPos = new Vector3((float)sp.X, (float)sp.Y, 0f);
        }
        return _lastPos;
    }

    public void OnGazeUpdate(GazeData gazeData) {
        //Add frame to GazeData cache handler
        GazeDataValidator.Instance.Update(gazeData);
    }
}
