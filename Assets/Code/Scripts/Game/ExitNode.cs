using System;
using UnityEngine;

public class ExitNode : MonoBehaviour
{
    [SerializeField] private LayerMask LayerMask;

    public static event Action Exit;

    private void OnEnable()
    {
        ShiftManager.OnEnemyTurn += CheckPlayerPresence;
    }

    private void OnDisable()
    {
        ShiftManager.OnEnemyTurn -= CheckPlayerPresence;
    }

    private void CheckPlayerPresence()
    {
        Ray ray = new(transform.position, Vector3.up);

        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 1f);

        if (Physics.Raycast(ray, LayerMask))
        {
            Exit?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.up * 1f);
    }
}
