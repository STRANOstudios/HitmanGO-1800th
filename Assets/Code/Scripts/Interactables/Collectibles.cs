using PathSystem;
using System;
using System.Linq;
using UnityEngine;

public class Collectibles : MonoBehaviour
{
    [SerializeField] private Node m_node;

    public static event Action OnCollectibleCollected;

    private void OnValidate()
    {
        if (m_node != null && m_node.transform.position != transform.position)
            transform.position = m_node.transform.position;
    }

    private void OnEnable()
    {
        GameStatusManager.OnEnemyTurn += CheckPlayerPresence;
    }

    private void OnDisable()
    {
        GameStatusManager.OnEnemyTurn -= CheckPlayerPresence;
    }

    private void CheckPlayerPresence()
    {
        Debug.Log("CheckPlayerPresence");

        if (m_node.Storages.Any(obj => obj.CompareTag("Player")))
        {
            OnCollectibleCollected?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
