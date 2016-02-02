using UnityEngine;

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
        StartCoroutine(_spawnerInstance.Spawn());
    }

    public void RestartGame() {
        StopAllCoroutines();
        _spawnerInstance.Reset();
        BeginGame();
    }
}
