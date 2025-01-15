using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DownloadController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI fileNameText;
    public TextMeshProUGUI percentageText;
    public GameObject downloadButton, deleteButton;
    public bool downloadFinish = false;

    // Start is called before the first frame update
    void Start()
    {
        percentageText.text = null;

        Button downloadBtn = downloadButton.GetComponent<Button>();
        downloadBtn.onClick.AddListener(() => DownloadManager.Instance.DownloadFile(this));

        Button deleteBtn = deleteButton.GetComponent<Button>();
        deleteBtn.onClick.AddListener(() => DeviceManager.Instance.DeviceFile_Delete(fileNameText.text));
    }

    // Update is called once per frame
    void Update()
    {
        if (PatientManager.CurrentPlayer == null)
        {
            percentageText.text = null;

            downloadButton.SetActive(true);
            deleteButton.SetActive(false);

        }
        else
        {
            if (downloadFinish)
            {
                downloadButton.SetActive(false);
                deleteButton.SetActive(true);
            }
            else
            {
                downloadButton.SetActive(true);
                deleteButton.SetActive(false);
            }
        }
    }

    public void UpdateCheckList(string percentage, bool downloadFinish)
    {
        this.percentageText.text = percentage;
        this.downloadFinish = downloadFinish;
    }
}