using Achievement;
using DataSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementUIController : MonoBehaviour
{
    [Title("References")]
    [SerializeField, Required] private TMP_Text title;
    [SerializeField, Required] private Transform achivementUIContainer;
    [SerializeField, Required] private GameObject prefabs;

    private List<AchievementUI> m_achievements = new();

    private void OnEnable()
    {
        AchievementManager.OnDataLoaded += Inizializer;

        AchievementManager.OnDataSaved += RefreshUI;
    }

    private void OnDisable()
    {
        AchievementManager.OnDataLoaded -= Inizializer;

        AchievementManager.OnDataSaved -= RefreshUI;
    }

    private void Inizializer()
    {
        Debug.Log("Inizializer");

        AchievementManager tmp = ServiceLocator.Instance.AchievementManager;

        title.text = "Level " + tmp.hub + "-" + tmp.levelName;

        foreach (var item in tmp.achievements)
        {
            var ui = Instantiate(prefabs, achivementUIContainer).GetComponent<AchievementUI>();

            ui.Description.text = item.NameKey;

            ui.Icon.gameObject.SetActive(item.IsCompleted);

            m_achievements.Add(ui);
        }
    }

    private void RefreshUI()
    {
        Debug.Log("RefreshUI");

        AchievementManager tmp = ServiceLocator.Instance.AchievementManager;

        for (int i = 0; i < m_achievements.Count; i++)
        {
            m_achievements[i].Icon.gameObject.SetActive(tmp.achievements[i].IsCompleted);
        }
    }
}
