using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopInputHandler : MonoBehaviour
{
    public GameObject VirtualHand;
    //This is the distance the clickable plane is from the camera. Set it in the Inspector before running.
    float DistanceZ = 0.5F;
    Plane MousePlane;
    public Vector3 VirtualHandPosition;
    public Vector3 VirtualHandDirection = new Vector3(0.0F, -0.5F, 0.5F);
    public Vector2 RightClickMouseMovement = new Vector2();
    private Vector2 PreviousRightClickMousePos = new Vector2();
    private bool wasRightClickEnabled = false;

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
            Vector3 SelectionRayTarget = new Vector3(((40.0F - (12.0F * (1.0F-zPos))) * xRelativePosFromCenter) - 2.0F , 0.0F, (21.0F * zRelativePosFromCenter) + 2.0F);
            VirtualHandDirection = (SelectionRayTarget - VirtualHandPosition).normalized; 

            if (Input.GetMouseButton(1))
            {
                Vector2 currentPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                if (wasRightClickEnabled)
                {
                    RightClickMouseMovement = currentPos - PreviousRightClickMousePos;
                }
                PreviousRightClickMousePos = currentPos;
                wasRightClickEnabled = true;
            }
            else
            {
                RightClickMouseMovement = Vector2.zero;
                wasRightClickEnabled = false;
            }
        }
    }

    public Ray GetVirtualHandRay()
    {
        Ray handRay = new Ray(VirtualHandPosition, VirtualHandDirection);
        return handRay;
    }

}
