﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerTargetManager : MonoBehaviour
{
    public enum TargetPlatform { Desktop, VR }
    public enum InputTarget { MenuScene, TowerDefenseScene }

    public OVRCameraRig OVRCamera;
	public Camera MainCamera;
    public DesktopInputHandler DesktopInput;
    public VRInputHandler VRInput;
    public OvrAvatar LocalAvatar;
    public TargetPlatform Target = TargetPlatform.VR;
    public InputTarget InputMode
    {
        get
        {
            return inputMode;
        }
        set
        {
            inputMode = value;
            DesktopInput.InputMode = inputMode;
        }
    }
    private InputTarget inputMode = InputTarget.TowerDefenseScene;
    private GameObject RocketLauncherArm = null;

    public Transform CameraTransform
    {
        get
        {
            if (TargetPlatform.Desktop == Target)
            {
                return MainCamera.transform;
            }
            else 
            {
                return OVRCamera.transform;
            }
        }
    }

	// Start is called before the first frame update
	void Start()
    {
        RocketLauncherArm = GameObject.FindWithTag("RocketLauncherArm");

        if (TargetPlatform.Desktop == Target)
        {
            MainCamera.enabled = true;
            MainCamera.transform.gameObject.SetActive(true);
            DesktopInput.enabled = true;
            DesktopInput.transform.gameObject.SetActive(true);
            VRInput.transform.gameObject.SetActive(false);
            VRInput.enabled = false;
            LocalAvatar.transform.gameObject.SetActive(false);
            LocalAvatar.enabled = false;
            OVRCamera.enabled = false;
            OVRCamera.transform.gameObject.SetActive(false);
            OVRRaycaster.WorldCamera = MainCamera;
        }
        else if (TargetPlatform.VR == Target)
        {
            MainCamera.enabled = false;
            MainCamera.transform.gameObject.SetActive(false);
            DesktopInput.enabled = false;
            DesktopInput.transform.gameObject.SetActive(false);
            OVRCamera.enabled = true;
            OVRCamera.transform.gameObject.SetActive(true);
            LocalAvatar.transform.gameObject.SetActive(true);
            LocalAvatar.enabled = true;
            VRInput.transform.gameObject.SetActive(true);
            VRInput.enabled = true;
            OVRRaycaster.WorldCamera = OVRCamera.centerEyeAnchor.gameObject.GetComponent<Camera>();
        }
    }

    // Update is called once per frame
   void Update()
    {
        if (TargetPlatform.VR == Target)
        {
            RocketLauncherArm.transform.position = OVRCamera.leftControllerAnchor.position;
            RocketLauncherArm.transform.forward = OVRCamera.leftControllerAnchor.forward;
            RocketLauncherArm.transform.rotation = OVRCamera.leftControllerAnchor.rotation;
        }
        else if (TargetPlatform.Desktop == Target)
        {
            MainCamera.transform.Rotate(-DesktopInput.MiddleClickMouseMovement.y * 0.1F, DesktopInput.MiddleClickMouseMovement.x * 0.1F, 0.0F);
            RocketLauncherArm.transform.forward = DesktopInput.VirtualHandDirection;
            RocketLauncherArm.transform.position = DesktopInput.VirtualHandPosition;
        }
    }
}
