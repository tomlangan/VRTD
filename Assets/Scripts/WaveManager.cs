using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInstance
{
    public EnemyDescription Desc;
    public double SpawnTime;
    public double Pos;
    public double HealthRemaining;
    public bool ReachedFinishLine;

    public bool IsActive
    {
        get
        {
            return (!ReachedFinishLine && (HealthRemaining > 0.0));
        }
    }

    public EnemyInstance(EnemyDescription desc, double spawnTime)
    {
        Desc = desc;
        SpawnTime = spawnTime;
        HealthRemaining = desc.HitPoints;
        Pos = 0.0;
        ReachedFinishLine = false;
    }
}

public class WaveInstance
{
    public List<EnemyInstance> Enemies;
    public EnemyWave Desc;
    public int EnemiesThatSurvived;
    public double WaveStartTime;
    public int SpawnedCount;
    public double LastSpawnTime;
    public int RoadSegments;

    public WaveInstance(EnemyWave waveDescription, int roadSegments)
    {
        Desc = waveDescription;
        Enemies = new List<EnemyInstance>();
        EnemiesThatSurvived = 0;
        RoadSegments = roadSegments;
    }

    public void Advance(double waveTime)
    {
        SpawnNewEnemies(waveTime);

        AdvanceEnemyPositions(waveTime);
    }

    void SpawnNewEnemies(double waveTime)
    {
        // Have we spawned all the enemies yet?
        // If not, are we due to spawn one or more enemies?
        while (SpawnedCount < Desc.Count)
        {
            EnemyInstance lastEnemy = Enemies[Enemies.Count - 1];
            double nextSpawnTime = lastEnemy.SpawnTime + Desc.EnemyType.SpawnRate;
            if (waveTime > nextSpawnTime)
            {
                EnemyInstance newEnemy = new EnemyInstance(Desc.EnemyType, nextSpawnTime);
            }
            else
            {
                break;
            }
        }
    }

    void AdvanceEnemyPositions(double waveTime)
    {
        for (int j = 0; j < Enemies.Count; j++)
        {
            EnemyInstance enemy = Enemies[j];
            if (enemy.IsActive)
            {
                double timeSinceSpawn = waveTime - enemy.SpawnTime;
                double newPosition = timeSinceSpawn * Desc.EnemyType.MovementSpeed;
                if (newPosition > (double)RoadSegments)
                {
                    enemy.Pos = (double)RoadSegments;
                    enemy.ReachedFinishLine = true;
                }
                else
                {
                    enemy.Pos = newPosition;
                }
            }
        }
    }
}

public class WaveManager
{
    public List<EnemyWave> Waves;
    public int WavesStarted;
    public WaveInstance CurrentWave;
    public double StartTime;
    int RoadSegments;

    public WaveManager(LevelDescription level, double startTime)
    {
        WavesStarted = 0;
        StartTime = startTime;
        Waves = level.Waves;
        RoadSegments = level.Road.Count;
        AdvanceToNextWave();
    }

    public WaveInstance GetCurrentWave()
    {
        if (null == CurrentWave)
        {
            AdvanceToNextWave();
        }

        return CurrentWave;
    }

    public void AdvanceToNextWave()
    {
        Debug.Assert(WavesStarted < Waves.Count);

        CurrentWave = new WaveInstance(Waves[WavesStarted], RoadSegments);

        WavesStarted++;
    }
}