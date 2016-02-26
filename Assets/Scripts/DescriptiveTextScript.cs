using UnityEngine;
using UnityEngine.UI;

public class DescriptiveTextScript : MonoBehaviour
{
    Text BlackBorderText;

    private GameObject currentGO;

    void Start()
    {
        BlackBorderText = GetComponent<Text>();
    }

    void FixedUpdate()
    {
        if (RayFromCrosshair.GOHitByRay != null)
        {
            BlackBorderText.text = RayFromCrosshair.GOHitByRay.text;

            if (Input.GetMouseButtonDown(0))
            {
                currentGO = RayFromCrosshair.GOHitByRay.gameObject;
                if (RayFromCrosshair.GOHitByRay.gotQuestItem != null && !ClickableFurniture.questItemAlreadyFound)
                {
                    ClickableFurniture.questItemAlreadyFound = true;
                }
                else if (RayFromCrosshair.GOHitByRay.gotQuestItem == null)
                {
                    ClickableFurniture.questItemNotFound = true;
                }
            }
            if (ClickableFurniture.questItemAlreadyFound)
            {
                BlackBorderText.text = "You have found a hidden note.";
            }
            else if (ClickableFurniture.questItemNotFound)
            {
                BlackBorderText.text = "You have found nothing of interest.";
            }
        }
        else
        {
            ClickableFurniture.questItemAlreadyFound = false;
            ClickableFurniture.questItemNotFound = false;
            BlackBorderText.text = "";
        }
        if (RayFromCrosshair.GOHitByRay != null && currentGO != null && currentGO != RayFromCrosshair.GOHitByRay.gameObject)
        {
            ClickableFurniture.questItemAlreadyFound = false;
            ClickableFurniture.questItemNotFound = false;
        }


        // For testing purposes only.
        if (Input.GetKeyDown("space"))
        {
            ClickableFurniture.questItemAlreadyFound = false;
            ClickableFurniture.questItemNotFound = false;
            print("quest item reset");
        }
    }
}