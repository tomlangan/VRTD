using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using VRTD.Gameplay;

public class InputState
{ 
    public enum InputIntent { Selection, MissileTrigger }
    public static InputIntent[] Intents = { InputIntent.Selection, InputIntent.MissileTrigger };    

    public Dictionary<InputIntent, bool> ButtonDown = new Dictionary<InputIntent, bool>();
    public Dictionary<InputIntent, bool> ButtonUp = new Dictionary<InputIntent, bool>();
    public Dictionary<InputIntent, bool> ButtonState = new Dictionary<InputIntent, bool>();

    public Ray DominantHandRay;
    public Ray SecondaryHandRay;

    public Vector3 DominantHandPointingAt;
    public bool DominantHandHasHitTarget;
}

public class InputPointer : MonoBehaviour
{

	public PlayerTargetManager Player;
	public LineRenderer InputLine;
    public GameObject cursorVisual;
    public float maxLength = 100.0f;
    public GameObject Hitting;
    public InputState State;


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
        State.DominantHandRay.origin = start;
        State.DominantHandRay.direction = (dest - start).normalized;
        State.DominantHandPointingAt = dest;
        State.DominantHandHasHitTarget = true;
    }

    public void SetInputState(Dictionary<InputState.InputIntent, bool> buttonState, Ray dominantHandRay, Ray secondaryHandRay)
    {
        State.ButtonDown.Clear();
        State.ButtonUp.Clear();
        foreach (InputState.InputIntent i in InputState.Intents)
        {
            bool value = false;
            if (!buttonState.TryGetValue(i, out value))
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
        State.DominantHandRay = dominantHandRay;
        State.SecondaryHandRay = secondaryHandRay;
        State.DominantHandHasHitTarget = false;
    }

    private void LateUpdate()
    {
        InputLine.SetPosition(0, State.DominantHandRay.origin);
        if (State.DominantHandHasHitTarget)
        {
            InputLine.SetPosition(1, State.DominantHandPointingAt);
            UpdateLine(State.DominantHandRay.origin, (State.DominantHandPointingAt - State.DominantHandRay.origin).normalized);
            if (cursorVisual)
            {
                cursorVisual.transform.position = State.DominantHandPointingAt;
                cursorVisual.SetActive(true);
            }
        }
        else
        {
            UpdateLine(State.DominantHandRay.origin, State.DominantHandRay.direction);
            InputLine.SetPosition(1, State.DominantHandRay.origin + maxLength * State.DominantHandRay.direction);
            if (cursorVisual) cursorVisual.SetActive(false);
        }
    }

    // make laser beam a behavior with a prop that enables or disables
    private void UpdateLine(Vector3 start, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(start, direction, out hit))
        {
            Hitting = hit.transform.gameObject;
        }
    }


    void OnDisable()
    {
        if (cursorVisual)
        {
            cursorVisual.SetActive(false);
        }
    }

}
