using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using TETCSharpClient;
using TETCSharpClient.Data;
/*
    Activating and deactivating the GazeListener, that works with the eye-tracker.
    We save the recorded data to a csv-file.
    We decide what data to include and write it to the file.
*/
public class Save : MonoBehaviour, IGazeListener {

    // Used to save the gazeData from the game

    private string _timeStamp;
    private List<GazeData> _gd;

    void Start() {
        BeginSavin();
    }

    // Implementing IGazeListener
    public void OnGazeUpdate(GazeData gazeData) {
        //Add frame to GazeData cache handler
        _gd.Add(gazeData);
    }

    // If the application is trying to shut down, we save the game state and allow it to continue to shut down.
    void OnApplicationQuit() {

        // RemoveListener and Deactivate GM
        GazeManager.Instance.RemoveGazeListener(this);
        GazeManager.Instance.Deactivate();

        Savecsv();
    }

    void BeginSavin() {
        // Get time stamp for file name
        _timeStamp = string.Format("{0:yyyy-MM-dd HH.mm.ss}", DateTime.Now);
        // Initialize the list to hold the gaze data
        _gd = new List<GazeData>();
        // activate C# TET client, default port
        GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);
        // register for gaze updates
        GazeManager.Instance.AddGazeListener(this);
    }

    void Savecsv() {
        string filePath = Application.dataPath + "/SavedFiles/" + _timeStamp + ".csv";
        string delimiter = ",";

        string[][] output = new string[_gd.Count + 1][]; // +1 for the header

        // Header of csv file
        output[0] = new string[] { "Head Orientation", "","", "Left Raw", "", "Left Smoothed", "", "Right Raw", "", "Right Smoothed", "",
                        "Combined Raw", "","Combined Smoothed", "", "Is Fixated", "State", "TimeStamp"};

        for (int i = 0; i < _gd.Count; i++) {

            // Head
            Head head = _gd[i].Head;
            string tmpHeadLoc = head.Orientation.ToString();
            
            // Left Eye
            Eye le = _gd[i].LeftEye;
            string tmpLeftRaw = le.RawCoordinates.ToString();
            string tmpLeftSmooth = le.SmoothedCoordinates.ToString();

            // Right Eye
            Eye re = _gd[i].RightEye;
            string tmpRightRaw = re.RawCoordinates.ToString();
            string tmpRightSmooth = re.SmoothedCoordinates.ToString();

            // Combined
            string tmpRaw = _gd[i].RawCoordinates.ToString();
            string tmpSmooth = _gd[i].SmoothedCoordinates.ToString();

            string tmpIsFixated = _gd[i].IsFixated.ToString();
            string tmpState = _gd[i].State.ToString();
            //string tmpTimeStamp = _gd[i].TimeStamp.ToString(); // dont need the long time stamp
            string tmpTimeStampString = _gd[i].TimeStampString.ToString();

            output[i + 1] = new string[] { tmpHeadLoc, tmpLeftRaw, tmpLeftSmooth, tmpRightRaw, tmpRightSmooth,
                                    tmpRaw, tmpSmooth, tmpIsFixated, tmpState, tmpTimeStampString };

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