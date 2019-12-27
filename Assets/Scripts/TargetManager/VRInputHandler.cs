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

        //
        // Update dominant hand
        //

        bool leftPressed = OVRInput.Get(OVRInput.Button.Any, OVRInput.Controller.LTouch);
        bool rightPressed = OVRInput.Get(OVRInput.Button.Any, OVRInput.Controller.RTouch);

        if (leftPressed && !rightPressed)
        {
            SetActiveHand(OVRInput.Controller.LTouch);
        }
        else if (!leftPressed && rightPressed)
        {
            SetActiveHand(OVRInput.Controller.RTouch);
        }

        //
        // Update buttons pressed
        //

        Dictionary<InputState.InputIntent, bool> Buttons = new Dictionary<InputState.InputIntent, bool>();


        if (ActiveController == OVRInput.Controller.LTouch)
        {
            Buttons[InputState.InputIntent.Selection] = OVRInput.Get(OVRInput.Button.One, ActiveController) || OVRInput.Get(OVRInput.RawButton.LIndexTrigger, ActiveController);
            Buttons[InputState.InputIntent.Grab] = OVRInput.Get(OVRInput.RawButton.LHandTrigger, ActiveController);
        }
        else
        {
            Buttons[InputState.InputIntent.Selection] = OVRInput.Get(OVRInput.Button.One, ActiveController) || OVRInput.Get(OVRInput.RawButton.RIndexTrigger, ActiveController);
            Buttons[InputState.InputIntent.Grab] = OVRInput.Get(OVRInput.RawButton.RHandTrigger, ActiveController);
        }

        InputDemuxer.SetButtonState(Buttons);

        //
        // Update input direction
        //

        Vector3 cameraPos = OVRCamera.transform.position;
        InputDirection.origin = OVRInput.GetLocalControllerPosition(ActiveController) + cameraPos;
        if (ActiveController == OVRInput.Controller.RTouch)
        {
            InputDirection.direction = OVRCamera.rightControllerAnchor.forward;
        }
        else
        {
            InputDirection.direction = OVRCamera.leftControllerAnchor.forward;
        }
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
