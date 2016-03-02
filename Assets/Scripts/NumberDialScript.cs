using UnityEngine;
using System.Collections.Generic;

/*
    This script is placed on the number dial gameobject.
    Is allows the player to turn the individual dials and compares the current numbers to the list of true numbers.
*/
public class NumberDialScript : MonoBehaviour
{
    public GameObject
        dialObject1,
        dialObject2,
        dialObject3;
    public static List<int> trueNumbers = new List<int>();

    Quaternion dialRotationZero;

    private List<GameObject> dialObjects = new List<GameObject>();
    private List<int> dialNumbers = new List<int>();
    private List<int> availableNumbers = new List<int>();
    private List<int> takenNumbers = new List<int>();

    private int correctDialNumbers;

    void Start()
    {
        dialObjects.Add(dialObject1);
        dialObjects.Add(dialObject2);
        dialObjects.Add(dialObject3);
        dialRotationZero = dialObject1.transform.rotation;
        dialNumbers.Add(0);
        dialNumbers.Add(0);
        dialNumbers.Add(0);
        availableNumbers.Add(trueNumbers[0]);
        availableNumbers.Add(trueNumbers[1]);
        availableNumbers.Add(trueNumbers[2]);
    }

    void Update()
    {
        // Starting at 0, incrementing the dial's value by 1, for each 36 degrees turned.
        // We turn each dial individually.
        if (Input.GetKeyDown("1"))
        {
            Quaternion rot = dialObjects[0].transform.rotation;
            dialObjects[0].transform.rotation = dialRotationZero;
            dialObjects[0].transform.rotation = rot * Quaternion.Euler(-1 * 36, 0, 0);
            if (dialNumbers[0] == 9) dialNumbers[0] = 0;
            else dialNumbers[0]++;
        }
        if (Input.GetKeyDown("2"))
        {
            Quaternion rot = dialObjects[1].transform.rotation;
            dialObjects[1].transform.rotation = dialRotationZero;
            dialObjects[1].transform.rotation = rot * Quaternion.Euler(-1 * 36, 0, 0);
            if (dialNumbers[1] == 9) dialNumbers[1] = 0;
            else dialNumbers[1]++;
        }
        if (Input.GetKeyDown("3"))
        {
            Quaternion rot = dialObjects[2].transform.rotation;
            dialObjects[2].transform.rotation = dialRotationZero;
            dialObjects[2].transform.rotation = rot * Quaternion.Euler(-1 * 36, 0, 0);
            if (dialNumbers[2] == 9) dialNumbers[2] = 0;
            else dialNumbers[2]++;
        }

        // We reset the available numbers each frame, so the taken numbers get reset to the current dials.
        for (int i = 0; i < trueNumbers.Count; i++)
        {
            availableNumbers[i] = trueNumbers[i];
        }
        correctDialNumbers = 0;

        // We compare dialNumbers to availableNumbers
        for (int i = 0; i < dialNumbers.Count;)
        {
            for (int j = 0; j < availableNumbers.Count;)
            {
                if (dialNumbers[i] == availableNumbers[j])
                {
                    availableNumbers[j] = -1;
                    correctDialNumbers++;
                    j = availableNumbers.Count;
                }
                else j++;
            }
            i++;
        }
        if (correctDialNumbers == 3)
        {
            DescriptiveTextScript.currentState = DescriptiveTextScript.State.completed;
        }
    }
}