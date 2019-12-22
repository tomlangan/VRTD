using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionUICanvas : MonoBehaviour
{
    public PlayerTargetManager TargetManager;
    public LineRenderer AnchorLine;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraDirection = transform.position - TargetManager.transform.position;
        transform.forward = cameraDirection.normalized;
    }

    public void EnableAndAttachToMapPos(MapPos pos)
    {
        Vector3 mapPosCenter = GameObjectFactory.MapPosToVec3(pos.Pos);
        Vector3 canvasCenter = new Vector3(mapPosCenter.x - 5.0F, mapPosCenter.y + 5.0F, mapPosCenter.z);

        AnchorLine.SetPosition(0, mapPosCenter);
        AnchorLine.SetPosition(1, canvasCenter);

        transform.position = canvasCenter;

        enabled = true;
        transform.gameObject.SetActive(true);
    }

    public void Disable()
    {
        enabled = false;
        transform.gameObject.SetActive(false);
    }
}
