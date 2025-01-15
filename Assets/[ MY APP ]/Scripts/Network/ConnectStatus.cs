using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class ConnectStatus : MonoBehaviour
{
    private readonly string connectionStatusMessage = "Statut de connection: ";
    private readonly string connectionDeviceMessage = "Nbr. d'appareils: ";

    public void Update()
    {
        UIManager.StatusConnectText.StringReference.Arguments = new object[] { PhotonNetwork.NetworkClientState };
        UIManager.DeviceConnectText.StringReference.Arguments = new object[] { PhotonNetwork.PlayerList.Length > 0 ? PhotonNetwork.PlayerList.Length : 0 };
    }
}
