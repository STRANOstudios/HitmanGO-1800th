using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [Header("References")]

    [Tooltip("The GameObject to look at")]
    [SerializeField] protected Transform m_LookAt;

    [Tooltip("The list of GameObjects to rotate")]
    [SerializeField] private List<Transform> m_List;

    [Header("Settings")]

    [Tooltip("The axis of rotation (E.g. Vector3(1, 1, 0) for X and Y only)")]
    [SerializeField] Vector3 rotationAxis = Vector3.zero;

    [Tooltip("Enable the rotation only if the GameObject is active")]
    [SerializeField] private bool m_ifIsActive = true;

    private void Update()
    {
        if (m_LookAt == null)
        {
            Debug.LogError("LookAt: LookAt missing reference");
            return;
        }

        // Cycle through the list and set the rotation
        foreach (Transform t in m_List)
        {
            if ((!m_ifIsActive || t.gameObject.activeInHierarchy) && t != null)
                Utils.LookAtWithAxes(t, m_LookAt.position, rotationAxis);
        }
    }
}
