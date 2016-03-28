using UnityEngine;
using UnityEngine.SceneManagement;

//////////////////////////////////////////////////
//  This script removes the selection menu screen, if we are not in the selection menu.
//////////////////////////////////////////////////

public class SelectionMenuPanel : MonoBehaviour
{
    public GameObject _panel;

    private float _timer;

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "SelectionMenu")
        {
            if (_panel.activeSelf)
            {
                _panel.SetActive(false);
            }
        }
        else if (SceneManager.GetActiveScene().name == "SelectionMenu")
        {
            _panel.SetActive(true);
        }
    }
}