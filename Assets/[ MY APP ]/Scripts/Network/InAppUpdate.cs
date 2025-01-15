using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Play.AppUpdate;
using Google.Play.Common;
using TMPro;

public class InAppUpdate : MonoBehaviour
{
    AppUpdateManager appUpdateManager;
    //[SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        appUpdateManager = new AppUpdateManager();
        StartCoroutine(CheckForUpdate());
    }

    private IEnumerator CheckForUpdate()
    {
        Debug.Log("Checking for updates...");
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation =
            appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.Error != AppUpdateErrorCode.NoError)
        {
            Debug.Log($"Update check failed: {appUpdateInfoOperation.Error}");
            yield break;
        }

        var appUpdateInfo = appUpdateInfoOperation.GetResult();

        if (appUpdateInfo.UpdateAvailability == UpdateAvailability.UpdateAvailable)
        {
            Debug.Log("Starting immediate update...");
            StartCoroutine(StartImmediateUpdate(appUpdateInfo));
        }
        else if (appUpdateInfo.UpdateAvailability == UpdateAvailability.UpdateNotAvailable)
        {
            Debug.Log("App is up to date");
        }
        else if (appUpdateInfo.UpdateAvailability == UpdateAvailability.DeveloperTriggeredUpdateInProgress)
        {
            Debug.Log("Update already in progress");
            StartCoroutine(StartImmediateUpdate(appUpdateInfo));
        }
    }

    private IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfo)
    {
        Debug.Log("Starting update...");
        var startUpdateRequest = appUpdateManager.StartUpdate(
            appUpdateInfo,
            AppUpdateOptions.ImmediateAppUpdateOptions()
        );

        yield return startUpdateRequest;

        if (startUpdateRequest.Error == AppUpdateErrorCode.NoError)
        {
            Debug.Log("Update downloaded, restarting...");
            // The app will restart automatically
        }
        else
        {
            Debug.Log($"Update failed: {startUpdateRequest.Error}");
        }
    }
}
