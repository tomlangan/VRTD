using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputHandler : MonoBehaviour
{
    public InputPointer InputDemuxer;
    public Ray InputDirection = new Ray();
    public OVRCameraRig OVRCamera;
    OVRInput.Controller ActiveController;

    // Start is called before the first frame update
    void Start()
    {

        // Set starting active hand
        OVRInput.Handedness handedness = OVRInput.GetDominantHand();
        switch (handedness)
        {
            case OVRInput.Handedness.LeftHanded:
                SetActiveHand(OVRInput.Controller.LTouch);
                break;
            case OVRInput.Handedness.RightHanded:
                SetActiveHand(OVRInput.Controller.RTouch);
                break;
            case OVRInput.Handedness.Unsupported:
                SetActiveHand(OVRInput.Controller.RTouch);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        Vector3 cameraPos = OVRCamera.transform.position;
        InputDirection.origin = OVRInput.GetLocalControllerPosition(ActiveController) + cameraPos;
        InputDirection.direction = OVRCamera.leftControllerAnchor.forward;
    }

    private void FixedUpdate()
    {
        OVRInput.FixedUpdate();
    }

    private void SetActiveHand(OVRInput.Controller controller)
    {
        ActiveController = controller;
    }
}
