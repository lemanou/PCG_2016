using UnityEngine;

public class SpawningBox : MonoBehaviour {

    [HideInInspector]
    public IntVector2 LocalCoordinates;

    public enum BoxCondition {
        Free,
        Tall,
        Short
    }

    public BoxCondition bc = BoxCondition.Free;

    public string Father = "none";

    public void SetColBox(SpawnableObject sObj) {
        if (Father == "none") {
            if (sObj.localTag == SpawnableObject.Tag.Short)
                bc = BoxCondition.Short;
            else if (sObj.localTag == SpawnableObject.Tag.Tall)
                bc = BoxCondition.Tall;

            Father = sObj.name;
        }
        GetComponent<Renderer>().enabled = true;
    }

    public void ReSetColBox() {
        bc = BoxCondition.Free;
        Father = "none";
        GetComponent<Renderer>().enabled = false;
    }

    //private void OnTriggerEnter(Collider col) {
    //    SpawnableObject sObj = col.GetComponent<SpawnableObject>();
    //    if (sObj && !sObj.currentTriggerBoxes.Contains(this)) {
    //        sObj.currentTriggerBoxes.Add(this);
    //        SetColBox(sObj);
    //    }
    //}

    //private void OnTriggerStay(Collider col) {
    //    SpawnableObject sObj = col.GetComponent<SpawnableObject>();
    //    if (sObj) {
    //        GetComponent<Renderer>().enabled = true;
    //        //GetComponent<Renderer>().material.color = sObj.GetComponent<Renderer>().material.color;
    //    }
    //}

    //private void OnTriggerExit(Collider col) {
    //    SpawnableObject sObj = col.GetComponent<SpawnableObject>();
    //    if (sObj) {
    //        sObj.currentTriggerBoxes.Remove(this);
    //        ReSetColBox();
    //    }
    //}
}
