using UnityEngine;
//using UnityEngine.SceneManagement;

/*
    This script is placed on each paper to make it a quest item.
    It also allows us to ask it what story the paper is part of, by returning a single char.
*/
public class QuestItemScript : MonoBehaviour {
    public int questNumber;
    //public bool alreadyAdded;

    private string _char = null;

    //private void Awake() {  // This must be Awake and not Start, to make sure that the list is created before NumberDialScript use the list on Start.
    //    if (SceneManager.GetActiveScene().name != "PCG")
    //        AddQuestNumber(); // in PCG scene the script shall call this
    //}

    //public void AddQuestNumber() {
    //    if (!alreadyAdded) {
    //        alreadyAdded = true;
    //        if (questNumber != -1) {
    //            NumberDialScript nds = FindObjectOfType<NumberDialScript>();
    //            if (nds != null)
    //                nds.trueNumbers.Add(questNumber);
    //        }
    //    }
    //}

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