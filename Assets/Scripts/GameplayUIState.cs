using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayUIState : MonoBehaviour
{
    public enum State { NothingSelected, TurretSpaceSelected, TurretSelected }

    public PlayerTargetManager TargetManager;
    public GameObject TurretSelectUI;

    // Cursor State
    public GameObject CursorObject = null;


    // Selection State
    public GameObject SelectedObject = null;


    private State UIState = State.NothingSelected;

    public void ClearAll()
    {
        if (null != SelectedObject)
        {
            ClearHalo(SelectedObject);
            SelectedObject = null;
        } 
        
        if (null != CursorObject)
        {
            ClearHalo(CursorObject);
            CursorObject = null;
        }

        UIState = State.NothingSelected;
    }

    public void CursorOver(MapPos pos, GameObject go)
    {
        switch (UIState)
        {
            case State.NothingSelected:
                if (CursorObject != go)
                {
                    // Clear previous cursor halo
                    if ((null != CursorObject) &&
                        (CursorObject.name.StartsWith("TurretSpace")))
                    {
                        ClearHalo(CursorObject);
                    }

                    // Set new cursor object
                    CursorObject = go;

                    // Set new halo if it needs one
                    if ((null != CursorObject) &&
                        (CursorObject.name.StartsWith("TurretSpace")))
                    {
                        SetHalo(CursorObject);
                    }
                }
                break;
            case State.TurretSpaceSelected:
                // Clear previous cursor halo if it's not the selected item
                if ((null != CursorObject) &&
                    (CursorObject != SelectedObject) && 
                    (CursorObject.name.StartsWith("TurretSpace")))
                {
                    ClearHalo(CursorObject);
                }

                // Set new cursor object
                CursorObject = go;

                // Set new halo if it needs one
                if ((null != CursorObject) &&
                    (CursorObject != SelectedObject) &&
                    (CursorObject.name.StartsWith("TurretSpace")))
                {
                    SetHalo(CursorObject);
                }
                break;
            case State.TurretSelected:
                // Clear previous cursor halo if it's not the selected item
                if ((null != CursorObject) &&
                    (CursorObject != SelectedObject) &&
                    (CursorObject.name.StartsWith("TurretSpace")))
                {
                    ClearHalo(CursorObject);
                }

                // Set new cursor object
                CursorObject = go;

                // Set new halo if it needs one
                if ((null != CursorObject) &&
                    (CursorObject != SelectedObject) &&
                    (CursorObject.name.StartsWith("TurretSpace")))
                {
                    SetHalo(CursorObject);
                }
                break;
        }
    }

    public void TriggerSelectAction()
    {
        switch (UIState)
        {
            case State.NothingSelected:
                if (null != CursorObject)
                {
                    AttemptSelectObjectFromClearState(CursorObject);
                }
                break;
            case State.TurretSpaceSelected:
                if (null != CursorObject)
                {
                    // If the user is clicking the original space, no effect
                    // otherwise there's work to do
                    if (CursorObject != SelectedObject)
                    {
                        ClearCurrentSelectedObject();
                    }

                    AttemptSelectObjectFromClearState(CursorObject);
                }
                break;
        }
    }

    private void AttemptSelectObjectFromClearState(GameObject go)
    {
        if (go.name.StartsWith("TurretSpace"))
        {
            SelectedObject = go;
            UIState = State.TurretSpaceSelected;
            ShowUIAttachedToObject(go, TurretSelectUI);
        }
    }

    private void ClearCurrentSelectedObject()
    {
        // TODO tear down turret space UI
        ClearHalo(SelectedObject);
        SelectedObject = null;
        ClearUI(TurretSelectUI);
    }

    public void TriggerDragStart()
    {

    }

    public void TriggerDragStop()
    {

    }

    private void SetHalo(GameObject go)
    {
        if (null != go)
        {
            Behaviour haloBehaviour = go.GetComponent<Behaviour>();
            if (null != haloBehaviour)
            {
                haloBehaviour.enabled = true; //Be careful if you have more than one Behaviour on your GameObject.
            }
        }
    }

    private void ClearHalo(GameObject go)
    {
        if (null != go)
        {
            Behaviour haloBehaviour = go.GetComponent<Behaviour>();
            if (null != haloBehaviour)
            {
                haloBehaviour.enabled = false; //Be careful if you have more than one Behaviour on your GameObject.
            }
        }
    }

    private void ShowUIAttachedToObject(GameObject anchor, GameObject ui)
    {
        Vector3 uiPos = new Vector3(anchor.transform.position.x - 1.0F, 2.0F, anchor.transform.position.z);
        Vector3 uiForward = (uiPos - TargetManager.transform.position).normalized;

        ui.SetActive(true);
        ui.transform.position = uiPos;
        ui.transform.forward = uiForward;
    }

    private void ClearUI(GameObject ui)
    {
        ui.SetActive(false);
    }
}
