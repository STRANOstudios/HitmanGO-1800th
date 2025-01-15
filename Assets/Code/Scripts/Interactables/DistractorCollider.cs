using Interactables;
using PathSystem;
using UnityEngine;

public class DistractorCollider : MonoBehaviour
{
    public Distractor distractor;
    public Node node;

    public void Clear()
    {
        node = null;
        distractor = null;
    }
}
