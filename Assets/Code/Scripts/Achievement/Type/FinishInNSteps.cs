using UnityEngine;

namespace DataSystem
{
    public class FinishInNSteps : AchievementBase
    {
        [Tooltip("Number of steps")]
        [Range(0, 50)] public int Value = 0;

        public override bool CheckCondition()
        {
            if (IsCompleted) return true;

            Debug.Log($"Finish In {Value} Steps");
            IsCompleted = ServiceLocator.Instance.AchievementManager.GetStepCount <= Value;
            return IsCompleted;
        }
    }
}
