using UnityEngine;
using System.Collections.Generic;

public class DisableAllChildrenChance : MonoBehaviour {

    public int percentToDisable = 10;

    void Start() {
        List<GameObject> goList = new List<GameObject>();
        var children = gameObject.GetComponentsInChildren<Transform>();
        int count = 0;
        foreach (var child in children) {
            if (child.parent == gameObject.transform) {
                count++;
                goList.Add(child.gameObject);
                //print(child + ": " + gameObject.name);
            }
        }

        int r = Random.Range(0, 100);
        if (r <= percentToDisable)
            goList.ForEach(child => child.SetActive(false));
    }
}
