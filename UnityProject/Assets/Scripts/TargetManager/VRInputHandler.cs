using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInputHandler : MonoBehaviour
{
    public InputPointer InputDemuxer;
    //public Ray InputDirection = new Ray();
    public OVRCameraRig OVRCamera;
    OVRInput.Controller DominantHand;
    Ray DominantHandRay = new Ray();
    Ray SecondaryHandRay = new Ray();

    // Start is called before the first frame update
    void Start()
    {

        // Set starting active hand
        OVRInput.Handedness handedness = OVRInput.GetDominantHand();
        switch (handedness)
        {
            case OVRInput.Handedness.LeftHanded:
                DominantHand = OVRInput.Controller.LTouch;
                break;
            case OVRInput.Handedness.RightHanded:
                DominantHand = OVRInput.Controller.RTouch;
                break;
            case OVRInput.Handedness.Unsupported:
                DominantHand = OVRInput.Controller.RTouch;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        //
        // Update buttons pressed
        //

        Dictionary<InputState.InputIntent, bool> Buttons = new Dictionary<InputState.InputIntent, bool>();


        if (DominantHand == OVRInput.Controller.LTouch)
        {
            Buttons[InputState.InputIntent.Selection] = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch) || OVRInput.Get(OVRInput.RawButton.LIndexTrigger, OVRInput.Controller.LTouch);
            Buttons[InputState.InputIntent.MissileTrigger] = OVRInput.Get(OVRInput.RawButton.RIndexTrigger, OVRInput.Controller.RTouch);
        }
        else
        {
            Buttons[InputState.InputIntent.Selection] = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch) || OVRInput.Get(OVRInput.RawButton.RIndexTrigger, OVRInput.Controller.RTouch);
            Buttons[InputState.InputIntent.MissileTrigger] = OVRInput.Get(OVRInput.RawButton.LIndexTrigger, OVRInput.Controller.LTouch);
        }


        //
        // Update input direction
        //

        Vector3 cameraPos = OVRCamera.transform.position;
        if (DominantHand == OVRInput.Controller.RTouch)
        {
            DominantHandRay.origin = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch) + cameraPos;
            DominantHandRay.direction = OVRCamera.rightControllerAnchor.forward;
            SecondaryHandRay.origin = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch) + cameraPos;
            SecondaryHandRay.direction = OVRCamera.leftControllerAnchor.forward;
        }
        else
        {
            SecondaryHandRay.origin = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch) + cameraPos;
            SecondaryHandRay.direction = OVRCamera.rightControllerAnchor.forward;
            DominantHandRay.origin = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch) + cameraPos;
            DominantHandRay.direction = OVRCamera.leftControllerAnchor.forward;
        }

        InputDemuxer.SetInputState(Buttons, DominantHandRay, SecondaryHandRay);
    }

    private void FixedUpdate()
    {
        OVRInput.FixedUpdate();
    }

    private void SetDominantHand(OVRInput.Controller controller)
    {
        DominantHand = controller;
    }
}
