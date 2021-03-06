﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/*
    This script is placed on the BlackBorderText and allows for descriptive subtitles to pop up.
    The pop up is depending on states.
    It also shows the corresponding quest paper, if we click a furniture with one attached.
    Because we are already making use of states; When the player finds the correct code,
    this script will know when to send us back to the main menu, and will do so.
    This script will therefore also take care of turning the cursor in/visible.
*/
public class DescriptiveTextScript : MonoBehaviour {
    Text BlackBorderText;
    public GameObject tutorialPaper;
    private GameObject currentGO, currentQuestItemGO;
    private GameObject currentDialNumberGO;
    private float lastSuccessfulMouseClick, mouseDelay;
    private bool
        dialNumberShown,
        proceedToRestart;

    public enum State {
        empty,
        normalDescription,
        foundHiddenNote,
        nothingOfInterest,
        completed
    }

    public static State currentState = State.foundHiddenNote;

    public State CurrentState {
        get { return currentState; }
        set { currentState = value; }
    }

    void Awake() {
        BlackBorderText = GetComponent<Text>();
        currentGO = null;
        if (SceneManager.GetActiveScene().name != "SelectionMenu") {
            tutorialPaper.SetActive(true);
            currentQuestItemGO = tutorialPaper;
            currentState = State.foundHiddenNote;
            Cursor.visible = false;
        } else {
            currentState = State.empty;
            tutorialPaper.SetActive(false);
            Cursor.visible = true;
        }
        proceedToRestart = false;
        mouseDelay = 0.2f;
    }

    void Update() {
        if (currentState != State.completed) {
            // If we click
            if (Input.GetMouseButtonDown(0)) {
                if (Time.time < lastSuccessfulMouseClick + mouseDelay) {
                    return;
                }
                lastSuccessfulMouseClick = Time.time;

                // Removing the visible dialNumber.
                if (currentDialNumberGO != null) {
                    if (currentDialNumberGO.activeSelf) {
                        currentDialNumberGO.SetActive(false);
                        currentDialNumberGO = null;
                    }
                }

                // Removing the visible paper.
                if (currentState == State.foundHiddenNote) {
                    currentQuestItemGO.GetComponent<Image>().enabled = false;
                    currentQuestItemGO = null;

                    if (RayFromCrosshair.GOHitByRay != null) {
                        currentState = State.normalDescription;
                    } else {
                        currentState = State.empty;
                    }
                }

                // If we hit anything, show a descriptive text.
                else if (RayFromCrosshair.GOHitByRay != null) {
                    currentGO = RayFromCrosshair.GOHitByRay.gameObject;

                    if (RayFromCrosshair.GOHitByRay.numberDialAttached != null) {
                        if (!RayFromCrosshair.GOHitByRay.numberDialAttached.activeSelf && !dialNumberShown) {
                            RayFromCrosshair.GOHitByRay.numberDialAttached.SetActive(true);
                            currentDialNumberGO = RayFromCrosshair.GOHitByRay.numberDialAttached;
                            dialNumberShown = true;
                        } else {
                            dialNumberShown = false;
                        }
                    }

                    // Has the player clicked furniture with a quest item attached?
                    if (RayFromCrosshair.GOHitByRay.questItemAttached != null) {
                        currentQuestItemGO = RayFromCrosshair.GOHitByRay.questItemAttached.gameObject;
                        currentState = State.foundHiddenNote;
                        currentQuestItemGO.GetComponent<Image>().enabled = true;
                    } else if (RayFromCrosshair.GOHitByRay.questItemAttached == null) {
                        currentState = State.nothingOfInterest;
                    }
                }
            }
            // If we do not click
            else {
                // We're hovering something
                if (RayFromCrosshair.GOHitByRay != null) {
                    if (currentState != State.foundHiddenNote && currentState != State.nothingOfInterest) {
                        currentState = State.normalDescription;
                        // If we go directly from hitting one to hitting another, reset any quest item text, to allow for descriptive text.  
                    } else if (currentState != State.foundHiddenNote && (RayFromCrosshair.GOHitByRay != null && currentGO != null && currentGO != RayFromCrosshair.GOHitByRay.gameObject)) {
                        currentState = State.normalDescription;
                    }
                }
                // We're not hovering anything
                else {
                    if (currentState != State.foundHiddenNote) {
                        currentState = State.empty;
                    }
                }
            }
        }
        // Set the descriptive text.
        if (currentState == State.foundHiddenNote) {
            if (currentQuestItemGO == tutorialPaper) BlackBorderText.text = "Hints will be written here. Press the left mouse button when you have finished reading the paper.";
            else BlackBorderText.text = "You have found a hidden note.";
        }
        if (currentState == State.nothingOfInterest) {
            if (currentDialNumberGO == null) {
                BlackBorderText.text = "You have found nothing of interest.";
            } else {
                BlackBorderText.text = "Enter the correct three digits to unlock the door.";
            }
        }
        if (currentState == State.normalDescription) {
            if (RayFromCrosshair.GOHitByRay != null) {
                BlackBorderText.text = RayFromCrosshair.GOHitByRay.text;
            }
        }
        if (currentState == State.empty) {
            currentGO = null;
            BlackBorderText.text = "";
        }
        if (currentState == State.completed) {
            BlackBorderText.text = "Congratulations! You have managed to find the hidden code!";
            if (!proceedToRestart) {
                proceedToRestart = true;
                StartCoroutine(ReturnToMenu(4.0f));
            }
        }
    }

    IEnumerator ReturnToMenu(float waitTime) {
        // Call recorders to save files
        LookedAtFurniture taf = FindObjectOfType<LookedAtFurniture>();
        if (taf != null)
            taf.Quiting();
        SaveGazesToCSV sgtc = FindObjectOfType<SaveGazesToCSV>();
        if (sgtc != null)
            sgtc.Quiting();
        BlinksDetector bd = FindObjectOfType<BlinksDetector>();
        if (bd != null)
            bd.Quiting();
        SaveObjectsCoordinates soc = FindObjectOfType<SaveObjectsCoordinates>();
        if (soc != null)
            soc.Quiting();

        yield return new WaitForSeconds(waitTime);
        Cursor.visible = true;
        SceneManager.LoadScene(0);
    }
}