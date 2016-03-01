using UnityEngine;
using UnityEngine.UI;

public class tmp : MonoBehaviour
{

    /*  PSEUDO


        ::find out state::
            if click
                if state = foundHiddenNote
                    hide paper
                        if hovering furniture
                            state == normalDescription
                        else
                            state == empty
                else if hovering furniture
                    if quest
                        state = foundHiddenNote
                        show paper
                    else
                        state = nothingOfInterest
            else
                if hovering furniture
                    currentGO = this
                    if paper already shown
                        keep state (state = foundHiddenNote)
                    else
                        if furniture != same
                            state == normalDescription
                else
                    if paper already shown
                        keep state (state = foundHiddenNote)
                    else
                        state == empty
        ::show correct description::
        if state empty...
        if state normalDescription...
        if state foundHiddenNote...
        if state nothingOfInterest...


    if mouse down
        if found paper already
            remove paper gameObject
            if hovering
                show normal description
            else
                show empty description
        else if hovering something
            set currentGO
            if paper not found, but there is quest
                set quest
                show paper
            else if no quest
                set no quest
    else
        if hovering something
            if you haven't clicked already
                show normal description
            else if you're hovering new furniture
                show normal description
        else
            if not show paper
                show empty










    */
}