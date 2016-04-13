using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BlinksDetector))]
[RequireComponent(typeof(SaveGazesToCSV))]

/*
    This script takes care of initiating a level (it creates the spawner) and resetting the spawners on level restart.
    It also allows us to pop back to the main menu or redo the level.
*/
public class GameManager : MonoBehaviour {

    public Spawner SpawnerPrefab;

    private Spawner _spawnerInstance;

    private void Start() {
        BeginGame();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            RestartGame();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            // Call recorders to save files
            LookedAtFurniture taf = FindObjectOfType<LookedAtFurniture>();
            if (taf != null)
                taf.Quiting();
            SaveGazesToCSV sgtc = GetComponent<SaveGazesToCSV>();
            if (sgtc != null)
                sgtc.Quiting();
            BlinksDetector bd = GetComponent<BlinksDetector>();
            if (bd != null)
                bd.Quiting();
            SaveObjectsCoordinates soc = GetComponent<SaveObjectsCoordinates>();
            if (soc != null)
                soc.Quiting();

            Cursor.visible = true;
            SceneManager.LoadScene("SelectionMenu");
        }
    }

    private void BeginGame() {
        if (SpawnerPrefab != null) {
            _spawnerInstance = Instantiate(SpawnerPrefab) as Spawner;
            _spawnerInstance.Spawn();
        }
    }

    public void RestartGame() {
        //ClearConsole(); // Not needed in build

        if (SpawnerPrefab != null) {
            SpawnObjectsOnMe[] sooma = FindObjectsOfType<SpawnObjectsOnMe>();
            foreach (var item in sooma) {
                item.Reset();
            }

            _spawnerInstance.gameObject.GetComponent<SpawnQuests>().Reset();
            _spawnerInstance.gameObject.GetComponent<SpawnChairs>().Reset();
            _spawnerInstance.gameObject.GetComponent<SpawnWallObjects>().Reset();
            _spawnerInstance.Reset();
            BeginGame();
        } else {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // UsefulShortcuts reflection trick
    //[MenuItem("Tools/Clear Console %#c")] // CMD + SHIFT + C
    static void ClearConsole() {
        // This simply does "LogEntries.Clear()" the long way:
        var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
}