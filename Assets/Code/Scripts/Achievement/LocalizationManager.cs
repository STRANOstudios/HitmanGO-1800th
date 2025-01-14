using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Achievement
{
    public class LocalizationManager : MonoBehaviour
    {
        private LocalizationSettings localizationSettings;

        void Start()
        {
            localizationSettings = LocalizationSettings.Instance;
        }

        // Get localized text for a given key
        public string GetLocalizedText(string key)
        {
            if (localizationSettings != null)
            {
                //var localizedString = localizationSettings.GetLocalizedString(key);
                //return localizedString;
            }
            return key;  // If localization fails, return the key as fallback
        }
    }
}
