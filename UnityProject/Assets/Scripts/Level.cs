using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;
using VRTD.Gameplay;



public enum LevelState { None, Loading, LevelSelect, WaveCountdown, Playing, StatsScreen }

public class Level : MonoBehaviour
{
    public GameObject RoadObject;
    public GameObject TerrainObject;
    public GameObject TurretSpaceObject;
    public GameObject BasicEnemy;
    public GameObject SwarmEnemy;
    public GameObject BasicTurret;
    public GameObject FireTurret;
    public GameObject IceTurret;
    public GameObject BasicBullet;
    public GameObject FireBullet;
    public GameObject IceBullet;
    public ListUI ListUITemplate;
    public PlayerTargetManager TargetManager;
    public GameObject TurretSelectUI;
    public InputPointer Pointer;
    public GameplayUIState GameplayUI;
    public MessageUI MessageUITemplate;

    //public UnityEngine.UI.Text TimerUIText;


    const float WAVE_COUNTDOWN_TIME = 5.0F;
    LevelDescription LevelDesc;
    WaveManager Waves = null;
    TurretManager Turrets = null;
    ProjectileManager Projectiles = null;
    public float GameTime;
    public float CountdownStartTime = 0.0F;
    public LevelState State = LevelState.None;
    bool Loading = false;

    private MessageUI CountdownUI;

    // Start is called before the first frame update
    void Start()
    {

        Debug.Log("Start()");


        Debug.Log("State ==> Loading");
        State = LevelState.Loading;
    }

    // Update is called once per frame
    void Update()
    {
        float elapsed = Time.deltaTime;
        GameTime += elapsed;

        switch (State)
        {
            case LevelState.Loading:
                if (!Loading)
                {
                    Loading = true;
                    GameTime = 0.0F;

                    GameObjectFactory.InitializeObjects(
                        BasicEnemy,
                        SwarmEnemy,
                        BasicTurret,
                        FireTurret,
                        IceTurret,
                        BasicBullet,
                        FireBullet,
                        IceBullet
                        );


                    Debug.Log("State ==> LevelSelect");
                    State = LevelState.LevelSelect;
                    ShowLevelSelectUI();
                    Loading = false;
                }
                break;

            case LevelState.LevelSelect:
                break;

            case LevelState.WaveCountdown:
                TickWaveCountdown();
                break;

            case LevelState.Playing:
                TickGameplay();
                break;

            case LevelState.StatsScreen:
                // For now, select level again
                Debug.Log("State ==> LevelSelect");
                State = LevelState.LevelSelect;
                ShowLevelSelectUI();
                break;
        }
    }

    void LoadLevel(string levelName)
    {
        GameTime = 0.0F;

        LevelDesc = LevelLoader.GetLevel(levelName);
        LevelLoader.LoadAndValidateLevel(LevelDesc);
        Waves = new WaveManager(LevelDesc);
        Turrets = new TurretManager(LevelDesc);
        Projectiles = new ProjectileManager();
        Debug.Log("Creating road objects");
        GameObjectFactory.CreateMapObjects(LevelDesc, RoadObject, TerrainObject, TurretSpaceObject);

        InitializeGameplayUISettings();
    }


    void TickWaveCountdown()
    {
        Debug.Assert((GameTime - CountdownStartTime) > 0);
        //TimerUIText.text = FormatTime(WAVE_COUNTDOWN_TIME - (GameTime - CountdownStartTime));

        float elapsed = GameTime - CountdownStartTime;

        if (elapsed < WAVE_COUNTDOWN_TIME)
        {
            int secsRemaining = (int)(WAVE_COUNTDOWN_TIME - elapsed + 0.5F);
            CountdownUI.Message = secsRemaining.ToString();
        }
        else
        {
            CountdownUI.transform.gameObject.SetActive(false);
            CountdownUI.enabled = false;

            Waves.AdvanceToNextWave(GameTime);
            Debug.Log("State ==> Playing");
            State = LevelState.Playing;
        }
    }

    void TickGameplay()
    {
        Waves.CurrentWave.Advance(GameTime);

        Turrets.Fire(GameTime);

        Projectiles.AdvanceAll(GameTime);

        if (Waves.IsComplete)
        {
            OnLevelComplete();
        }
        else if (Waves.CurrentWave.IsCompleted)
        {

            Debug.Log("State ==> WaveCountdown");
            ShowCountdownUI();
            State = LevelState.WaveCountdown;
            CountdownStartTime = GameTime;
        }

        if (null != Pointer.Hitting)
        {
            GameObject go = Pointer.Hitting;
            if (go.name.StartsWith("TurretSpace"))
            {
                TurretInstance turret = Turrets.GetTurretAtPosition(GameObjectFactory.Vec3ToMapPos(go.transform.position));

                if (null != turret)
                {
                    go = turret.go;
                }
            }

            GameplayUI.CursorOver(go);
        }
        
        bool value = false;
        if (Pointer.State.ButtonDown.TryGetValue(InputState.InputIntent.Selection, out value) && value)
        {
                GameplayUI.TriggerSelectAction();
        }

        //TimerUIText.text = FormatTime(GameTime - Waves.StartTime);
    }
    
    void OnLevelComplete()
    {
        Debug.Log("State ==> StatsScreen");
        State = LevelState.StatsScreen;
    }

    string FormatTime(float timeeInSec)
    {
        int minutes = Mathf.FloorToInt((float)timeeInSec / 60.0f);
        int seconds = Mathf.FloorToInt((float)timeeInSec - minutes * 60.0f);
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }


    private void ShowLevelSelectUI()
    {
        ListUIParams uiparams = new ListUIParams();
        uiparams.Title = "Select a Level";

        List<string> levels = LevelLoader.GetAllLevels();

        for (int i = 0; i < levels.Count; i++)
        {
            uiparams.Options.Add(levels[i]);
        }

        uiparams.Callback = OnLevelSelected;

        ListUI ui = Instantiate<ListUI>(ListUITemplate);

        ui.Create(uiparams);

        Vector3 uiPos = new Vector3(0.0F, 5.0F, -8.0F);
        Vector3 uiForward = (uiPos - TargetManager.transform.position).normalized;

        ui.transform.gameObject.SetActive(true);
        ui.transform.gameObject.transform.position = uiPos;
        ui.transform.gameObject.transform.forward = uiForward;
    }

    private bool OnLevelSelected(int index, string levelName)
    {
        LoadLevel(levelName);
        CountdownStartTime = GameTime;
        State = LevelState.WaveCountdown;
        ShowCountdownUI();

        return false;
    }


    private void ShowCountdownUI()
    {
        CountdownUI = Instantiate<MessageUI>(MessageUITemplate);

        CountdownUI.Title = "Get ready!";
        CountdownUI.Message = "5";

        Vector3 uiPos = new Vector3(0.0F, 5.0F, -8.0F);
        Vector3 uiForward = (uiPos - TargetManager.transform.position).normalized;

        CountdownUI.transform.gameObject.SetActive(true);
        CountdownUI.transform.gameObject.transform.position = uiPos;
        CountdownUI.transform.gameObject.transform.forward = uiForward;
    }


    private void InitializeGameplayUISettings()
    {
        ListUIParams TurretSelectParams = new ListUIParams();
        TurretSelectParams.Title = "Select Turret";
        

        for (int i = 0; i < LevelDesc.AllowedTurrets.Count; i++)
        {
            TurretSelectParams.Options.Add(LevelDesc.AllowedTurrets[i]);
        }

        TurretSelectParams.Callback = OnTurretSelected;

        GameplayUI.TurretSelectUIParams = TurretSelectParams;

        ListUIParams TurretOptionsParams = new ListUIParams();
        TurretOptionsParams.Title = "Turret Options";

        TurretOptionsParams.Options.Add("Upgrade");
        TurretOptionsParams.Options.Add("Sell");

        TurretOptionsParams.Callback = OnTurretOptionSelect;

        GameplayUI.TurretOptionUIParams = TurretOptionsParams;
    }


    private bool OnTurretSelected(int index, string turretName)
    {
        MapPos position = GameObjectFactory.Vec3ToMapPos(GameplayUI.SelectedObject.transform.position);
        Turrets.AddTurret(LevelDesc.AllowedTurrets[index], position, Projectiles);

        return false;
    }

    private bool OnTurretOptionSelect(int index, string option)
    {
        Debug.Log("Turret option selected: " + option);

        return false;
    }
}
