using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using System.IO;
using UnityEngine.Video;
using Photon.Realtime;

public class VideoManager : MonoBehaviourPunCallbacks
{
    public static VideoManager Instance;

    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public Material waitingMaterial;
    public Material playingMaterial;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    public void MakeMediaNew(Player _player)
    {
        photonView.RPC("RPC_MakeMediaNew", _player);
    }
    void OnEnable()
    {
        videoPlayer.prepareCompleted += OnPrepareVideo;
    }

    void OnDisable()
    {
        videoPlayer.prepareCompleted -= OnPrepareVideo;
    }

    [PunRPC]
    public void RPC_MakeMediaNew()
    {
        videoPlayer.Stop();
        videoPlayer.url = null;

        audioSource.Stop();
        audioSource.clip = null;

        videoPlayer.GetComponent<MeshRenderer>().material = waitingMaterial;

        UIManager.RoomPanel.SetActive(true);
    }

    public void PlayMedia(string mp4File)
    {
        if (PatientManager.CurrentPlayer != null)
            photonView.RPC("RPC_PlayVideo", PatientManager.CurrentPlayer, mp4File);
    }

    [PunRPC]
    public void RPC_PlayVideo(string mp4File)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            UIManager.RoomPanel.SetActive(false);
        }

        string mp4Path = Path.Combine(DeviceManager.DeviceFolderPath, mp4File);

        if (videoPlayer.url != mp4Path)
        {
            videoPlayer.url = mp4Path;
            videoPlayer.Prepare();
        }
        else
        {
            videoPlayer.Play();

        }

    }

    void OnPrepareVideo(VideoPlayer source)
    {
        int width = (int)videoPlayer.width;
        int height = (int)videoPlayer.height;
        RenderTexture renderTexture = new RenderTexture(width, height, 0);

        videoPlayer.targetTexture = renderTexture;

        // Set the skybox material to use the render texture
        playingMaterial.SetTexture("_MainTex", renderTexture);
        videoPlayer.GetComponent<MeshRenderer>().material = playingMaterial;

        videoPlayer.Play();
    }

    public void PauseVideo()
    {
        if (PatientManager.CurrentPlayer != null)
            photonView.RPC("RPC_PauseVideo", PatientManager.CurrentPlayer);
    }


    [PunRPC]
    public void RPC_PauseVideo()
    {
        videoPlayer.Pause();
    }

    public void PlayAudio(string mp3File, string audioState)
    {
        if (PatientManager.CurrentPlayer != null)
            photonView.RPC("RPC_PlayAudio", PatientManager.CurrentPlayer, mp3File, audioState);
    }

    [PunRPC]
    public void RPC_PlayAudio(string mp3File, string audioState)
    {
        string mp3Path = Path.Combine(DeviceManager.DeviceFolderPath, mp3File);

        if (audioState == "Play")
            StartCoroutine(LoadAudioClip(mp3Path));
        else if (audioSource != null && audioState == "UnPause")
            audioSource.UnPause();
    }

    IEnumerator LoadAudioClip(string path)
    {
        WWW www = new WWW("file://" + path);

        while (!www.isDone)
            yield return null;

        audioSource.clip = www.GetAudioClip(false, false);
        audioSource.Play();
    }

    public void PauseAudio()
    {
        if (PatientManager.CurrentPlayer != null)
            photonView.RPC("RPC_PauseAudio", PatientManager.CurrentPlayer);
    }

    [PunRPC]
    public void RPC_PauseAudio()
    {
        if (audioSource.clip != null)
            audioSource.Pause();
    }
}
