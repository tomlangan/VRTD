using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public HandSimulator VirtualHand;
    public LineRenderer InputLine;
    Plane Ground;

    // Start is called before the first frame update
    void Start()
    {
        Ground = new Plane(Vector3.up, Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        Ray handRay = new Ray();
        if (null != VirtualHand)
        {
            handRay = VirtualHand.GetVirtualHandRay();
        }


        float enter;
        if (Ground.Raycast(handRay, out enter))
        {

            //Get the point that is clicked
            Vector3 groundIntersect = handRay.GetPoint(enter);

            Vector3[] linePoints = new Vector3[2];
            linePoints[0] = handRay.origin;
            linePoints[1] = groundIntersect;
            Debug.Log(" Pos[0] " + linePoints[0].ToString() + "  Pos[1] " + linePoints[1].ToString() + " Ray " + handRay.direction.ToString());
            InputLine.SetPositions(linePoints);
        }

    }

}
