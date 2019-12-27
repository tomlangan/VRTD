﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;



public enum LevelState { None, Loading, WaveCountdown, Playing, StatsScreen }

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
    public GameObject TurretSelectUI;
    public InputPointer Pointer;
    public GameplayUIState GameplayUI;

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

                    LoadLevel();


                    InitializeUISettings();

                    Debug.Log("State ==> WaveCountdown");
                    State = LevelState.WaveCountdown;
                    CountdownStartTime = GameTime;
                    Loading = false;
                }
                break;

            case LevelState.WaveCountdown:
                TickWaveCountdown();
                break;

            case LevelState.Playing:
                TickGameplay();
                break;

            case LevelState.StatsScreen:

                break;
        }
    }

    void LoadLevel()
    {
        GameTime = 0.0F;

        LevelDesc = LevelLoader.GetTestLevel();
        LevelLoader.LoadAndValidateLevel(LevelDesc);
        Waves = new WaveManager(LevelDesc);
        Turrets = new TurretManager(LevelDesc);
        Projectiles = new ProjectileManager();
        Debug.Log("Creating road objects");
        GameObjectFactory.CreateMapObjects(LevelDesc, RoadObject, TerrainObject, TurretSpaceObject);
    }


    void TickWaveCountdown()
    {
        Debug.Assert((GameTime - CountdownStartTime) > 0);
        //TimerUIText.text = FormatTime(WAVE_COUNTDOWN_TIME - (GameTime - CountdownStartTime));

        if ((GameTime - CountdownStartTime) > WAVE_COUNTDOWN_TIME)
        {
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
            State = LevelState.WaveCountdown;
            CountdownStartTime = GameTime;
        }

        if (null != Pointer.Hitting)
        {
            GameplayUI.CursorOver(Pointer.Hitting);
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

    private void InitializeUISettings()
    {
        ListUIParams TurretParams = new ListUIParams();
        TurretParams.Title = "Select Turret";
        

        for (int i = 0; i < LevelDesc.AllowedTurrets.Count; i++)
        {
            TurretParams.Options.Add(LevelDesc.AllowedTurrets[i].Name);
        }

        TurretParams.Callback = OnTurretSelected;

        GameplayUI.TurretSelectUIParams = TurretParams;
    }

    private bool OnTurretSelected(int index, string turretName)
    {
        MapPos position = GameObjectFactory.Vec3ToMapPos(GameplayUI.SelectedObject.transform.position);
        Turrets.AddTurret(LevelDesc.AllowedTurrets[index], position, Projectiles);

        return false; 
    }
}
