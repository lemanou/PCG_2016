using UnityEngine;
using UnityEngine.UI;

/*
    This script is placed on BlackBorderText and allows for descriptive subtitles to pop up.
*/
public class DescriptiveTextScript : MonoBehaviour
{
    Text BlackBorderText;

    private GameObject currentGO, currentQuestItemGO;
    private GameObject currentDialNumberGO;
    private float lastSuccessfulMouseClick, mouseDelay;
    private bool dialNumberShown;

    public enum State
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
        mouseDelay = 0.5f;
    }

    void Update()
    {
        print("currentDialNumberGO= " + currentDialNumberGO);
        // If we click
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time < lastSuccessfulMouseClick + mouseDelay)
            {
                return;
            }
            lastSuccessfulMouseClick = Time.time;

            // Removing the visible dialNumber.
            if (currentDialNumberGO != null)
            {
                if (currentDialNumberGO.activeSelf)
                {
                    currentDialNumberGO.SetActive(false);
                    currentDialNumberGO = null;
                }
            }

            // Removing the visible paper.
            if (currentState == State.foundHiddenNote)
            {
                currentQuestItemGO.SetActive(false);
                currentQuestItemGO = null;

                if (RayFromCrosshair.GOHitByRay != null)
                {
                    currentState = State.normalDescription;
                }
                else
                {
                    currentState = State.empty;
                }
            }

            // If we hit anything, show a descriptive text.
            else if (RayFromCrosshair.GOHitByRay != null)
            {
                currentGO = RayFromCrosshair.GOHitByRay.gameObject;

                if (RayFromCrosshair.GOHitByRay.numberDialAttached != null)
                {
                    if (!RayFromCrosshair.GOHitByRay.numberDialAttached.activeSelf && !dialNumberShown)
                    {
                        RayFromCrosshair.GOHitByRay.numberDialAttached.SetActive(true);
                        currentDialNumberGO = RayFromCrosshair.GOHitByRay.numberDialAttached;
                        dialNumberShown = true;
                    }
                    else
                    {
                        dialNumberShown = false;
                    }
                }

                // Has the player clicked furniture with a quest item attached?
                if (RayFromCrosshair.GOHitByRay.questItemAttached != null)
                {
                    currentQuestItemGO = RayFromCrosshair.GOHitByRay.questItemAttached.gameObject;
                    currentState = State.foundHiddenNote;
                    currentQuestItemGO.SetActive(true);
                }
                else if (RayFromCrosshair.GOHitByRay.questItemAttached == null)
                {
                    currentState = State.nothingOfInterest;
                }
            }
        }
        // If we do not click
        else
        {
            // We're hovering something
            if (RayFromCrosshair.GOHitByRay != null)
            {
                if (currentState != State.foundHiddenNote && currentState != State.nothingOfInterest)
                {
                    currentState = State.normalDescription;
                    // If we go directly from hitting one to hitting another, reset any quest item text, to allow for descriptive text.  
                }
                else if (currentState != State.foundHiddenNote && (RayFromCrosshair.GOHitByRay != null && currentGO != null && currentGO != RayFromCrosshair.GOHitByRay.gameObject))
                {
                    currentState = State.normalDescription;
                }
            }
            // We're not hovering anything
            else
            {
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
            if (currentDialNumberGO == null)
            {
                BlackBorderText.text = "You have found nothing of interest.";
            }
            else
            {
                BlackBorderText.text = "Enter the correct three digits to unlock the door.";
            }
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
            currentGO = null;
            BlackBorderText.text = "";
        }
    }
}