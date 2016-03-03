using UnityEngine;

/*
    This script is placed on each quest paper and adds the paper's correct answer to the list of true numbers.
*/
public class QuestItemScript : MonoBehaviour {
    public int questNumber;
    public bool alreadyAdded;

    private string _char = null;

    private void Start() {
        if (!alreadyAdded) {
            alreadyAdded = true;
            if (questNumber != -1) NumberDialScript.trueNumbers.Add(questNumber);
        }
    }

    private void FindCharName() {
        _char = gameObject.name.Remove(0, 5).Remove(1);
        if (_char.Length != 1)
            Debug.LogWarning("Wrong name for: " + gameObject.name);
    }

    public string GetNameChar() {
        if (_char == null)
            FindCharName();
        return _char;
    }
}