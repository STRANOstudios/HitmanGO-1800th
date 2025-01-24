using Agents;
using System;
using UnityEngine;

public class KillHandler : MonoBehaviour
{
    public static event Action OnKill;
    public static event Action<Agent> OnKillAgent;

    public void Kill()
    {
        if (gameObject.GetComponent<Agent>())
        {
            OnKillAgent?.Invoke(gameObject.GetComponent<Agent>());
            return;
        }

        OnKill?.Invoke();
    }
}
