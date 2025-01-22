using Achievement;
using DataSystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class AchievementUIController : MonoBehaviour
{
    [Title("References")]
    [SerializeField] private Transform achivementUIContainer;
    [SerializeField] private GameObject prefabs;

    private List<AchievementUI> m_achievements = new();

    private void OnEnable()
    {
        AchievementManager.OnDataLoaded += RefreshUI;

        GameManager.OnEndGame += RefreshUI;
    }

    private void OnDisable()
    {
        AchievementManager.OnDataLoaded -= RefreshUI;

        GameManager.OnEndGame -= RefreshUI;
    }

    private void RefreshUI()
    {
        Debug.Log("RefreshUI");

        AchievementManager tmp = ServiceLocator.Instance.AchievementManager;

        foreach (var item in tmp.achievements)
        {
            var ui = Instantiate(prefabs, achivementUIContainer).GetComponent<AchievementUI>();

            ui.Description.text = item.NameKey;

            ui.Icon.gameObject.SetActive(item.IsCompleted);

            m_achievements.Add(ui);
        }
    }
}
