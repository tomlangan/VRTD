using System.Collections;
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
    public UnityEngine.UI.Text TimerUIText;


    const double WAVE_COUNTDOWN_TIME = 5.0;
    LevelDescription LevelDesc;
    WaveManager Waves = null;
    TurretManager Turrets = null;
    ProjectileManager Projectiles = null;
    public double GameTime;
    public double CountdownStartTime = 0.0;
    public LevelState State = LevelState.None;

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
        double elapsed = Time.deltaTime;
        GameTime += elapsed;

        switch (State)
        {
            case LevelState.Loading:
                GameTime = 0.0;
                LoadLevel();
                Debug.Log("State ==> WaveCountdown");
                State = LevelState.WaveCountdown;
                CountdownStartTime = GameTime;
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
        GameTime = 0.0;

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
        TimerUIText.text = FormatTime(WAVE_COUNTDOWN_TIME - (GameTime - CountdownStartTime));

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
        TimerUIText.text = FormatTime(GameTime - Waves.StartTime);
    }
    
    void OnLevelComplete()
    {
        Debug.Log("State ==> StatsScreen");
        State = LevelState.StatsScreen;
    }

    string FormatTime(double timeeInSec)
    {
        int minutes = Mathf.FloorToInt((float)timeeInSec / 60.0f);
        int seconds = Mathf.FloorToInt((float)timeeInSec - minutes * 60.0f);
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }
}
