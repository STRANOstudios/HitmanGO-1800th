using UnityEngine;

namespace DataSystem
{
    public class Retrieve : AchievementBase
    {
        public override bool CheckCondition()
        {
            Debug.Log("Retrieve");
            IsCompleted = ServiceLocator.Instance.AchievementManager.GetCollectibleState;
            return IsCompleted;
        }
    }
}