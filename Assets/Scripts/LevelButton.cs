using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour {

    public void LoadLevelByName(string tmpName) {
        SceneManager.LoadScene(tmpName);
    }
}
