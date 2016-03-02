using UnityEngine;
using System.Collections.Generic;

public class DisableOnOfMyChildren : MonoBehaviour {

    public int numToDisable = 1;

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

        if (numToDisable > count)
            numToDisable = count;

        for (int i = 0; i < numToDisable;) {
            int random = Random.Range(-1, goList.Count - 1);
            if (random > -1) {
                if (goList[random].gameObject.activeSelf) {
                    goList[random].gameObject.SetActive(false);
                    i++;
                }
            }
        }
    }
}
