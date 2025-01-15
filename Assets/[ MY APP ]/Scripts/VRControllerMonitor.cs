using UnityEngine;

public class VRControllerMonitor : MonoBehaviour
{
    public float timeToHide = 3.0f; // Time in seconds after which controller will hide
    public SkinnedMeshRenderer controllerRenderer;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float lastMoveTime;


    private void Start()
    {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private void Update()
    {
        bool hasMoved = Vector3.Distance(lastPosition, transform.position) > 0.001f;
        bool hasRotated = Quaternion.Angle(lastRotation, transform.rotation) > 0.5f;

        if (hasMoved || hasRotated)
        {
            // Controller has moved or rotated
            lastMoveTime = Time.time;
            ShowController(true);
        }
        else
        {
            // Controller hasn't moved or rotated for a duration
            if (Time.time - lastMoveTime > timeToHide)
            {
                ShowController(false);
            }
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void ShowController(bool show)
    {
        controllerRenderer.enabled= show;
    }
}
