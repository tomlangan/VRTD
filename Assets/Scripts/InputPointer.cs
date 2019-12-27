using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputState
{ 
    public enum InputIntent { Selection, Grab }
    public static InputIntent[] Intents = { InputIntent.Selection, InputIntent.Grab };    

    public Dictionary<InputIntent, bool> ButtonDown = new Dictionary<InputIntent, bool>();
    public Dictionary<InputIntent, bool> ButtonUp = new Dictionary<InputIntent, bool>();
    public Dictionary<InputIntent, bool> ButtonState = new Dictionary<InputIntent, bool>();
}

public class InputPointer : MonoBehaviour
{

	public PlayerTargetManager Player;
	public LineRenderer InputLine;
    public GameObject cursorVisual;
    public float maxLength = 100.0f;
    public GameObject Hitting;
    public InputState State;


    private Vector3 _startPoint;
    private Vector3 _forward;
    private Vector3 _endPoint;
    private bool _hitTarget;

    private void Start()
    {
        State = new InputState();

        if (cursorVisual) cursorVisual.SetActive(false);
    }

    public void Update()
    {
    }

    public void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)
    {
        _startPoint = start;
        _endPoint = dest;
        _hitTarget = true;
    }

    public void SetCursorRay(Ray r)
    {
        _startPoint = r.origin;
        _forward = r.direction;
        _hitTarget = false;
    }

    public void SetButtonState(Dictionary<InputState.InputIntent, bool> ButtonState)
    {
        State.ButtonDown.Clear();
        State.ButtonUp.Clear();
        foreach (InputState.InputIntent i in InputState.Intents)
        {
            bool value = false;
            if (!ButtonState.TryGetValue(i, out value))
            {
                value = false;
            }
            bool previousState = false;
            if ((!State.ButtonState.TryGetValue(i, out previousState) || !previousState) && value)
            {
                State.ButtonDown[i] = true;
            }
            if (previousState && !value)
            {
                State.ButtonUp[i] = true;
            }
            State.ButtonState[i] = value;
        }
    }

    private void LateUpdate()
    {
        InputLine.SetPosition(0, _startPoint);
        if (_hitTarget)
        {
            InputLine.SetPosition(1, _endPoint);
            UpdateLine(_startPoint, _endPoint);
            if (cursorVisual)
            {
                cursorVisual.transform.position = _endPoint;
                cursorVisual.SetActive(true);
            }
        }
        else
        {
            UpdateLine(_startPoint, _startPoint + maxLength * _forward);
            InputLine.SetPosition(1, _startPoint + maxLength * _forward);
            if (cursorVisual) cursorVisual.SetActive(false);
        }
    }

    // make laser beam a behavior with a prop that enables or disables
    private void UpdateLine(Vector3 start, Vector3 end)
    {
        InputLine.SetPosition(0, start);
        InputLine.SetPosition(1, end);

        RaycastHit hit;
        if (Physics.Raycast(start, end, out hit))
        {
            Hitting = hit.transform.gameObject;
        }
    }


    void OnDisable()
    {
        if (cursorVisual) cursorVisual.SetActive(false);
    }

}
