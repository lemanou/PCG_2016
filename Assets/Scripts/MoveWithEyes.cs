using UnityEngine;
using TETCSharpClient;
using TETCSharpClient.Data;

public class MoveWithEyes : MonoBehaviour, IGazeListener {

    private RectTransform rTrans;

    private void Start() {
        //activate C# TET client, default port
        GazeManager.Instance.Activate
        (
            GazeManager.ApiVersion.VERSION_1_0,
            GazeManager.ClientMode.Push
        );

        //register for gaze updates
        GazeManager.Instance.AddGazeListener(this);

        rTrans = transform.GetComponent<RectTransform>();
    }

    private void Update() {
        rTrans.position = GetGazeScreenPosition();
    }

    private void OnApplicationQuit() {
        GazeManager.Instance.RemoveGazeListener(this);
        GazeManager.Instance.Deactivate();
    }

    private Vector3 GetGazeScreenPosition() {
        Point2D gp = GazeDataValidator.Instance.GetLastValidSmoothedGazeCoordinates();

        if (null != gp) {
            Point2D sp = UnityGazeUtils.GetGazeCoordsToUnityWindowCoords(gp);
            return new Vector3((float)sp.X, (float)sp.Y, 0f);
        } else
            return Vector3.zero;
    }

    public void OnGazeUpdate(GazeData gazeData) {
        //Add frame to GazeData cache handler
        GazeDataValidator.Instance.Update(gazeData);
    }
}
