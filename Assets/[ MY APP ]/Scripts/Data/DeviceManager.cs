using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DeviceManager : MonoBehaviourPunCallbacks
{
    public static DeviceManager Instance;

    [SerializeField] string _devicePath;
    public static string DevicePath
    {
        get
        {
            return Instance._devicePath;
        }
        set
        {
            Instance._devicePath = value;
        }
    }

    [SerializeField] string _deviceFolderPath;
    public static string DeviceFolderPath
    {
        get
        {
            return Instance._deviceFolderPath;
        }
        set
        {
            Instance._deviceFolderPath = value;
        }
    }

    private void Awake()
    {
        Instance = this;
        _devicePath = Application.persistentDataPath;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void DeviceFolder_Check()
    {
        string[] folderPaths = Directory.GetDirectories(DevicePath);

        foreach (string folderPath in folderPaths)
        {
            string folderName = Path.GetFileName(folderPath);

            if (!ServerManager.FolderData.folderNames.Contains(folderName))
            {
                Directory.Delete(folderPath, true);
            }
        }
    }

    public static void DeviceFolder_Create(string folder)
    {
        DeviceFolderPath = Path.Combine(DevicePath, folder);

        if (!Directory.Exists(DeviceFolderPath))
        {
            Directory.CreateDirectory(DeviceFolderPath);
        }
    }

    public static void DeviceFile_Check(List<FileSize> serverFiles)
    {
        string[] filePaths;

        if (serverFiles == ServerManager.Mp4_files)
        {
            filePaths = Directory.GetFiles(DeviceFolderPath, "*.mp4");
        }
        else
        {
            filePaths = Directory.GetFiles(DeviceFolderPath, "*.mp3");
        }

        foreach (string filePath in filePaths)
        {
            if (!FileExist(filePath, serverFiles))
            {
                Debug.Log(Path.GetFileName(filePath) + " not exist on server.");
                File.Delete(filePath);
            }
        }
    }

    public void DeviceFile_Delete(string fileName)
    {
        photonView.RPC("RPC_DeleteFile", PatientManager.CurrentPlayer, fileName);
    }

    [PunRPC]
    public void RPC_DeleteFile(string fileName)
    {
        string deviceFilePath = Path.Combine(DeviceFolderPath, fileName);
        File.Delete(deviceFilePath);

        DownloadManager.DownloadFileList[fileName].downloadFinish = false;
        DownloadManager.Instance.UpdateCheckListInfo();
    }

    static bool FileExist(string filePath, List<FileSize> serverFiles)
    {
        string fileName = Path.GetFileName(filePath);

        foreach (FileSize serverFile in serverFiles)
        {
            if (fileName == serverFile.filename)
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long deviceFileSize = fileInfo.Length;

                if (deviceFileSize == serverFile.filesize)
                {
                    DownloadManager.DownloadFileList[fileName] = new FileDownloadInfo(null, null, true);
                    return true;
                }
            }
        }

        return false;
    }
}
