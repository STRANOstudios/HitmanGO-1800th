using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Collectible : TriggerVisualizer
{
    public static event Action OnCollectibleCollected;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            OnCollectibleCollected?.Invoke();
            Destroy(gameObject);
        }
    }
}
