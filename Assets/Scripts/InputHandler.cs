using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public HandSimulator VirtualHand;
    public LineRenderer InputLine;
    public Ray InputDirection = new Ray();
    public OVRCameraRig OVRCamera;
    Plane Ground;

    // Start is called before the first frame update
    void Start()
    {
        Ground = new Plane(Vector3.up, Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        OVRInput.Controller activeController = OVRInput.GetActiveController();

        if (activeController == OVRInput.Controller.LTouch)
        {
            Vector3 cameraPos = OVRCamera.transform.position;
            InputDirection.origin = OVRInput.GetLocalControllerPosition(activeController) + cameraPos;
            InputDirection.direction = OVRCamera.leftControllerAnchor.forward;
        }
        else if ((activeController == OVRInput.Controller.RTouch) || (activeController == OVRInput.Controller.Touch))
        {
            Vector3 cameraPos = OVRCamera.transform.position;
            Vector3 ovrPos = OVRInput.GetLocalControllerPosition(activeController);
            Vector3 anchorPos = OVRCamera.rightControllerAnchor.position;
            InputDirection.origin = ovrPos + cameraPos;
            InputDirection.direction = OVRCamera.rightControllerAnchor.forward;
        }
        else if (null != VirtualHand)
        {
            InputDirection = VirtualHand.GetVirtualHandRay();
        }

        Vector3[] linePoints = new Vector3[2];
        linePoints[0] = InputDirection.origin;
        linePoints[1] = InputDirection.origin + (InputDirection.direction * 50.0F);
        InputLine.SetPositions(linePoints);

        /*
        float enter;
        if (Ground.Raycast(InputDirection, out enter))
        {

            //Get the point that is clicked
            Vector3 groundIntersect = InputDirection.GetPoint(enter);

        }
        */
    }

    private void FixedUpdate()
    {
        OVRInput.FixedUpdate();
    }
}
