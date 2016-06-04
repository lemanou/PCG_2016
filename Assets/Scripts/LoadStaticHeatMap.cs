using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadStaticHeatMap : MonoBehaviour {

    private string[] _csvFile; // Reference of CSV file
    private Shader _shader;
    private GameObject[] _objectsList;
    private bool _found;
    private char lineSeperator = '\n'; // It defines line seperate character
    private char fieldSeperator = ','; // It defines field seperate character

    void Start() {
        if (SceneManager.GetActiveScene().name.Contains("scene")) {
            StartColoring();
        }
    }

    public void StartColoring() {

        string filePath = Application.persistentDataPath + "/SavedFiles/LookedAtFurniture For "
            + SceneManager.GetActiveScene().name + ".csv";
        _csvFile = File.ReadAllText(filePath).Split(lineSeperator);

        if (_csvFile == null) {
            Debug.LogWarning("Missing HeatMap CSV file. Should be named as: LookedAtFurniture For {SCENENAME} 2.csv  ~ Without the spaces and {}");
            return;
        }

        _shader = Shader.Find("Unlit/Color");

        _objectsList = FindObjectsOfType<GameObject>();
        foreach (var item in _objectsList) {
            ColorAll(item, Color.black);
        }

        ReadCsvAndColor();
    }

    // Read data from CSV file
    private void ReadCsvAndColor() {
        float avgTime = 0;
        int counter = 0;
        string[] records = _csvFile;

        foreach (string record in records) {
            string[] columns = record.Split(fieldSeperator);
            if (columns.Length == 10) {
                // skip header and objects
                if (columns[0] == "Name" || columns[0].Contains("Quest") || columns[0].Contains("Tutorial"))
                    continue;

                float timeCounted = float.Parse(columns[1]);
                if (timeCounted == 0)
                    continue;

                avgTime += timeCounted;
                counter++;
            }
        }

        avgTime = avgTime / counter;
        avgTime = avgTime + avgTime / 2; // estimate of starring at furniture

        foreach (string record in records) {
            //Debug.Log("NewRow");
            string[] fields = record.Split(fieldSeperator);

            if (fields[0] == "Name")
                continue; // skip header

            if (fields.Length == 10) {
                float timeMeasured = float.Parse(fields[1]);
                if (timeMeasured == 0)
                    continue;

                string csvObjName = fields[0];

                float posx = float.Parse(fields[3]);
                float posy = float.Parse(fields[4]);
                float posz = float.Parse(fields[5]);
                Vector3 pos = new Vector3(posx, posy, posz);

                _found = false;
                foreach (var obj in _objectsList) {
                    if (obj.name == csvObjName & obj.gameObject.transform.position == pos) {
                        //Debug.Log("Found object in array, " + name);
                        _found = true;

                        float redness = 0.0f;
                        float blueness = redness;
                        float greeness = redness;
                        float objTime = timeMeasured / avgTime;

                        if (objTime > 1) objTime = 1;

                        // Crazy coloring from heatmap paper
                        if (objTime <= 0.25) {
                            redness = 0;
                            greeness = objTime * 4;
                            blueness = 1;
                        } else if (objTime <= 0.5) {
                            redness = 0;
                            greeness = 1;
                            blueness = 1 - (objTime - 0.25f) * 4;
                        } else if (objTime <= 0.75) {
                            redness = 1 - (objTime - 0.5f) * 4;
                            greeness = 1;
                            blueness = 0;
                        } else {
                            redness = 1;
                            greeness = 1 - (objTime - 0.75f) * 4;
                            blueness = 0;
                        }

                        Color c = new Color(redness, greeness, blueness);
                        ColorAll(obj, c);
                        //Debug.Log("Average:" + avgTime + " objColor: " + c + " name: " + obj.name);
                        break;
                    }
                }
                if (_found == false && !csvObjName.Contains("Quest") && !csvObjName.Contains("Tutorial")) {
                    Debug.LogWarning("Didn't find obj, " + csvObjName);
                }
            }

        }
    }

    private void ColorAll(GameObject obj, Color c) {
        Renderer r = obj.GetComponent<Renderer>();
        if (r == null) return;
        var mat = r.materials;
        foreach (var m in mat) {
            if (m.name != "Glass") {
                m.shader = _shader;
                m.color = c;
            }
        }
    }
}
