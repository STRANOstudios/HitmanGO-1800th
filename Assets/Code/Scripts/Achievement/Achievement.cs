using System;

namespace Achievement
{
    [Serializable]
    public class Achievement
    {
        public string NameKey;    // Key for localized achievement name
        public string DescriptionKey;  // Key for localized description
        public bool IsCompleted;

        public Achievement(string nameKey, string descriptionKey)
        {
            NameKey = nameKey;
            DescriptionKey = descriptionKey;
            IsCompleted = false;
        }
    }
}
