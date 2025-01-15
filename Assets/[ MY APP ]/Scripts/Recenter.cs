using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Recenter : MonoBehaviour
{
    public List<GameObject> objects = new List<GameObject>();
    public Camera mainCamera;

    public XRController leftController;
    public XRController rightController;
    public InputHelpers.Button menuButton = InputHelpers.Button.MenuButton;

    void Update()
    {
        CheckMenuButtonPress(leftController);
        CheckMenuButtonPress(rightController);
    }

    private void CheckMenuButtonPress(XRController controller)
    {
        if (controller)
        {
            bool isPressed;
            controller.inputDevice.IsPressed(menuButton, out isPressed);
            if (isPressed)
            {
                OnMenuButtonPressed();
            }
        }
    }

    private void OnMenuButtonPressed()
    {
        objects.ForEach(obj =>
        {
            Quaternion rotation = mainCamera.transform.rotation * Quaternion.Euler(new Vector3(0, -90, 0));
            obj.transform.rotation = rotation;
        });

    }
}
