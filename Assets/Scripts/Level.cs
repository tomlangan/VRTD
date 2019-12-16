using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;



public enum LevelState { None, Loading, WaveCountdown, Playing, StatsScreen }

public class Level : MonoBehaviour
{
    const double WAVE_COUNTDOWN_TIME = 5.0;
    public LevelDescription LevelDesc;
    WaveManager Waves = null;
    TurretManager Turrets = null;
    ProjectileManager Projectiles = null;
    public double GameTime;
    public double CountdownStartTime = 0.0;
    bool Loading = false;
    public LevelState State = LevelState.None;

    // Start is called before the first frame update
    void Start()
    {
        State = LevelState.Loading;
        GameTime = 0.0;
        LoadLevel();
    }

    // Update is called once per frame
    void Update()
    {
        double elapsed = Time.deltaTime;
        GameTime += elapsed;

        switch (State)
        {
            case LevelState.Loading:
                if (!Loading)
                {
                    State = LevelState.WaveCountdown;
                    CountdownStartTime = GameTime;
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
        GameTime = 0.0;
        Loading = true;

        Task t = new Task(() =>
        {
            LevelDesc = LevelLoader.GetTestLevel();
            LevelLoader.LoadAndValidateLevel(LevelDesc);
            Waves = new WaveManager(LevelDesc);
            Turrets = new TurretManager(LevelDesc);
            Projectiles = new ProjectileManager();
            Loading = false;
        });
        t.Start();
    }

    void TickWaveCountdown()
    {
        Debug.Assert((GameTime - CountdownStartTime) > 0);

        if ((GameTime - CountdownStartTime) > WAVE_COUNTDOWN_TIME)
        {
            Waves.AdvanceToNextWave(GameTime);
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
            State = LevelState.WaveCountdown;
            CountdownStartTime = GameTime;
        }
    }
    
    void OnLevelComplete()
    {
        State = LevelState.StatsScreen;
    }

}
