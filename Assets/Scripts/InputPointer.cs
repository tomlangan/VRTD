using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPointer : MonoBehaviour
{
	public PlayerTargetManager Player;
	public LineRenderer InputLine;
    public GameObject cursorVisual;
    public float maxLength = 10.0f;


    private Vector3 _startPoint;
    private Vector3 _forward;
    private Vector3 _endPoint;
    private bool _hitTarget;

    private void Start()
    {
        if (cursorVisual) cursorVisual.SetActive(false);
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
    }

    void OnDisable()
    {
        if (cursorVisual) cursorVisual.SetActive(false);
    }

}
