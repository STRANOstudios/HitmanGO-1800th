using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static event Action OnDataLoaded;

    private void OnEnable()
    {
        SceneLoader.OnSceneLoaded += LoadData;
    }

    private void OnDisable()
    {
        SceneLoader.OnSceneLoaded -= LoadData;
    }

    private void LoadData()
    {
        // caricamento dati livello

        Debug.Log("Dati caricati");

        OnDataLoaded?.Invoke();
    }
}
