using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using VRTD.Gameplay;



public class MessageUI : MonoBehaviour
{
    public string Title = "";
    public string Message = "";
    public string Countdown = "";

    private Text TitleTextField;
    private Text MessageTextField;
    private Text CountdownTextField;

    // Start is called before the first frame update
    void Start()
    {
        Text[] textFields = gameObject.GetComponentsInChildren<Text>();
        TitleTextField = textFields[0];
        MessageTextField = textFields[1];
        CountdownTextField = textFields[2];
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(Title))
        {
            TitleTextField.text = Title;
        }
        else
        {
            TitleTextField.enabled = false;
        }

        if (!string.IsNullOrEmpty(Message))
        {
            MessageTextField.text = Message;
        }
        else
        {
            MessageTextField.enabled = false;
        }

        if (!string.IsNullOrEmpty(Countdown))
        {
            CountdownTextField.text = Countdown;
        }
        else
        {
            CountdownTextField.enabled = false;
        }
    }
}
