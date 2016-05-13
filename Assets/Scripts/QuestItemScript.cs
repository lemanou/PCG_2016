using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

/*
    This script is placed on each paper to make it a quest item.
    It also allows us to ask it what story the paper is part of, by returning a single char.
    It also counts up how much time the quest Image was on, as we assume that the player is looking at the image when it is on
*/
public class QuestItemScript : MonoBehaviour {
    public int questNumber;

    private FirstPersonController _fpc;
    private bool _once = false;
    private string _char = null;
    private float _timeread, _timereading = 0.0f;
    private int _countread = 0;

    public float GetTimeRead() {
        return _timeread;
    }

    public int GetCountRead() {
        return _countread;
    }

    public bool CheckImage() {
        Image img = GetComponent<Image>();
        if (img.enabled)
            return true;

        return false;
    }

    private void Start() {
        _fpc = FindObjectOfType<FirstPersonController>();
        //Debug.Log("FPS: " + _fpc);
    }

    private void Update() {
        if (SceneManager.GetActiveScene().name == "PCG" && gameObject.name == "TutorialPaper") {
            gameObject.SetActive(false);
            return;
        }


        if (_fpc == null)
            _fpc = FindObjectOfType<FirstPersonController>();

        if (_fpc != null)
            if (CheckImage()) {
                if (_fpc.GetRun())
                    _fpc.SetOff(gameObject.name);
                _timereading += Time.deltaTime;
                if (!_once)
                    _once = true;
                // Debug.Log(_timereading);
            } else {
                if (!_fpc.GetRun())
                    _fpc.SetOn(gameObject.name);
                _timeread += _timereading;
                if (_once) {
                    _once = false;
                    _countread += 1;
                }
                // if (_timeread > 0)
                // Debug.Log(gameObject.name + ": " + _timeread + ", times: " + _countread);
                _timereading = 0;
            }
    }

    private void FindCharName() {
        _char = gameObject.name.Remove(0, 5).Remove(1);
        if (_char.Length != 1)
            Debug.LogWarning("Wrong name for: " + gameObject.name);
    }

    public string GetNameChar() {
        if (_char == null)
            FindCharName();
        return _char;
    }
}