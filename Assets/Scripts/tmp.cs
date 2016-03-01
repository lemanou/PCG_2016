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
    */
}