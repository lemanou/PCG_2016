using UnityEngine;
using System.Collections;

public class SpawningBox : MonoBehaviour {

    [HideInInspector]
    public IntVector2 LocalCoordinates;

    public enum BoxCondition {
        Free,
        Tall,
        Short
    }

    public BoxCondition bc = BoxCondition.Free;
}
