using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;


/*
    Used to save all the object's pos&rot to load the level
*/

public class SaveObjectsCoordinates : MonoBehaviour {

    private string _timeStamp;

    //If the application is trying to shut down, we save the game state and allow it to continue to shut down.
    //void OnApplicationQuit() {
    //    if (enabled)
    //        Savecsv();
    //}

    public void Quiting() {
        if (SceneManager.GetActiveScene().name == "PCG")
            Savecsv();
    }

    void Start() {
        // Get time stamp for file name
        _timeStamp = string.Format("{0:yyyy-MM-dd HH.mm.ss}", DateTime.Now);
    }

    void Savecsv() {
        string filePath = Application.persistentDataPath + "/SavedFiles/AllObjects " + _timeStamp + ".csv";
        string delimiter = ",";

        GameObject[] objs = FindObjectsOfType<GameObject>();

        string[][] output = new string[objs.Length + 1][]; // +1 for the header

        // Header of csv file
        output[0] = new string[] { "Name", "Position", "", "", "Rotation" };

        for (int i = 0; i < objs.Length; i++) {

            // Skip specific objects
            string name = objs[i].name;
            if (name.Contains("Character") || name.Contains("Controller") || name.Contains("ImageEyeTracker") || name.Contains("Shelf")
                || name.Contains("TutorialPaper") || name.Contains("QuestItemHolder") || name.Contains("BlackBorderText") || name.Contains("ImageCrosshair")
                || name.Contains("BlackBorderTop") || name.Contains("BlackBorderBottom") || name.Contains("Text") || name.Contains("ImageLoadingScreen")
                || name.Contains("Canvas") || name.Contains("Spawner") || name.Contains("GameManager") || name.Contains("EventSystem")
                || name.Contains("SP1") || name.Contains("SP2") || name.Contains("SP3") || name.Contains("SPD") || name.Contains("SPBK")
                || name.Contains("SPFP") || name.Contains("SPA") || name.Contains("wall") || name.Contains("Wall") || name.Contains("door")) {
                continue;
            }

            int index = name.IndexOf("("); // cut everything after (
            if (index > 0)
                name = name.Substring(0, index);

            ClickableFurniture cf = objs[i].GetComponent<ClickableFurniture>();
            if (cf != null) {
                QuestItemScript qis = cf.questItemAttached;
                if (qis != null) {

                    string qname = qis.name;
                    int c = qname.IndexOf("("); // cut everything after (
                    if (c > 0)
                        qname = qname.Substring(0, c);

                    name = name + "_" + qname;
                }
            }

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