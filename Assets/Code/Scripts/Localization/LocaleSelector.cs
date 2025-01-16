using UnityEngine;
//using UnityEngine.Localization;
//using UnityEngine.Localization.Settings;

public class LocaleSelector : MonoBehaviour
{

    //private int currentLanguageIndex = 0;

    //private void Start()
    //{
    //    SetInitialLanguageIndex();
    //}

    ///// <summary>
    ///// Switches the language.
    ///// </summary>
    //public void SwitchLanguage()
    //{
    //    int availableLanguagesCount = LocalizationSettings.AvailableLocales.Locales.Count;

    //    currentLanguageIndex = (currentLanguageIndex + 1) % availableLanguagesCount;

    //    SetLanguageByIndex(currentLanguageIndex);
    //}

    //private void SetLanguageByIndex(int index)
    //{
    //    if (index >= 0 && index < LocalizationSettings.AvailableLocales.Locales.Count)
    //    {
    //        Locale newLocale = LocalizationSettings.AvailableLocales.Locales[index];
    //        LocalizationSettings.SelectedLocale = newLocale;
    //    }
    //}

    //private void SetInitialLanguageIndex()
    //{
    //    Locale currentLocale = LocalizationSettings.SelectedLocale;
    //    currentLanguageIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(currentLocale);
    //}
}

