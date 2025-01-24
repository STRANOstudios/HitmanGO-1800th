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
        if (Vector3.Distance(obj.position, targetPosition) < 0.1f)
        {
            onComplete?.Invoke();
            yield break;
        }

        obj.GetPositionAndRotation(out Vector3 startPosition, out Quaternion startRotation);

        bool isAgent = false;

        if (obj.TryGetComponent(out Agent agent)) isAgent = true;

        if (isAgent && Vector3.Dot(obj.forward, (agent.CurrentNode.transform.position - obj.position).normalized) < 0.1f && Vector3.Distance(obj.position, targetPosition) > 1f)
        {
            yield return Rotation(
                agent,
                Utils.CalculateQuaternionRotationDifference(
                    obj,
                    targetPosition,
                    transform.position,
                    agent.CurrentNode.transform.position
                    )
                );
        }
        else
        {
            yield return new WaitForSeconds(rotateSpeed);
        }

        float elapsedTime = 0;

        // Perform smooth movement from the start position to the target position.
        while (elapsedTime < moveSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveSpeed);

            obj.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null; // Wait for the next frame to continue movement.
        }

        obj.position = targetPosition;

        if (isAgent && agent.Path.Count > 0)
        {
            bool flag = true;

            int index = agent.Index + 1;
            if (agent.Index >= agent.Path.Count - 1)
            {
                if (!agent.IsPatrol) flag = false;

                index = agent.Index - 1;
            }

            if (flag)
            {
                yield return Rotation(
                    agent,
                    Utils.CalculateQuaternionRotationDifference(
                        obj,
                        agent.Path[index].transform.position,
                        obj.position,
                        agent.Path[index].transform.position
                        )
                    );
            }
        }

        onComplete?.Invoke();
    }

    private IEnumerator Rotation(Agent agent, float targetYRotation)
    {
        float startYRotation = agent.transform.eulerAngles.y;

        float angleDifference = Mathf.Abs(startYRotation) - targetYRotation;

        float elapsedTime = 0f;
        while (elapsedTime < rotateSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / rotateSpeed);

            float currentYRotation = Mathf.Lerp(startYRotation, angleDifference, t);

            agent.transform.rotation = Quaternion.Euler(0, currentYRotation, 0);

            yield return null;
        }

        agent.transform.rotation = Quaternion.Euler(0, angleDifference, 0);
    }

    #endregion
}
