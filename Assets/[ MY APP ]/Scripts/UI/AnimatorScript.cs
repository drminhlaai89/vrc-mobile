using Pico.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorScript : MonoBehaviour
{
    bool isExit = false;

    public void ExecuteScript()
    {
        if (!isExit)
        {
            UIManager.ActivePanel(UIManager.RoomPanel, UIManager.PanelList);
            isExit = true;
        }
        else
        {
            UIManager.ActivePanel(UIManager.LoginPanel, UIManager.PanelList);
            isExit = false;
        }
    }
}
