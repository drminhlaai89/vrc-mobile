using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class FolderData
{
    public List<string> folderNames;
}

[Serializable]
public class FileList
{
    public FileSize[] files;
}

[Serializable]
public class FileSize
{
    public string filename;
    public long filesize;
}

[Serializable]
public class VideoData
{
    public string name;
    public string duration;
    public string video;
    public string audio;
    public string delay;
}

[Serializable]
public class VideoList
{
    public List<VideoData> videos;
}

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;

    [SerializeField] string _serverUrl = "https://v-r-c.tech/media";
    public static string ServerUrl
    {
        get { return Instance._serverUrl; }
        set { Instance._serverUrl = value; }
    }

    [SerializeField] string _serverFolderUrl;
    public static string ServerFolderUrl
    {
        get { return Instance._serverFolderUrl; }
        set { Instance._serverFolderUrl = value; }
    }

    [Header("[ FOLDERS / FILES - SERVER ]")]
    [SerializeField] FolderData _folderData;
    public static FolderData FolderData
    {
        get { return Instance._folderData; }
        set { Instance._folderData = value; }
    }

    [SerializeField] FileList _fileData;
    public static FileList FileData
    {
        get { return Instance._fileData; }
        set { Instance._fileData = value; }
    }

    [Header("[ FILES - CLASSIFY ]")]
    [SerializeField] List<FileSize> mp4_files;
    public static List<FileSize> Mp4_files
    {
        get { return Instance.mp4_files; }
        set { Instance.mp4_files = value; }
    }

    [SerializeField] List<FileSize> mp3_files;
    public static List<FileSize> Mp3_files
    {
        get { return Instance.mp3_files; }
        set { Instance.mp3_files = value; }
    }

    [Header("[ PRESET ]")]
    [SerializeField] VideoList _videoList;
    public static VideoList VideoList
    {
        get { return Instance._videoList; }
        set { Instance._videoList = value; }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public IEnumerator ServerFolders_Get()
    {
        string folderPath = Path.Combine(ServerUrl, "GetFolder.php");
        UnityWebRequest www = UnityWebRequest.Get(folderPath);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            string folders = www.downloadHandler.text;
            FolderData = JsonUtility.FromJson<FolderData>(folders);
            Debug.Log("Get folders on server success.");

            yield return new WaitUntil(() => FolderData != null && DeviceManager.DevicePath != null);

            DeviceManager.DeviceFolder_Check();
            UIManager.CreateRoomButton.interactable = true;

            www.Dispose();
        }
    }

    public IEnumerator ServerFileInfo_Get()
    {
        string fileInfoPath = ServerGetFilePath();
        UnityWebRequest www = UnityWebRequest.Get(fileInfoPath);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string fileInfoData = www.downloadHandler.text;
            FileData = JsonUtility.FromJson<FileList>(fileInfoData);
            Debug.Log("Get files on server success.");

            yield return new WaitUntil(() => FileData != null && DeviceManager.DeviceFolderPath != null);

            for (int i = 0; i < FileData.files.Length; i++)
            {
                FileSize fileSize = FileData.files[i];
                string extension = Path.GetExtension(fileSize.filename).ToLower();

                if (extension == ".mp3")
                {
                    Mp3_files.Add(fileSize);
                }
                else if (extension == ".mp4")
                {
                    Mp4_files.Add(fileSize);
                }

                DownloadManager.DownloadFileList[fileSize.filename] = new FileDownloadInfo(null, null, false);
            }

            DeviceManager.DeviceFile_Check(Mp4_files);
            DeviceManager.DeviceFile_Check(Mp3_files);

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(PresetListUpdate(Path.Combine(ServerFolderUrl, "PatientData.json")));

                UIManager.Instance.UpadteUIFile(Mp4_files);
                UIManager.Instance.UpadteUIFile(Mp3_files);

                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    PatientManager.AddPatient(player);
                }
            }

            NetworkManager.Instance.EnterRoom();
            Debug.Log("Enter room");
        }
    }

    string ServerGetFilePath()
    {
        string folderName = "";
        TMP_InputField sessionInputField = UIManager.SessionInputField;

        if (FolderData.folderNames.Contains(sessionInputField.text))
        {
            int folderIndex = FolderData.folderNames.IndexOf(sessionInputField.text);
            folderName = FolderData.folderNames[folderIndex];
        }
        else
        {
            folderName = "Main";
        }

        ServerFolderUrl = Path.Combine(ServerUrl, folderName);
        DeviceManager.DeviceFolder_Create(folderName);

        return Path.Combine(ServerFolderUrl, "FilesSize.php");
    }

    IEnumerator PresetListUpdate(string jsonURL)
    {
        UnityWebRequest www = UnityWebRequest.Get(jsonURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            VideoList = JsonUtility.FromJson<VideoList>(www.downloadHandler.text);

            UIManager.PresetDropdown.dropdownItems.Clear();
            UIManager.PresetDropdown.selectedItemIndex = 0;

            foreach (var videoData in VideoList.videos)
            {
                UIManager.PresetDropdown.SetItemTitle(videoData.name);
                UIManager.PresetDropdown.SetItemIcon(UIManager.UserIcon);
                UIManager.PresetDropdown.CreateNewItem();
            }

            UIManager.PresetDropdown.dropdownEvent.AddListener((value) =>
            {
                VideoData videoData = VideoList.videos[value];

                UIManager.PatientNameInput.text = videoData.name;
                UIManager.DurationInput.text = videoData.duration;
                UIManager.VideoDropdown.dropdownItems.ForEach(videoItem =>
                {
                    if (videoItem.itemName == videoData.video)
                    {
                        int videoIndex = UIManager.VideoDropdown.dropdownItems.IndexOf(videoItem);
                        UIManager.VideoDropdown.ChangeDropdownInfo(videoIndex);
                    }
                });
                
                UIManager.AudioDropdown.dropdownItems.ForEach(audioItem =>
                {
                    if (audioItem.itemName == videoData.audio)
                    {
                        int audioIndex = UIManager.AudioDropdown.dropdownItems.IndexOf(audioItem);
                        UIManager.AudioDropdown.ChangeDropdownInfo(audioIndex);
                    }
                });

                UIManager.DelayInput.text = videoData.delay;

                PatientManager.Instance.ApplyPatientInfo();
            });
        }
    }
}
