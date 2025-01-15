using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class FileDownloadInfo
{
    public UnityWebRequest request = null;
    public string percentage = null;
    public bool downloadFinish = false;

    public FileDownloadInfo(UnityWebRequest request, string percentage, bool downloadFinish)
    {
        this.request = request;
        this.percentage = percentage;
        this.downloadFinish = downloadFinish;
    }
}

public class DownloadManager : MonoBehaviourPunCallbacks
{
    public static DownloadManager Instance;

    [SerializeField] Dictionary<string, FileDownloadInfo> _downloadFileList = new Dictionary<string, FileDownloadInfo>();
    public static Dictionary<string, FileDownloadInfo> DownloadFileList
    {
        get { return Instance._downloadFileList; }
        set { Instance._downloadFileList = value; }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void DownloadFile(DownloadController downloadController)
    {
        if (PatientManager.CurrentPlayer != null)
        {
            string fileName = downloadController.fileNameText.text;
            photonView.RPC("RPC_DownloadFile", PatientManager.CurrentPlayer, fileName);
        }
    }

    [PunRPC]
    public void RPC_DownloadFile(string fileName)
    {
        FileDownloadInfo fileDownloadInfo;
        DownloadFileList.TryGetValue(fileName, out fileDownloadInfo);

        if (fileDownloadInfo != null)
        {
            if (fileDownloadInfo.request == null && !fileDownloadInfo.downloadFinish)
            {
                StartCoroutine(StartDownloadFile(fileName));
            }
            else if (fileDownloadInfo.request != null)
            {
                Debug.Log($"{fileName} is downloading.");
            }
            else if (fileDownloadInfo.downloadFinish)
            {
                Debug.Log($"{fileName} is downloaded.");
            }
        }
    }

    IEnumerator StartDownloadFile(string fileName)
    {
        string urlPath = Path.Combine(ServerManager.ServerFolderUrl, fileName);
        string devicePath = Path.Combine(DeviceManager.DeviceFolderPath, fileName);

        UnityWebRequest request = UnityWebRequest.Get(urlPath);
        request.downloadHandler = new DownloadHandlerFile(devicePath);

        UnityWebRequestAsyncOperation op = request.SendWebRequest();

        DownloadFileList[fileName].request = request;

        while (!op.isDone)
        {
            string percentage = Mathf.RoundToInt(request.downloadProgress * 100).ToString() + "%";
            DownloadFileList[fileName].percentage = percentage;
            UpdateCheckListInfo();

            yield return null;
        }

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            if (DownloadFileList != null)
                DownloadFileList[fileName] = new FileDownloadInfo(null, null, true);

            UpdateCheckListInfo();
            request.Dispose();
        }
    }

    public void UpdateCheckListInfo()
    {
        foreach (var item in DownloadFileList)
        {
            photonView.RPC("RPC_UpdateDownloadFIleInfo", PhotonNetwork.MasterClient, item.Key, item.Value.percentage, item.Value.downloadFinish);
            Debug.Log(item.Key + " - " + item.Value.downloadFinish);
        }
    }

    [PunRPC]
    public void RPC_UpdateDownloadFIleInfo(string fileName, string percentage, bool downloadFinish, PhotonMessageInfo info)
    {
        if (info.Sender == PatientManager.CurrentPlayer)
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allGameObjects)
            {
                if (obj.name == fileName)
                {
                    obj.GetComponent<DownloadController>().UpdateCheckList(percentage, downloadFinish);
                }
            }
        }
    }
}