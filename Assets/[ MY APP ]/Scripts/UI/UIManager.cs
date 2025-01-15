using Michsky.UI.ModernUIPack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("PANELS")]
    [SerializeField] List<GameObject> _panelList = new List<GameObject>();
    public static List<GameObject> PanelList
    {
        get { return Instance._panelList; }
    }

    [SerializeField] List<GameObject> _roomList = new List<GameObject>();
    public static List<GameObject> RoomList
    {
        get { return Instance._roomList; }
    }

    [Header("[ LOGIN UI ]")]
    [SerializeField] GameObject _loginPanel;
    public static GameObject LoginPanel
    {
        get { return Instance._loginPanel; }
    }

    [SerializeField] Button _createRoomButton;
    public static Button CreateRoomButton
    {
        get { return Instance._createRoomButton; }
    }

    [SerializeField] TMP_InputField _sessionInputField;
    public static TMP_InputField SessionInputField
    {
        get { return Instance._sessionInputField; }
    }

    [SerializeField] LocalizeStringEvent _announceText;
    public static LocalizeStringEvent AnnounceText
    {
        get { return Instance._announceText; }
    }

    [Header("[ LOADING UI ]")]
    [SerializeField] GameObject _loadingPanel;
    public static GameObject LoadingPanel
    {
        get { return Instance._loadingPanel; }
    }

    [SerializeField] Animator _loadingPanelAnimator;
    public static Animator LoadingPanelAnimator
    {
        get { return Instance._loadingPanelAnimator; }
    }

    [Header("[ ROOM UI - INFO]")]
    [SerializeField] GameObject _roomPanel;
    public static GameObject RoomPanel
    {
        get { return Instance._roomPanel; }
    }

    [SerializeField] LocalizeStringEvent _roomText;
    public static LocalizeStringEvent RoomText
    {
        get { return Instance._roomText; }
    }

    [SerializeField] LocalizeStringEvent _statusConnectText;
    public static LocalizeStringEvent StatusConnectText
    {
        get { return Instance._statusConnectText; }
    }

    [SerializeField] LocalizeStringEvent _deviceConnectText;
    public static LocalizeStringEvent DeviceConnectText
    {
        get { return Instance._deviceConnectText; }
    }

    [Header("[ ROOM UI - PRESET]")]
    [SerializeField] Sprite _userIcon;
    public static Sprite UserIcon
    {
        get { return Instance._userIcon; }
    }

    [SerializeField] CustomDropdown _presetDropdown;
    public static CustomDropdown PresetDropdown
    {
        get { return Instance._presetDropdown; }
    }

    [Header("[ ROOM UI - CONFIG]")]
    [SerializeField] TMP_InputField _patientNameInput;
    public static TMP_InputField PatientNameInput
    {
        get { return Instance._patientNameInput; }
        set { Instance._patientNameInput = value; }
    }

    [SerializeField] TMP_InputField _durationInput;
    public static TMP_InputField DurationInput
    {
        get { return Instance._durationInput; }
        set { Instance._durationInput = value; }
    }

    [SerializeField] TMP_InputField _delayInput;
    public static TMP_InputField DelayInput
    {
        get { return Instance._delayInput; }
        set { Instance._delayInput = value; }
    }

    [SerializeField] Sprite videoIcon;
    [SerializeField] Sprite audioIcon;

    [SerializeField] CustomDropdown _videoDropdown;
    public static CustomDropdown VideoDropdown
    {
        get { return Instance._videoDropdown; }
    }

    [SerializeField] CustomDropdown _audioDropdown;
    public static CustomDropdown AudioDropdown
    {
        get { return Instance._audioDropdown; }
    }

    [Header("[ ROOM UI - CHECKLIST]")]
    [SerializeField] GameObject downloadPrefab;

    [SerializeField] Transform _videoCheckListContent;
    public static Transform VideoCheckListContent
    {
        get { return Instance._videoCheckListContent; }
    }

    [SerializeField] Transform _audioCheckListContent;
    public static Transform AudioCheckListContent
    {
        get { return Instance._audioCheckListContent; }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        ActivePanel(LoginPanel, PanelList);
    }

    public static void ActivePanel(GameObject objActive, List<GameObject> panelList)
    {
        panelList.ForEach(panel => panel.SetActive(panel == objActive));
    }

    public void UpadteUIFile(List<FileSize> files)
    {
        CustomDropdown dropdown;
        Transform checkListContent;

        if (ServerManager.Mp4_files == files)
        {
            dropdown = VideoDropdown;
            checkListContent = VideoCheckListContent;
        }
        else
        {
            dropdown = AudioDropdown;
            checkListContent = AudioCheckListContent;
        }

        dropdown.dropdownItems.Clear();
        dropdown.selectedItemIndex = 0;

        foreach (Transform child in checkListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var file in files)
        {
            UpdateDropdown(file, dropdown);
            UpdateCheckList(file, checkListContent);
        }
    }

    void UpdateDropdown(FileSize file, CustomDropdown dropdown)
    {

        dropdown.SetItemTitle(file.filename);
        dropdown.SetItemIcon(videoIcon);
        dropdown.CreateNewItem();
    }

    void UpdateCheckList(FileSize file, Transform checkListContent)
    {
        GameObject downloadObj = Instantiate(downloadPrefab, checkListContent);
        downloadObj.name = file.filename;

        DownloadController downloadController = downloadObj.GetComponent<DownloadController>();
        downloadController.fileNameText.text = file.filename;
    }
}
