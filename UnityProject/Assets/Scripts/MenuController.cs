using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public PlayerTargetManager TargetManager;
    public DesktopInputHandler DesktopInput;

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
        UpdateSelfAvatar();
    }

    void UpdateSelfAvatar()
    {
        if (TargetManager.Target == PlayerTargetManager.TargetPlatform.VR)
        {
        }
        else if (TargetManager.Target == PlayerTargetManager.TargetPlatform.Desktop)
        {
            TargetManager.MainCamera.transform.Rotate(-DesktopInput.MiddleClickMouseMovement.y * 0.1F, DesktopInput.MiddleClickMouseMovement.x * 0.1F, 0.0F);
        }
    }
}
