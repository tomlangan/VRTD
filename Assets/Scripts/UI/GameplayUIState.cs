using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayUIState : MonoBehaviour
{
    public enum State { NothingSelected, TurretSpaceSelected, TurretSelected }

    public PlayerTargetManager TargetManager;

    // Cursor State
    public GameObject CursorObject = null;


    // Selection State
    public GameObject SelectedObject = null;

    // Templates
    public ListUI ListUITemplate;

    // Data for UI scenes
    public List<string> AllowedTurretNames;

    private State UIState = State.NothingSelected;
    private ListUI TurretSelectUIObject = null;

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

        if (null != TurretSelectUIObject)
        {
            GameObject.Destroy(TurretSelectUIObject);
            TurretSelectUIObject = null;
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
            TurretSelectUIObject = CreateTurretSelectUI();
            ShowUIAttachedToObject(go, TurretSelectUIObject.transform.gameObject);
        }
    }

    private void ClearCurrentSelectedObject()
    {
        // TODO tear down turret space UI
        ClearHalo(SelectedObject);
        SelectedObject = null;

        if (null != TurretSelectUIObject)
        {
            ClearUI(TurretSelectUIObject.transform.gameObject);
            GameObject.Destroy(TurretSelectUIObject.transform.gameObject);
            TurretSelectUIObject = null;
        }
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
        float xPos = ((anchor.transform.position.x > 0) ? (anchor.transform.position.x + 5.0F) : (anchor.transform.position.x - 5.0F));
        Vector3 uiPos = new Vector3(xPos, 3.0F, anchor.transform.position.z);
        Vector3 uiForward = (uiPos - TargetManager.transform.position).normalized;

        
        ui.SetActive(true);
        ui.transform.position = uiPos;
        ui.transform.forward = uiForward;
    }

    private void ClearUI(GameObject ui)
    {
        ui.SetActive(false);
    }

    private ListUI CreateTurretSelectUI()
    {
        ListUI go = Instantiate<ListUI>(ListUITemplate);

        go.Create("Build Turret", AllowedTurretNames, OnTurretUISelected);

        return go;
    }

    void OnTurretUISelected(int index, string turretName)
    {
        Debug.Log(" TURRET SELECTED: [" + index + "] - " + turretName);
    }
}
