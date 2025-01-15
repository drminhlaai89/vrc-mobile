using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PatientController : MonoBehaviour
{
    [Header("Text")]
    public TextMeshProUGUI patientName;
    public TextMeshProUGUI videoName;
    public TextMeshProUGUI audioName;
    public TextMeshProUGUI time;

    [Header("Play/Pause Button")]
    public GameObject playButton;
    public GameObject pauseButton;

    [Header("Select Avatar")]
    public Image selectImage;
    public Button selectButton;

    [HideInInspector] public int duration = 30;
    [HideInInspector] public int delay = 0;
    [HideInInspector] public float currentTime;
    [HideInInspector] public bool isCountingDown = false;

    // Start is called before the first frame update
    void Start()
    {
        time.text = string.Format("{0:00}:{1:00}", duration / 60, duration % 60);
        currentTime = duration;

        playButton.GetComponent<Button>().onClick.AddListener(PlayPauseMedia);
        pauseButton.GetComponent<Button>().onClick.AddListener(PlayPauseMedia);
    }

    // Update is called once per frame
    void Update()
    {
        if (PatientManager.CurrentPlayer != null && PatientManager.PlayerEntries[PatientManager.CurrentPlayer] == this)
        {
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (GameObject obj in allGameObjects)
            {
                ChangeColor(obj, videoName);
                ChangeColor(obj, audioName);
            }
        }
        else
        {
            videoName.color = PatientManager.DownloadColor;
            audioName.color = PatientManager.DownloadColor;
        }

        if (isCountingDown)
        {
            currentTime -= Time.deltaTime;
            time.text = string.Format("{0:00}:{1:00}", (int)currentTime / 60, (int)currentTime % 60);

            if (currentTime <= 0)
            {
                isCountingDown = false;
                currentTime = duration;
                time.text = string.Format("{0:00}:{1:00}", duration / 60, duration % 60);

                playButton.SetActive(true);
                pauseButton.SetActive(false);

                foreach (var playerEntry in PatientManager.PlayerEntries)
                {
                    if (playerEntry.Value.Equals(this))
                    {
                        VideoManager.Instance.MakeMediaNew(playerEntry.Key);
                    }
                }
            }
        }
    }

    void ChangeColor(GameObject obj, TextMeshProUGUI mediaName)
    {
        if (obj.name == mediaName.text)
        {
            DownloadController downloadController = obj.GetComponent<DownloadController>();
            if (downloadController.downloadFinish)
            {
                mediaName.color = PatientManager.DownloadColor;
            }
            else
            {
                mediaName.color = PatientManager.NotDownloadColor;
            }
        }
    }

    void PlayPauseMedia()
    {
        if (PatientManager.CurrentPlayer != null && PatientManager.PlayerEntries[PatientManager.CurrentPlayer] == this)
        {
            if (playButton.activeInHierarchy)
            {
                isCountingDown = true;

                playButton.SetActive(false);
                pauseButton.SetActive(true);

                if (currentTime == duration)
                {
                    StartCoroutine(PlayMediaDelay(delay));
                }
                else
                {
                    PlayCurrentMedia("UnPause");
                }
            }
            else
            {
                isCountingDown = false;

                playButton.SetActive(true);
                pauseButton.SetActive(false);

                VideoManager.Instance.PauseVideo();
                VideoManager.Instance.PauseAudio();
            }
        }
    }

    IEnumerator PlayMediaDelay(int delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (isCountingDown)
            PlayCurrentMedia("Play");
    }

    void PlayCurrentMedia(string audioState)
    {
        if (videoName.color == PatientManager.DownloadColor)
            VideoManager.Instance.PlayMedia(videoName.text);

        if (audioName.color == PatientManager.DownloadColor)
            VideoManager.Instance.PlayAudio(audioName.text, audioState);
    }
}
