using UnityEngine;

namespace DataSystem
{
    public class KillNEnemy : AchievementBase
    {
        [Tooltip("If true, this achievement will be completed when all killed, else it will be completed when no one killed.")]
        public bool AllKill = false;

        public override bool CheckCondition()
        {
            if (IsCompleted) return true;

            string tmp = AllKill ? "All Kill" : "No Kill";
            Debug.Log(tmp);
            IsCompleted = AllKill ? ServiceLocator.Instance.DeathManager.IsFull() : ServiceLocator.Instance.DeathManager.IsEmpty();

            return IsCompleted;
        }
    }
}