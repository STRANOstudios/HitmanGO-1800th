using PathSystem;
using UnityEngine;

public class HiddenPlace : MonoBehaviour
{
    [SerializeField] private Node m_node;

    private void OnValidate()
    {
        if (m_node != null && m_node.transform.position != transform.position)
            transform.position = m_node.transform.position;
    }

    private void Awake()
    {
        transform.position = m_node.transform.position;

        m_node.Storages.Add(gameObject);
    }
}
