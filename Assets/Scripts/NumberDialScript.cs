using UnityEngine;
using System.Collections.Generic;

public class NumberDialScript : MonoBehaviour
{
    public GameObject dial1, dial2, dial3;
    public static List<QuestItemScript> trueNumbers = new List<QuestItemScript>();

    Quaternion dialRotationZero;

    private List<GameObject> dials = new List<GameObject>();
    private List<int> dialNumbers = new List<int>();

    //private int questNumber1, questNumber2, questNumber3;
    private int dialNumber1, dialNumber2, dialNumber3;

    void Start()
    {
        dials.Add(dial1);
        dials.Add(dial2);
        dials.Add(dial3);
        dialRotationZero = dial1.transform.rotation;
        dialNumbers.Add(dialNumber1);
        dialNumbers.Add(dialNumber2);
        dialNumbers.Add(dialNumber3);
    }

    void Update()
    {
        print("dial roation= " + dialRotationZero);
        if (Input.GetKeyDown("1"))
        {
            Quaternion rot = dials[0].transform.rotation;
            dials[0].transform.rotation = dialRotationZero;
            dials[0].transform.rotation = rot * Quaternion.Euler(-1 * 36, 0, 0);
        }
        if (Input.GetKeyDown("2"))
        {
            Quaternion rot = dials[1].transform.rotation;
            dials[1].transform.rotation = dialRotationZero;
            dials[1].transform.rotation = rot * Quaternion.Euler(-1 * 36, 0, 0);
        }
        if (Input.GetKeyDown("3"))
        {
            Quaternion rot = dials[2].transform.rotation;
            dials[2].transform.rotation = dialRotationZero;
            dials[2].transform.rotation = rot * Quaternion.Euler(-1 * 36, 0, 0);
        }
        for (int i = 0; i < dials.Count; i++)
        {
            if (dials[i].transform.rotation.x == 0)
            {
                dialNumbers[i] = 0;
            }
            else if (dials[i].transform.rotation.x == -36)
            {
                dialNumbers[i] = 1;
            }
            else if (dials[i].transform.rotation.x == -72)
            {
                dialNumbers[i] = 2;
            }
            else if (dials[i].transform.rotation.x == -108)
            {
                dialNumbers[i] = 3;
            }
            else if (dials[i].transform.rotation.x == -144)
            {
                dialNumbers[i] = 4;
            }
            else if (dials[i].transform.rotation.x == -180)
            {
                dialNumbers[i] = 5;
            }
            else if (dials[i].transform.rotation.x == -216)
            {
                dialNumbers[i] = 6;
            }
            else if (dials[i].transform.rotation.x == -252)
            {
                dialNumbers[i] = 7;
            }
            else if (dials[i].transform.rotation.x == -288)
            {
                dialNumbers[i] = 8;
            }
            else if (dials[i].transform.rotation.x == -324)
            {
                dialNumbers[i] = 9;
            }
        }
        for (int i = 0; i < dialNumbers.Count; i++)
        {
            //compare dialNumbers to trueNumbers

        }
    }
}