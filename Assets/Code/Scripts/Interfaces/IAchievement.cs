using UnityEngine;

namespace DataSystem
{
    public interface IAchievement
    {
        public string NameKey { get; set; }
        public Sprite Icon { get; set; }
        public bool IsCompleted { get; set; }

        public abstract bool CheckCondition();
    }
}
