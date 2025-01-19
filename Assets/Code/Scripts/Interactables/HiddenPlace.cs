using PathSystem;
using UnityEngine;

public class HiddenPlace : MonoBehaviour
{
    [SerializeField] private Node m_node;

    private void Awake()
    {
        transform.position = m_node.transform.position;

        m_node.Storages.Add(gameObject);
    }
}
