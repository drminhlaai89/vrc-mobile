using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocaleManager : MonoBehaviour
{
    [SerializeField] TMP_Dropdown dropdown;
    private bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.English:
                dropdown.value = 0;
                break;
            case SystemLanguage.French:
                dropdown.value = 1;
                break;
            default:
                dropdown.value = 0;
                break;
        }
    }

    public void ChangeLocale(int localeID)
    {
        if (active == true)
            return;

        StartCoroutine(SetLocale(localeID));
    }

    IEnumerator SetLocale(int _localeID)
    {
        active = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
        active = false;
    }
}
