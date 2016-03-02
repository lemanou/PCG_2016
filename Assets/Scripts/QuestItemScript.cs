using UnityEngine;

/*
    This script is placed on each quest paper and adds the paper's correct answer to the list of true numbers.
*/
public class QuestItemScript : MonoBehaviour
{
    public int questNumber;
    public bool alreadyAdded;

    void Start()
    {
        if (!alreadyAdded)
        {
            alreadyAdded = true;
            if (questNumber != -1) NumberDialScript.trueNumbers.Add(questNumber);
        }
    }
}