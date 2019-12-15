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
    WaveInstance CurrentWave = null;
    public double GameTime;
    bool Loading = false;
    public LevelState State = LevelState.None;

    // Start is called before the first frame update
    void Start()
    {
        State = LevelState.Loading;
        LoadLevel();
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case LevelState.Loading:
                if (!Loading)
                {
                    State = LevelState.WaveCountdown;
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
            Waves = new WaveManager(LevelDesc, Time.deltaTime);
            Loading = false;
        });
        t.Start();
    }

    void TickWaveCountdown()
    {
        double elapsed = Time.deltaTime;
        GameTime += elapsed;

        if (GameTime > WAVE_COUNTDOWN_TIME)
        {
            GameTime = 0.0;
            Waves.AdvanceToNextWave();
            CurrentWave = Waves.GetCurrentWave();
            State = LevelState.Playing;
        }
    }

    void TickGameplay()
    {
        double elapsed = Time.deltaTime;
        GameTime += elapsed;

        Debug.Assert(null != CurrentWave);
        CurrentWave.Advance(GameTime);

        CheckEndConditions();
    }
    
    void CheckEndConditions()
    {

    }


}
