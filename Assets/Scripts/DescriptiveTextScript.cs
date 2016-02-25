using UnityEngine;
using UnityEngine.UI;

public class DescriptiveTextScript : MonoBehaviour
{
    Text BlackBorderText;

    void Start()
    {
        BlackBorderText = GetComponent<Text>();
    }
    
    void Update()
    {
        BlackBorderText.text = "no text";

        if (RayFromCrosshair.GOHitByRay != null)
        {
            if (RayFromCrosshair.GOHitByRay.GetComponent<ClickableFurniture>() != null)
            {
                BlackBorderText.text = RayFromCrosshair.GOHitByRay.GetComponent<ClickableFurniture>().DescriptiveText.text;
            }
        }
    }
}