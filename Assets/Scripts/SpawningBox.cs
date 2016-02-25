﻿using System.Collections.Generic;
using UnityEngine;

public class SpawningBox : MonoBehaviour {

    [HideInInspector]
    public IntVector2 LocalCoordinates;

    public enum BoxCondition {
        Free,
        Tall,
        Short,
        Occupied
    }

    public enum BoxLocation {
        North,
        East,
        South,
        West,
        Middle,
        NotSet
    }

    public BoxLocation _boxloc = BoxLocation.NotSet;
    public BoxCondition _boxcond = BoxCondition.Free;
    public GameObject _father = null;
    private List<string> OccupiedBoxes = new List<string>(new string[] { "Spawned Box 0, 1", "Spawned Box 0, 2", "Spawned Box 0, 3", "Spawned Box 0, 4",
                                                                        "Spawned Box 0, 5", "Spawned Box 0, 6", "Spawned Box 7, 3", "Spawned Box 7, 4" });
    private void Start() {
        if (OccupiedBoxes.Contains(gameObject.name)) {
            _boxcond = BoxCondition.Occupied;
            _father = gameObject;
            GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().material.color = Color.red;
        }

        if (gameObject.name.Contains(", 0")) {
            _boxloc = BoxLocation.South;
        } else if (gameObject.name.Contains(", 7")) {
            _boxloc = BoxLocation.North;
        } else if (gameObject.name.Contains("7, ")) {
            _boxloc = BoxLocation.East;
        } else if (gameObject.name.Contains("0, ")) {
            _boxloc = BoxLocation.West;
        } else {
            _boxloc = BoxLocation.Middle;
        }

    }

    public BoxCondition GetBoxCondition() {
        return _boxcond;
    }

    public BoxLocation GetBoxLocation() {
        if (_boxloc == BoxLocation.NotSet)
            Debug.LogWarning("Racing issue for box: " + gameObject.name);
        return _boxloc;
    }

    public GameObject GetFather() {
        return _father;
    }

    public void SetColBox(SpawnableObject sObj) {

        if (_boxcond == BoxCondition.Occupied)
            return;

        if (_father == null) {
            if (sObj.localTag == SpawnableObject.Tag.Short)
                _boxcond = BoxCondition.Short;
            else if (sObj.localTag == SpawnableObject.Tag.Tall)
                _boxcond = BoxCondition.Tall;

            _father = sObj.gameObject;
        } else {
            //Debug.Log(gameObject.name + " ~ Cannot set box, used by: " + Father);
        }
        GetComponent<Renderer>().enabled = true;
        GetComponent<Renderer>().material.color = Color.white;
    }

    public void ReSetColBox() {
        _boxcond = BoxCondition.Free;
        _father = null;
        GetComponent<Renderer>().enabled = false;
    }
}