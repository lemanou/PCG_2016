using UnityEngine;

/*
    This script is placed on furniture that is supposed to be interacted with.
    It allows furniture to have a description and to hold a quest item.
    And in case of the door, instead of a quest item, it holds the number dial object.
*/
public class ClickableFurniture : MonoBehaviour
{
    public static bool questItemFound, questItemNotFound;   // We do need both, since we have 3 states: not clicked, not found and found
    public QuestItemScript questItemAttached;
    public GameObject numberDialAttached;
    public string text;
    
    /* If the renderer and all comments are uncommented, furniture will light up when hovered.
       However the result was not desirable.*/

    //private Renderer _renderer;

    void Start()
    {
        /*_renderer = GetComponent<Renderer>();
        for (int i = 0; i < _renderer.materials.Length; i++)
        {
            _renderer.materials[i].color = Color.white;
        */
    }

    void FixedUpdate()
    {
        /*if (RayFromCrosshair.GOHitByRay == this.gameObject && _renderer != null)
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
        }*/
    }
}