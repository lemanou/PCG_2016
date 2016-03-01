using UnityEngine;

public class QuestItemScript : MonoBehaviour
{
    public int questNumber;

    void Start()
    {
        if(questNumber != -1) NumberDialScript.trueNumbers.Add(this);
    }
}