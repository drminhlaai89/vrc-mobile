using Photon.Pun;
using Photon.Pun.Demo.Cockpit.Forms;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Networking;
using UnityEngine.Video;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    [SerializeField] List<GameObject> controllers = new List<GameObject>();

    TMP_InputField sessionText;
    LocalizeStringEvent announceText;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        sessionText = UIManager.SessionInputField;
        announceText = UIManager.AnnounceText;

        if (PlayerPrefs.GetString("PlayerName") != string.Empty)
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        else
            PhotonNetwork.NickName = "Patient";

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateSession()
    {
        if (string.IsNullOrEmpty(sessionText.text.Trim()))
        {
            announceText.SetEntry("Room name empty");
        }
        else
        {
            if (PhotonNetwork.InRoom)
                return;

            announceText.SetEntry("Announce hide");
            UIManager.ActivePanel(UIManager.LoadingPanel, UIManager.PanelList);
            UIManager.LoadingPanelAnimator.Play("PanelFadeIn");

            byte maxPlayers = 20;
            maxPlayers = (byte)Mathf.Clamp(maxPlayers, 2, 8);

            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 1000 };
            PhotonNetwork.CreateRoom(sessionText.text, options, null);
        }
    }

    public void EnterRoom()
    {
        UIManager.RoomText.StringReference.Arguments = new object[] { sessionText.text };
        UIManager.LoadingPanelAnimator.Play("PanelFadeOut");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        announceText.SetEntry("Create room failed");
        UIManager.ActivePanel(UIManager.LoginPanel, UIManager.PanelList);
    }

    public void JoinSession()
    {
        if (string.IsNullOrEmpty(sessionText.text.Trim()))
        {
            announceText.SetEntry("Room name empty");
        }
        else
        {
            UIManager.ActivePanel(UIManager.LoadingPanel, UIManager.PanelList);
            UIManager.LoadingPanelAnimator.Play("PanelFadeIn");

            announceText.SetEntry("Announce hide");

            if (PhotonNetwork.InRoom)
                return;

            PhotonNetwork.JoinRoom(sessionText.text);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        announceText.SetEntry("Join room failed");
        UIManager.ActivePanel(UIManager.LoginPanel, UIManager.PanelList);
    }

    public override void OnJoinedRoom()
    {
        StartCoroutine(ServerManager.Instance.ServerFileInfo_Get());
        if (controllers.Count > 0)
        {
            controllers.ForEach(controller => { controller.SetActive(false); });
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            PatientManager.AddPatient(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            PatientManager.RemovePatient(otherPlayer);
    }

    public override void OnConnectedToMaster()
    {
        StartCoroutine(ServerManager.Instance.ServerFolders_Get());
        Debug.Log("Connect to server success.");
    }

    public void ExitSession()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {
        foreach (var item in DownloadManager.DownloadFileList)
        {
            if (item.Value.request != null)
            {
                item.Value.request.Abort();
                item.Value.request.Dispose();

                Debug.Log($"{item.Key} abort donwload.");
            }
        }

        StartCoroutine(LeaveRoom());
    }

    IEnumerator LeaveRoom()
    {
        ServerManager.Mp4_files.Clear();
        ServerManager.Mp3_files.Clear();
        DownloadManager.DownloadFileList.Clear();

        PatientManager.CurrentPlayer = null;
        PatientManager.PlayerEntries.Clear();
        foreach (Transform child in PatientManager.PatientPrefabContent)
        {
            Destroy(child.gameObject);
        }

        VideoManager.Instance.videoPlayer.Stop();
        VideoManager.Instance.videoPlayer.url = null;

        VideoManager.Instance.audioSource.Stop();
        VideoManager.Instance.audioSource.clip = null;

        VideoManager.Instance.videoPlayer.GetComponent<MeshRenderer>().material = VideoManager.Instance.waitingMaterial;

        UIManager.PatientNameInput.text = null;
        UIManager.DurationInput.text = null;
        UIManager.DelayInput.text = null;

        if (controllers.Count > 0)
        {
            controllers.ForEach(controller => { controller.SetActive(true); });
        }

        UIManager.ActivePanel(UIManager.LoadingPanel, UIManager.PanelList);
        UIManager.ActivePanel(UIManager.RoomList[0], UIManager.RoomList);

        UIManager.LoadingPanelAnimator.Play("PanelFadeIn");

        yield return new WaitForSeconds(1.0f);

        UIManager.LoadingPanelAnimator.Play("PanelFadeOut");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        PhotonNetwork.LeaveRoom();
    }
}
