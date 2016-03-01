using UnityEngine;
using UnityEngine.UI;

/*
    This script is placed on BlackBorderText and allows for descriptive subtitles to pop up.
*/
public class DescriptiveTextScript : MonoBehaviour
{
    Text BlackBorderText;

    private GameObject currentGO, currentQuestItemGO;

    private enum State
    {
        empty,
        normalDescription,
        foundHiddenNote,
        nothingOfInterest
    }

    private State currentState = State.empty;

    private State CurrentState
    {
        get { return currentState; }
        set { currentState = value; }
    }

    void Start()
    {
        BlackBorderText = GetComponent<Text>();
        currentGO = currentQuestItemGO = null;
    }

    void FixedUpdate()
    {
        print("state: " + currentState);
        // If we're clicking
        if (Input.GetMouseButtonDown(0))
        {
            //print("mouse pressed");
            // If a quest item is shown already, use the mouse click to deactivate it.
            if (currentState == State.foundHiddenNote)
            {
                //print("already had paper");
                currentQuestItemGO.SetActive(false);
                currentQuestItemGO = null;

                if (RayFromCrosshair.GOHitByRay != null)
                {
                    //print("hovering furniture after paper was removed");
                    currentState = State.normalDescription;
                }
                else
                {
                    //print("not hovering furniture after paper was removed");
                    currentState = State.empty;
                }
            }
            // If we hit anything, show a descriptive text.
            else if (RayFromCrosshair.GOHitByRay != null)
            {
                //print("mouse pressed while hovering");
                currentGO = RayFromCrosshair.GOHitByRay.gameObject;
                // Has the player clicked furniture with a quest item attached?
                if (RayFromCrosshair.GOHitByRay.questItemAttached != null && currentState != State.foundHiddenNote)
                {
                    //print("found quest item");
                    currentQuestItemGO = RayFromCrosshair.GOHitByRay.questItemAttached;
                    currentState = State.foundHiddenNote;
                    currentQuestItemGO.SetActive(true);
                }
                else if (RayFromCrosshair.GOHitByRay.questItemAttached == null)
                {
                    //print("found no quest item");
                    currentState = State.nothingOfInterest;
                }
            }
        }
        // If we're not clicking
        else
        {
            // We're hovering something
            if (RayFromCrosshair.GOHitByRay != null)
            {
                //print("hovering something");
                if (currentState != State.foundHiddenNote || currentState != State.nothingOfInterest)
                {
                    //print("hovering something without having checked for quest item");
                    currentState = State.normalDescription;
                    // If we go directly from hitting one to hitting another, reset any quest item text, to allow for descriptive text.  
                }
                if (RayFromCrosshair.GOHitByRay != null && currentGO != null && currentGO != RayFromCrosshair.GOHitByRay.gameObject)
                {
                    //print("changing hover from one item directly to the next");
                    currentState = State.normalDescription;
                }
            }
            // We're not hovering anything
            else
            {
                //print("not hovering anything");
                if (currentState != State.foundHiddenNote)
                {
                    currentState = State.empty;
                }
            }
        }
        // Set the descriptive text.
        if (currentState == State.foundHiddenNote)
        {
            BlackBorderText.text = "You have found a hidden note.";
        }
        if (currentState == State.nothingOfInterest)
        {
            BlackBorderText.text = "You have found nothing of interest.";
        }
        if (currentState == State.normalDescription)
        {
            if (RayFromCrosshair.GOHitByRay != null)
            {
                BlackBorderText.text = RayFromCrosshair.GOHitByRay.text;
            }
        }
        if (currentState == State.empty)
        {
            BlackBorderText.text = "";
        }
    }
}