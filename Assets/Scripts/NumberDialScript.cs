using UnityEngine;
using System.Collections.Generic;

public class NumberDialScript : MonoBehaviour
{
    public GameObject dial1, dial2, dial3;

    Quaternion dialRotationZero;
    private List<GameObject> dials = new List<GameObject>();

    void Start()
    {
        dials.Add(dial1);
        dials.Add(dial2);
        dials.Add(dial3);
        dialRotationZero = dial1.transform.rotation;
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
    }
}