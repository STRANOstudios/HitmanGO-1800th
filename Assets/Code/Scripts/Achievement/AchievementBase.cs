using UnityEngine;

namespace DataSystem
{
    public abstract class AchievementBase : MonoBehaviour, IAchievement
    {
        [SerializeField] private string nameKey;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isCompleted;

        public string NameKey { get => nameKey; set => nameKey = value; }
        public Sprite Icon { get => icon; set => icon = value; }
        public bool IsCompleted { get => isCompleted; set => isCompleted = value; }

        public abstract bool CheckCondition();
    }
}
