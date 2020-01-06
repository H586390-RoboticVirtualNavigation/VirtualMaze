using UnityEngine;

public class Poster : MonoBehaviour {
    //for mapping posters to binwalls
    [SerializeField]
    private GameObject _attachedTo = null;
    public GameObject AttachedTo { get => _attachedTo; }

    public static string GetNameOfAttachedTo(GameObject obj) {
        Poster poster = obj.GetComponent<Poster>();
        if (poster != null) {
            return poster._attachedTo.name;
        }
        else {
            return null;
        }
    }
}
