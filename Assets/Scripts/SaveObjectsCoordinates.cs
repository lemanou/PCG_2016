using UnityEngine;
using System;
using System.IO;
using System.Text;

public class SaveObjectsCoordinates : MonoBehaviour {

    private string _timeStamp;

    // If the application is trying to shut down, we save the game state and allow it to continue to shut down.
    void OnApplicationQuit() {
        Savecsv();
    }

    void Start() {
        // Get time stamp for file name
        _timeStamp = string.Format("{0:yyyy-MM-dd HH.mm.ss}", DateTime.Now);
    }

    void Savecsv() {
        string filePath = Application.dataPath + "/SavedFiles/Furniture " + _timeStamp + ".csv";
        string delimiter = ",";

        GameObject[] objs = FindObjectsOfType<GameObject>();

        string[][] output = new string[objs.Length + 1][]; // +1 for the header

        // Header of csv file
        output[0] = new string[] { "Name", "Position", "", "", "Rotation" };

        for (int i = 0; i < objs.Length; i++) {

            string name = objs[i].name;
            string Position = objs[i].transform.position.ToString();
            string Rotation = objs[i].transform.rotation.ToString();

            output[i + 1] = new string[] { name, Position, Rotation };

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