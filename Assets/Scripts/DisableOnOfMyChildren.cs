using UnityEngine;
using System.Collections.Generic;

public class DisableOnOfMyChildren : MonoBehaviour {

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

        int random = Random.Range(-1, goList.Count - 1);
        if (random > -1)
            goList[random].gameObject.SetActive(false);
    }
}
