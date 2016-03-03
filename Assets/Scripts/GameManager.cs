using UnityEngine;
//using UnityEditor;

public class GameManager : MonoBehaviour {

    public Spawner SpawnerPrefab;

    private Spawner _spawnerInstance;

    private void Start() {
        BeginGame();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            RestartGame();
        }
    }

    private void BeginGame() {
        _spawnerInstance = Instantiate(SpawnerPrefab) as Spawner;
        _spawnerInstance.Spawn();
    }

    public void RestartGame() {
        ClearConsole();
        
        SpawnObjectsOnMe[] sooma = FindObjectsOfType<SpawnObjectsOnMe>();
        foreach (var item in sooma) {
            item.Reset();
        }

        _spawnerInstance.gameObject.GetComponent<SpawnQuests>().Reset();
        _spawnerInstance.gameObject.GetComponent<SpawnChairs>().Reset();
        _spawnerInstance.gameObject.GetComponent<SpawnWallObjects>().Reset();
        _spawnerInstance.Reset();
        BeginGame();
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