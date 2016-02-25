using UnityEngine;
using UnityEngine.UI;

public class ClickableFurniture : MonoBehaviour
{
    public GameObject questItem;
    public Text DescriptiveText;
    
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        for (int i = 0; i < _renderer.materials.Length; i++)
        {
            _renderer.materials[i].color = Color.white;
        }
    }

    void FixedUpdate()
    {
        if (RayFromCrosshair.GOHitByRay == this.gameObject)
        {
            for (int i = 0; i < _renderer.materials.Length; i++)
            {
                _renderer.materials[i].color = new Color(1.5f, 1.5f, 1.5f);
            }
        }
        else
        {
            for (int i = 0; i < _renderer.materials.Length; i++)
            {
                _renderer.materials[i].color = Color.white;
            }
        }
    }
}