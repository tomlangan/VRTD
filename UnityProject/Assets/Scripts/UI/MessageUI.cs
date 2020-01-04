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

    private Text TitleTextField;
    private Text MessageTextField;

    // Start is called before the first frame update
    void Start()
    {
        Text[] textFields = gameObject.GetComponentsInChildren<Text>();
        TitleTextField = textFields[0];
        MessageTextField = textFields[1];
    }

    // Update is called once per frame
    void Update()
    {
        TitleTextField.text = Title;
        MessageTextField.text = Message;
    }
}
