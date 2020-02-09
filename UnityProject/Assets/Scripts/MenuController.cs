using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public PlayerTargetManager TargetManager;

    private Button StartButton;

    // Start is called before the first frame update
    void Start()
    {
        TargetManager.InputMode = PlayerTargetManager.InputTarget.MenuScene;
        StartButton = FindObjectOfType<Button>();
        StartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("CastleScene");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
