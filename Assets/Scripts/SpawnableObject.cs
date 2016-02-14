using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnableObject : MonoBehaviour {

    public enum Tag {
        Short,
        Tall
    }

    public enum Placement {
        NotSet,
        Middle,
        Wall
    }

    public enum Facing {
        North,
        East,
        South,
        West
    }

    public Tag LocalTag = Tag.Short;
    public Placement LocalPlacement = Placement.NotSet;
    public Facing LocalFacing = Facing.North;

    public IntVector2 NeededSpaceSize;
    public int PlacementNumber;

    private List<SpawningBox> currentTriggerBoxes = new List<SpawningBox>();

    public void CorrectPlacement(Vector3 RoomBoundaries) {

        if ((RoomBoundaries.x - 2) == 0 || (RoomBoundaries.z - 2) == 0 || (-RoomBoundaries.x + 2) == 0 || (-RoomBoundaries.z + 2) == 0) {
            Debug.LogWarning("Room too small");
            return;
        }

        //RandomPlacing(RoomBoundaries);

        switch (LocalPlacement) {
            case Placement.Middle:
                PlaceInMidRoom(RoomBoundaries);
                break;
            case Placement.Wall:
                PlaceNearWall(RoomBoundaries);
                break;
            case Placement.NotSet:
                Debug.LogWarning(gameObject.name + ": Placing not set, please check.");
                break;
        }
    }

    private void PlaceInMidRoom(Vector3 RoomBoundaries) {
        float x = 0f, y = 0f, z = 0f;
        Vector3 tmpBounds = GetComponent<Collider>().bounds.size;
        y = tmpBounds.y - tmpBounds.y / 2f;

        switch (LocalFacing) {
            case Facing.North:
                x = UnityEngine.Random.Range(-RoomBoundaries.x + 2, RoomBoundaries.x - 2);
                z = UnityEngine.Random.Range(0, RoomBoundaries.z - 2);
                break;
            case Facing.East:
                x = UnityEngine.Random.Range(0, RoomBoundaries.x - 2);
                z = UnityEngine.Random.Range(-RoomBoundaries.z + 2, RoomBoundaries.z - 2);
                break;
            case Facing.South:
                x = UnityEngine.Random.Range(-RoomBoundaries.x + 2, RoomBoundaries.x - 2);
                z = UnityEngine.Random.Range(-RoomBoundaries.z + 2, 0);
                break;
            case Facing.West:
                x = UnityEngine.Random.Range(-RoomBoundaries.x + 2, 0);
                z = UnityEngine.Random.Range(-RoomBoundaries.z + 2, RoomBoundaries.z - 2);
                break;
        }

        transform.position = new Vector3(x, y, z);

        CheckAllBoxes(RoomBoundaries);

        Debug.Log(gameObject.name + " placed at: " + transform.position);
    }

    private void SetColBox(SpawningBox sbx) {
        if (LocalTag == Tag.Short)
            sbx.bc = SpawningBox.BoxCondition.Short;
        else if (LocalTag == Tag.Tall)
            sbx.bc = SpawningBox.BoxCondition.Tall;

        sbx.GetComponent<Renderer>().enabled = true;
    }

    private void ReSetColBox(SpawningBox sbx) {
        sbx.bc = SpawningBox.BoxCondition.Free;
        sbx.GetComponent<Renderer>().enabled = false;
    }

    private void CheckAllBoxes(Vector3 RoomBoundaries) {
        foreach (var sbx in currentTriggerBoxes) {
            if (sbx.bc != SpawningBox.BoxCondition.Free)
                PlaceInMidRoom(RoomBoundaries); // ToDo Check recursion if works 
        }
    }

    private void PlaceNearWall(Vector3 RoomBoundaries) {
        throw new NotImplementedException();
    }

    private void RandomPlacing(Vector3 RoomBoundaries) {
        Vector3 tmpBounds = GetComponent<Collider>().bounds.size;

        Vector3 tmpPosition =
            new Vector3(UnityEngine.Random.Range(-RoomBoundaries.x, RoomBoundaries.x),
            tmpBounds.y - tmpBounds.y / 2f, UnityEngine.Random.Range(-RoomBoundaries.z, RoomBoundaries.z));

        transform.position = tmpPosition;
        transform.rotation = Quaternion.identity;
    }

    void OnTriggerEnter(Collider col) {
        SpawningBox sbx = col.GetComponent<SpawningBox>();
        if (sbx) {
            currentTriggerBoxes.Add(sbx);
            SetColBox(sbx);
        }
    }

    void OnTriggerStay(Collider col) {
        SpawningBox sbx = col.GetComponent<SpawningBox>();
        if (sbx) {
            if (!currentTriggerBoxes.Contains(sbx))
                currentTriggerBoxes.Add(sbx);
            SetColBox(sbx);
        }
    }

    void OnTriggerExit(Collider col) {
        SpawningBox sbx = col.GetComponent<SpawningBox>();
        if (sbx) {
            currentTriggerBoxes.Remove(sbx);
            ReSetColBox(sbx);
        }
    }
}
