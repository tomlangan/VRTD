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
    public ListUI ListUITemplate;
    public PlayerTargetManager TargetManager;
    public GameObject TurretSelectUI;
    public InputPointer Pointer;
    public GameplayUIState GameplayUI;
    public MessageUI MessageUITemplate;
    public HUDUI HUDUITemplate;
    public GameObject ListTemplateTextOnly;
    public GameObject ListTemplateWithCoin;



    const float WAVE_COUNTDOWN_TIME = 5.0F;
    const float MISSILE_COUNTDOWN_TIME = 1.0F;
    LevelDescription LevelDesc;
    WaveManager Waves = null;
    TurretManager Turrets = null;
    ProjectileManager Projectiles = null;
    public float GameTime;
    public float CountdownStartTime = 0.0F;
    public LevelState State = LevelState.None;
    bool Loading = false;
    int Coin = 0;
    int LivesRemaining = 0;
    float LastMissileTime = 0.0F;

    private MessageUI CountdownUI;
    private HUDUI HUD;


    // Start is called before the first frame update
    void Start()
    {

        Utilities.Log("Start()");

        UnityEngine.Random.InitState(42);


        Utilities.Log("State ==> Loading");
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
                    PreloadAssets();

                    Utilities.Log("State ==> LevelSelect");
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
                Utilities.Log("State ==> LevelSelect");
                State = LevelState.LevelSelect;
                ShowLevelSelectUI();
                break;
        }
    }

    void LoadLevel(string levelName)
    {
        CleanupLevel();

        LevelDesc = LevelLoader.GetLevel(levelName);
        LevelLoader.LoadAndValidateLevel(LevelDesc);
        GameObjectFactory.Initialize(LevelDesc);
        Waves = new WaveManager(LevelDesc);
        Turrets = new TurretManager(LevelDesc);
        Projectiles = new ProjectileManager(LevelDesc);
        Utilities.Log("Creating road objects");
        GameObjectFactory.CreateMapObjects(LevelDesc);
        InitializeGameplayUISettings();

        Coin = LevelDesc.StartingCoins;
        LivesRemaining = LevelDesc.Lives;
        GameTime = 0.0F;
        LastMissileTime = 0.0F;

        CountdownStartTime = GameTime;
        State = LevelState.WaveCountdown;
        Waves.AdvanceToNextWave(GameTime);
        ShowHUDUI();
        ShowCountdownUI();
    }

    void CleanupLevel()
    {
        if (null != Projectiles)
        {
            Projectiles.DestroyAll();
            Projectiles = null;
        }
        if (null != Turrets)
        {
            Turrets.DestroyAll();
            Turrets = null;
        }
        if (null != Waves)
        {
            Waves.DestroyAll();
            Waves = null;
        }
        GameObjectFactory.Cleanup();
        LevelDesc = null;
    }

    void TickWaveCountdown()
    {
        Utilities.Assert((GameTime - CountdownStartTime) > 0);

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

            Utilities.Log("State ==> Playing");
            State = LevelState.Playing;
        }

        UpdateHUD();
    }

    void TickGameplay()
    {
        Waves.CurrentWave.Advance(GameTime);
        Coin += Waves.CurrentWave.ReportCoinEarned();
        LivesRemaining -= Waves.CurrentWave.ReportLivesLost();

        Projectiles.AdvanceAll(GameTime);

        Turrets.Fire(GameTime);

        if (Waves.IsComplete)
        {
            OnLevelComplete();
        }
        else if (Waves.CurrentWave.IsCompleted)
        {

            Utilities.Log("State ==> WaveCountdown");
            ShowCountdownUI();
            Waves.AdvanceToNextWave(GameTime);
            State = LevelState.WaveCountdown;
            CountdownStartTime = GameTime;
        }

        if (null != Pointer.Hitting)
        {
            GameObject go = Pointer.Hitting;
            if (go.name.StartsWith("TurretSpace"))
            {
                TurretInstance turret = Turrets.GetTurretAtPosition(GameObjectFactory.WorldVec3ToMapPos(go.transform.position));

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
        if (Pointer.State.ButtonDown.TryGetValue(InputState.InputIntent.MissileTrigger, out value) && value)
        {
            ProcessMissileTrigger();
        }


        UpdateHUD();
    }
    
    void OnLevelComplete()
    {
        Utilities.Log("State ==> StatsScreen");
        State = LevelState.StatsScreen;
        HideHUDUI();
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

        ui.Create(ListTemplateTextOnly, uiparams);

        Vector3 uiPos = new Vector3(0.0F, 5.0F, -8.0F);
        Vector3 uiForward = (uiPos - TargetManager.transform.position).normalized;

        ui.transform.gameObject.SetActive(true);
        ui.transform.gameObject.transform.position = uiPos;
        ui.transform.gameObject.transform.forward = uiForward;
    }

    private bool OnLevelSelected(int index, string levelName, object context)
    {
        LoadLevel(levelName);

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

    private void ShowHUDUI()
    {
        HUD = Instantiate<HUDUI>(HUDUITemplate);

        Vector3 uiPos = new Vector3(0.0F, 8.0F, 12.0F);
        Vector3 uiForward = (uiPos - TargetManager.transform.position).normalized;

        HUD.transform.gameObject.SetActive(true);
        HUD.transform.gameObject.transform.position = uiPos;
        HUD.transform.gameObject.transform.forward = uiForward;
        HUD.OnRestartLevelClicked += HUD_OnRestartLevelClicked;
    }

    private void HUD_OnRestartLevelClicked()
    {
        // Are we leaking a bunch of objects doing this?
        LoadLevel(LevelDesc.Name);
    }

    private void UpdateHUD()
    {
        HUD.CurrentWave = Waves.CurrentWave.EnemyType.Name;
        HUD.WavePosition = "Wave " + Waves.WavesStarted + " of " + LevelDesc.Waves.Count;
        HUD.Coin = Coin.ToString("#,##0");
        HUD.Lives = LivesRemaining.ToString();
        if (Waves.WavesStarted < LevelDesc.Waves.Count)
        {
            HUD.NextEnemy = LevelDesc.Waves[Waves.WavesStarted].Enemy;
        }
    }

    private void HideHUDUI()
    {
        HUD.enabled = false;
        HUD.transform.gameObject.SetActive(false);
    }

    private void InitializeGameplayUISettings()
    {
        ListUIParams TurretSelectParams = new ListUIParams();
        TurretSelectParams.Title = "Select Turret";
        

        for (int i = 0; i < LevelDesc.AllowedTurrets.Count; i++)
        {
            TurretSelectParams.Options.Add(LevelDesc.AllowedTurrets[i]);
            Turret t = LevelLoader.LookupTurret(LevelDesc.AllowedTurrets[i]);
            TurretSelectParams.Prices.Add(t.Cost.ToString()); 
        }

        TurretSelectParams.Callback = OnTurretSelected;

        GameplayUI.TurretSelectUIParams = TurretSelectParams;

        ListUIParams TurretOptionsParams = new ListUIParams();
        TurretOptionsParams.Title = "Turret Options";

        TurretOptionsParams.Callback = OnTurretOptionSelect;

        GameplayUI.TurretOptionUIParams = TurretOptionsParams;

        GameplayUI.TurretManagerInstance = Turrets;
    }


    private bool OnTurretSelected(int index, string turretName, object context)
    {
        MapPos position = GameObjectFactory.WorldVec3ToMapPos(GameplayUI.SelectedObject.transform.position);
        Turret turretSelected = LevelLoader.LookupTurret(LevelDesc.AllowedTurrets[index]);

        if (Coin >= turretSelected.Cost)
        {
            Turrets.AddTurret(LevelLoader.LookupTurret(LevelDesc.AllowedTurrets[index]), position, Projectiles);
            Coin -= turretSelected.Cost;
        }

        return false;
    }

    private bool OnTurretOptionSelect(int index, string option, object context)
    {
        if ("Sell" == option)
        {
            TurretInstance t = (TurretInstance)context;
            Turrets.RemoveTurret(t);
            Coin += (t.TurretType.Cost / 2);
        }

        return false;
    }

    private void ProcessMissileTrigger()
    {
        float timeSinceLastMissile = GameTime - LastMissileTime;
        if (timeSinceLastMissile >= MISSILE_COUNTDOWN_TIME)
        {

            RaycastHit hitinfo = new RaycastHit();
            if (Physics.Raycast(Pointer.State.SecondaryHandRay, out hitinfo))
            {
                if (hitinfo.transform.gameObject.tag == "Grass"
                    ||
                    hitinfo.transform.gameObject.tag == "Road")
                {
                    Projectiles.Fire("Hand Missile", Pointer.State.SecondaryHandRay.origin, Pointer.State.SecondaryHandRay.direction, hitinfo.distance, GameTime);
                    LastMissileTime = GameTime;
                    Utilities.Log("Fired hand missile");
                }
                else if (hitinfo.transform.gameObject.tag == "Enemy")
                {
                    EnemyInstance enemy = Waves.CurrentWave.GetEnemyFromGameObject(hitinfo.transform.gameObject);
                    if (null == enemy)
                    {
                        throw new Exception("Couldn't find enemy from GameObject lookup!");
                    }
                    Projectiles.Fire("Hand Missile", Pointer.State.SecondaryHandRay.origin, enemy, GameTime);
                    LastMissileTime = GameTime;
                    Utilities.Log("Fired hand missile");
                }
            }
        }
    }

    private void PreloadAssets()
    {
        GameObject loaded = GameObjectFactory.InstantiateObject("HandMissile");
        GameObjectFactory.DestroyObject(loaded);
    }
}
