using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopInputHandler : MonoBehaviour
{
    public InputPointer InputDemuxer;
    public GameObject VirtualHand;
    //This is the distance the clickable plane is from the camera. Set it in the Inspector before running.
    float DistanceZ = 0.5F;
    Plane MousePlane;
    public Vector3 VirtualHandPosition;
    public Vector3 VirtualHandDirection = new Vector3(0.0F, -0.5F, 0.5F);
    public Vector2 MiddleClickMouseMovement = new Vector2();
    private Vector2 PreviousMiddleClickMousePos = new Vector2();
    private bool wasMiddleClickEnabled = false;

    // Start is called before the first frame update
    void Start()
    {
        //This is how far away from the Camera the plane is placed
        Vector3 planeOrigin = Camera.main.transform.position + (Camera.main.transform.forward * (Camera.main.nearClipPlane + DistanceZ));

        MousePlane = new Plane(-Camera.main.transform.forward, planeOrigin);
    }

    // Update is called once per frame
    void Update()
    {
        Dictionary<InputState.InputIntent, bool> Buttons = new Dictionary<InputState.InputIntent, bool>();

        //Detect when there is a mouse click
        if (Input.mousePresent)
        {
            //Create a ray from the Mouse click position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            

            //Initialise the enter variable
            float enter = 0.0f;

            if (MousePlane.Raycast(ray, out enter))
            {
                //Get the point that is clicked
                VirtualHandPosition = ray.GetPoint(enter);

                //Move your cube GameObject to the point where you clicked
                VirtualHand.transform.position = VirtualHandPosition;
            }

            
            float xRelativePosFromCenter = (Input.mousePosition.x - (Screen.width / 2)) / Screen.width;
            float zPos = Input.mousePosition.y / Screen.height;
            float zRelativePosFromCenter = zPos - 0.5F;
            // Black magic that needs to change if we ever change the camera position!
            Vector3 SelectionRayTarget = new Vector3((70.0F * xRelativePosFromCenter) - 2.0F , 0.0F, (100.0F * zRelativePosFromCenter) + 15.0F);
            VirtualHandDirection = (SelectionRayTarget - VirtualHandPosition).normalized;

            InputDemuxer.SetCursorRay(new Ray(VirtualHandPosition, VirtualHandDirection));

            Buttons[InputState.InputIntent.Selection] = Input.GetMouseButtonDown(0);
            Buttons[InputState.InputIntent.Grab] = Input.GetMouseButtonDown(1);


            if (Input.GetMouseButton(2))
            {
                Vector2 currentPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (wasMiddleClickEnabled)
                {
                    MiddleClickMouseMovement = currentPos - PreviousMiddleClickMousePos;
                }
                PreviousMiddleClickMousePos = currentPos;
                wasMiddleClickEnabled = true;
            }
            else
            {
                MiddleClickMouseMovement = Vector2.zero;
                wasMiddleClickEnabled = false;
            }

            InputDemuxer.SetButtonState(Buttons);
        }
    }

    public Ray GetVirtualHandRay()
    {
        Ray handRay = new Ray(VirtualHandPosition, VirtualHandDirection);
        return handRay;
    }

}
