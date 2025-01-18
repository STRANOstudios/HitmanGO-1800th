using Agents;
using PathSystem;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManager : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField, Range(0.1f, 10f)] private float moveSpeed = 1f;
    [SerializeField, Range(0.1f, 10f)] private float rotateSpeed = 1f;
    [SerializeField, Range(0, 5f)] private float distance = 1f;

    public static event Action OnEndMovement;

    private void Awake()
    {
        ServiceLocator.Instance.NodeManager = this;
    }

    private void OnEnable()
    {
        AgentsManager.OnAgentsEndSettingsMovement += MoveCalculate;
    }

    private void OnDisable()
    {
        AgentsManager.OnAgentsEndSettingsMovement -= MoveCalculate;
    }


    #region PRIVATE METHODS

    private void MoveCalculate()
    {
        List<GameObject> storages = new();
        List<Vector3> vertices = new();

        foreach (Node node in NodeCache.Nodes)
        {
            if (node.Storages.Count > 0)
            {
                int counter = node.Storages.Count;

                if (node.Storages.Contains(ServiceLocator.Instance.Player.gameObject))
                    counter--;

                if (counter == 0) continue;

                vertices.AddRange(Utils.GenerateInscribedPolygonVertices(node.transform.position, counter, distance));

                foreach (GameObject obj in node.Storages)
                {
                    if (obj == ServiceLocator.Instance.Player.gameObject) continue;
                    storages.Add(obj);
                }
            }
        }

        if (storages.Count == 0)
        {
            OnEndMovement?.Invoke();
            return;
        }

        int activeMovements = storages.Count;

        int j = 0;
        foreach (GameObject obj in storages)
        {
            StartCoroutine(Movement(obj.transform, vertices[j], () =>
            {
                activeMovements--;
                if (activeMovements == 0)
                {
                    OnEndMovement?.Invoke();
                }
            })
                );
            j++;
        }
    }

    private IEnumerator Movement(Transform obj, Vector3 targetPosition, Action onComplete)
    {
        if (obj.position == targetPosition)
        {
            onComplete?.Invoke();
            yield break;
        }

        obj.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);

        float elapsedTime = 0f;

        // Perform smooth movement from the start position to the target position.
        while (elapsedTime < moveSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveSpeed);

            obj.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null; // Wait for the next frame to continue movement.
        }

        obj.position = targetPosition;

        if (obj.TryGetComponent(out Agent agent))
        {
            if (agent.Path.Count > 0)
            {
                int index = agent.Index + 1;
                if (agent.Index >= agent.Path.Count - 1)
                {
                    index = agent.Index - 1;
                }

                Debug.DrawLine(agent.Path[index].transform.position, agent.Path[index].transform.position + Vector3.up * 2, Color.blue, 2f);

                Quaternion targetRotation = Utils.CalculateTargetRotation(obj.position, agent.Path[index].transform.position);

                if (targetRotation != startRotation)
                {
                    elapsedTime = 0;

                    while (elapsedTime < moveSpeed)
                    {
                        elapsedTime += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsedTime / rotateSpeed);

                        // Rotate the agent smoothly towards the target position.
                        Quaternion smoothedRotation = Quaternion.Slerp(startRotation, targetRotation, t);
                        obj.rotation = Quaternion.Euler(0, smoothedRotation.eulerAngles.y, 0);
                        yield return null; // Wait for the next frame to continue movement.
                    }

                    obj.rotation = targetRotation;
                }
            }

        }

        onComplete?.Invoke();
    }

    #endregion
}
