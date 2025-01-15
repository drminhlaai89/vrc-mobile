using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PatientManager : MonoBehaviourPunCallbacks
{
    public static PatientManager Instance;

    [SerializeField] GameObject _patientPrefab;
    public static GameObject PatientPrefab
    {
        get { return Instance._patientPrefab; }
    }

    [SerializeField] Transform _patientPrefabContent;
    public static Transform PatientPrefabContent
    {
        get { return Instance._patientPrefabContent; }
    }

    Dictionary<Player, PatientController> _playerEntries = new Dictionary<Player, PatientController>();
    public static Dictionary<Player, PatientController> PlayerEntries
    {
        get { return Instance._playerEntries; }
    }

    Player _currentPlayer;
    public static Player CurrentPlayer
    {
        get { return Instance._currentPlayer; }
        set
        {
            Instance._currentPlayer = value;
            CheckPatient();
        }
    }

    [SerializeField] Color _downloadColor;
    public static Color DownloadColor
    {
        get { return Instance._downloadColor; }
    }

    [SerializeField] Color _notDownloadColor;
    public static Color NotDownloadColor
    {
        get { return Instance._notDownloadColor; }
    }



    private void Awake()
    {
        Instance = this;
    }

    public static void AddPatient(Player patient)
    {
        GameObject entry = Instantiate(PatientPrefab, PatientPrefabContent);
        PatientController patientController = entry.GetComponent<PatientController>();

        patientController.patientName.text = patient.NickName;
        patientController.videoName.text = ServerManager.Mp4_files[0].filename;
        patientController.audioName.text = ServerManager.Mp3_files[0].filename;
        patientController.selectButton.onClick.AddListener(() => Instance.SelectPatient(patient, patientController));

        PlayerEntries.Add(patient, patientController);
    }

    public static void RemovePatient(Player patient)
    {
        if (PlayerEntries.TryGetValue(patient, out PatientController patientController))
        {
            Destroy(patientController.gameObject);
            PlayerEntries.Remove(patient);

            if (CurrentPlayer == patient)
            {
                CurrentPlayer = null;
                UIManager.PatientNameInput.text = null;
                UIManager.DurationInput.text = null;
                UIManager.DelayInput.text = null;
            }
        }
    }

    void SelectPatient(Player patient, PatientController patientController)
    {
        if (CurrentPlayer != patient)
        {
            CurrentPlayer = patient;
            UIManager.PatientNameInput.text = patientController.patientName.text;
            UIManager.DurationInput.text = patientController.duration.ToString();
            UIManager.DelayInput.text = patientController.delay.ToString();

            photonView.RPC("RPC_UpdateCheckListInfo", CurrentPlayer);
        }
        else
        {
            CurrentPlayer = null;
            UIManager.PatientNameInput.text = null;
            UIManager.DurationInput.text = null;
            UIManager.DelayInput.text = null;
        }
    }

    [PunRPC]
    public void RPC_UpdateCheckListInfo()
    {
        DownloadManager.Instance.UpdateCheckListInfo();
    }

    static void CheckPatient()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (PlayerEntries.TryGetValue(player, out PatientController patientController))
            {
                if (player == CurrentPlayer)
                {
                    patientController.selectImage.enabled = true;
                }
                else
                {
                    patientController.selectImage.enabled = false;
                }
            }
        }
    }

    public void ApplyPatientInfo()
    {
        if (CurrentPlayer != null && PlayerEntries.TryGetValue(CurrentPlayer, out PatientController patientController))
        {
            string patientName = UIManager.PatientNameInput.text;

            if (!string.IsNullOrEmpty(patientName))
            {
                patientController.patientName.text = patientName;
                photonView.RPC("RPC_ChangeNickName", CurrentPlayer, patientName);
            }

            if (!string.IsNullOrEmpty(UIManager.DelayInput.text))
                patientController.delay = int.Parse(UIManager.DelayInput.text);

            if (!string.IsNullOrEmpty(UIManager.DurationInput.text))
            {
                patientController.duration = int.Parse(UIManager.DurationInput.text);
                patientController.currentTime = patientController.duration;
                patientController.time.text = string.Format("{0:00}:{1:00}", patientController.duration / 60, patientController.duration % 60);
            }

            patientController.isCountingDown = false;
            patientController.audioName.text = UIManager.AudioDropdown.selectedText.text;
            patientController.videoName.text = UIManager.VideoDropdown.selectedText.text;
            patientController.playButton.SetActive(true);
            patientController.pauseButton.SetActive(false);

            VideoManager.Instance.MakeMediaNew(CurrentPlayer);
        }
    }

    public void AddPatitentPreset()
    {
        if (CurrentPlayer != null)
        {
            ServerManager.VideoList.videos.Add(new VideoData
            {
                name = UIManager.PatientNameInput.text,
                duration = UIManager.DurationInput.text,
                video = UIManager.VideoDropdown.selectedText.text,
                audio = UIManager.AudioDropdown.selectedText.text,
                delay = UIManager.DelayInput.text
            });
            UIManager.PresetDropdown.SetItemTitle(UIManager.PatientNameInput.text);
            UIManager.PresetDropdown.SetItemIcon(UIManager.UserIcon);
            UIManager.PresetDropdown.CreateNewItem();
        }
    }

    [PunRPC]
    public void RPC_ChangeNickName(string newPatientName)
    {
        PlayerPrefs.SetString("PlayerName", newPatientName);
        PhotonNetwork.NickName = newPatientName;
    }
}
