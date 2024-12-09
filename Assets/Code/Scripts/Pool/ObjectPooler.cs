using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [ShowInInspector]
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Initialize a pool for a given prefab with a specified amount.
    /// </summary>
    public void InitializePool(GameObject prefab, int initialSize)
    {
        if (!poolDictionary.ContainsKey(prefab))
            poolDictionary[prefab] = new Queue<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            var newObject = CreateObject(prefab);
            newObject.SetActive(false);
            poolDictionary[prefab].Enqueue(newObject);
        }
    }

    /// <summary>
    /// Get an object from the pool, or create a new one if the pool is empty.
    /// </summary>
    [Obsolete("Use Get(string) instead")]
    public GameObject Get(GameObject prefab)
    {
        if (poolDictionary.ContainsKey(prefab) && poolDictionary[prefab].Count > 0)
        {
            var obj = poolDictionary[prefab].Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // If pool is empty, create a new object
        return CreateObject(prefab);
    }

    /// <summary>
    /// Get an object from the pool, or create a new one if the pool is empty.
    /// </summary>
    [Obsolete("Use Get(string, float) instead")]
    public GameObject Get(GameObject prefab, float delay)
    {
        GameObject obj;
        if (poolDictionary.ContainsKey(prefab) && poolDictionary[prefab].Count > 0)
        {
            obj = poolDictionary[prefab].Dequeue();
            obj.SetActive(true);

            StartCoroutine(DisableObject(obj, delay));

            return obj;
        }

        // If pool is empty, create a new object
        obj = CreateObject(prefab);
        StartCoroutine(DisableObject(obj, delay));

        return obj;
    }

    /// <summary>
    /// Get an object from the pool, or create a new one if the pool is empty.
    /// </summary>
    public GameObject Get(string prefabName)
    {
        GameObject prefab = GetPrefabByName(prefabName);

        if (poolDictionary.ContainsKey(prefab) && poolDictionary[prefab].Count > 0)
        {
            var obj = poolDictionary[prefab].Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return CreateObject(prefab);
    }

    /// <summary>
    /// Get an object from the pool, or create a new one if the pool is empty.
    /// </summary>
    public GameObject Get(string prefabName, float delay)
    {
        GameObject prefab = GetPrefabByName(prefabName);

        GameObject obj = null;
        if (poolDictionary.ContainsKey(prefab) && poolDictionary[prefab].Count > 0)
        {
            obj = poolDictionary[prefab].Dequeue();
            obj.SetActive(true);
        }

        if (obj == null) obj = CreateObject(prefab);

        StartCoroutine(DisableObject(obj, delay));

        return obj;
    }

    /// <summary>
    /// Searches for a prefab in the pool by its name and returns the GameObject prefab.
    /// </summary>
    public GameObject GetPrefabByName(string prefabName)
    {
        // Search for the prefab in the dictionary
        foreach (var entry in poolDictionary)
        {
            if (entry.Key.name == prefabName)
            {
                return entry.Key; // Return the prefab GameObject
            }
        }

        // If not found, you can either return null or create a new prefab (depending on the logic you want)
        return null;
    }

    /// <summary>
    /// Return an object to its pool. This method doesn't require the prefab to be passed.
    /// </summary>
    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        // Disabilita il GameObject
        obj.SetActive(false);

        // Rimuovi le ultime 7 lettere dal nome del GameObject (per esempio "_Pooled")
        string originalName = obj.name;
        string baseName = originalName.Length > 7 ? originalName.Substring(0, originalName.Length - 7) : originalName;

        // Trova la coda corretta per il prefab e rimetti l'oggetto
        bool found = false;
        foreach (var entry in poolDictionary)
        {
            // Verifica se il nome dell'oggetto (senza "_Pooled") corrisponde al nome del prefab
            if (entry.Key.name == baseName)
            {
                entry.Value.Enqueue(obj);
                found = true;
                break;
            }
        }

        // Se non trovi una coda per questo prefab, aggiungi l'oggetto al dizionario
        if (!found)
        {
            // Crea una nuova coda per il prefab se non esiste e aggiungi l'oggetto
            if (!poolDictionary.ContainsKey(obj))
            {
                poolDictionary.Add(obj, new Queue<GameObject>());
            }
            poolDictionary[obj].Enqueue(obj);
        }
    }


    /// <summary>
    /// Creates a new instance of the prefab.
    /// </summary>
    private GameObject CreateObject(GameObject prefab)
    {
        var newObject = Instantiate(prefab);
        newObject.name = prefab.name + "_Pooled";
        return newObject;
    }

    /// <summary>
    /// Returns the AudioSource GameObject to the pool after playback.
    /// </summary>
    private IEnumerator DisableObject(GameObject source, float duration)
    {
        yield return new WaitForSeconds(duration);
        ReturnToPool(source);
    }
}
