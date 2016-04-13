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
            string objName = objs[i].name;
            if (objName.Contains("Character") || objName.Contains("Controller") || objName.Contains("ImageEyeTracker") || objName.Contains("Shelf")
                || objName.Contains("candleStick") || objName.Contains("Point light") || objName.Contains("TutorialPaper") || objName.Contains("QuestItemHolder")
                || objName.Contains("BlackBorderText") || objName.Contains("ImageCrosshair") || objName.Contains("BlackBorderTop") || objName.Contains("BlackBorderBottom")
                || objName.Contains("Text") || objName.Contains("ImageLoadingScreen") || objName.Contains("Canvas") || objName.Contains("Spawner")
                || objName.Contains("GameManager") || objName.Contains("FireWood") || objName.Contains("EventSystem") || objName.Contains("particle")
                || objName.Contains("SP1") || objName.Contains("SP2") || objName.Contains("SP3") || objName.Contains("SPD") || objName.Contains("SPBK")
                || objName.Contains("Particle") || objName.Contains("SPFP") || objName.Contains("SPA") || objName.Contains("spoon") || objName.Contains("fork") || objName.Contains("knife")
                || objName.Contains("wall") || objName.Contains("Wall") || objName.Contains("door") || objName.Contains("Raycaster") || objName.Contains("audio")
                || objName.Contains("bookA") || objName.Contains("bookB") || objName.Contains("bookC") || objName.Contains("bookD") || objName.Contains("bookStackBlueStanding")) {
                continue;
            }

            if (objName == "TableCloth")
                continue;

            int index = objName.IndexOf("("); // cut everything after (
            if (index > 0)
                objName = objName.Substring(0, index);

            ClickableFurniture cf = objs[i].GetComponent<ClickableFurniture>();
            if (cf != null) {
                QuestItemScript qis = cf.questItemAttached;
                if (qis != null) {

                    string qname = qis.name;
                    int c = qname.IndexOf("("); // cut everything after (
                    if (c > 0)
                        qname = qname.Substring(0, c);

                    objName = objName + "_" + qname;
                }
            }

            string Position = objs[i].transform.position.x + "," + objs[i].transform.position.y + "," + objs[i].transform.position.z;
            string Rotation = objs[i].transform.rotation.x + "," + objs[i].transform.rotation.y + "," + objs[i].transform.rotation.z + "," + objs[i].transform.rotation.w;

            output[i + 1] = new string[] { objName, Position, Rotation };
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