using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//////////////////////////////////////////////////
//  This script removes the loading screen after 5 seconds. This is made to ensure that the test person sees no difference in "loading time".
//  The loading screen is also used as background for the menu.
//////////////////////////////////////////////////

public class LoadingScreenImage : MonoBehaviour
{
    public Image _loadingImage;
    public Text _loadingText;

    private float _timer;

    void Start()
    {
        _loadingImage.enabled = true;
        if (SceneManager.GetActiveScene().name != "SelectionMenu")
        {
            _loadingText.enabled = true;
        }
        else _loadingText.enabled = false;
    }

    void Update()
    {
        _timer = Time.timeSinceLevelLoad;

        if (SceneManager.GetActiveScene().name != "SelectionMenu")
        {
            if (_loadingImage.enabled == true && _timer > 5)
            {
                _loadingImage.enabled = false;
                _loadingText.enabled = false;
            }
        }
    }
}