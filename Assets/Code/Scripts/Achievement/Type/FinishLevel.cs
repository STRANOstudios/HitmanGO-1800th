using UnityEngine;

namespace DataSystem
{
    public class FinishLevel : AchievementBase
    {
        public override bool CheckCondition()
        {
            if (IsCompleted) return true;

            Debug.Log("Finish Level");
            IsCompleted = ServiceLocator.Instance.AchievementManager.GetCompleteState;
            return IsCompleted;
        }
    }
}
