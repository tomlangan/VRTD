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
    public MapPos MapPosition = null;
    public List<EffectInstance> ActiveEffects;

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

    public void UpdateMapPosition(LevelDescription levelDesc)
    {
        if (ReachedFinishLine)
        {
            MapPosition.EnemiesOccupying.Remove(this);
            MapPosition = null;
        }
        else
        {
            int posIndex = (int)Pos;
            MapPos newMapPos = null;

            Debug.Assert(posIndex < levelDesc.Road.Count);

            newMapPos = levelDesc.Road[posIndex];

            if (MapPosition != newMapPos)
            {
                if (null != MapPosition)
                {
                    MapPosition.EnemiesOccupying.Remove(this);
                }

                newMapPos.EnemiesOccupying.Add(this);
                MapPosition = newMapPos;
            }
        }
    }
}

public class WaveInstance
{
    LevelDescription LevelDesc;
    List<EnemyInstance> Enemies;
    EnemyWave Desc;
    public int EnemiesThatSurvived;
    double WaveStartTime;
    int SpawnedCount;
    double LastSpawnTime;
    int RoadSegments;
    double speedWithEffectApplied;
    public bool IsCompleted;

    public WaveInstance(LevelDescription levelDesc, EnemyWave waveDescription, int roadSegments, double gameTime)
    {
        LevelDesc = levelDesc;
        Desc = waveDescription;
        Enemies = new List<EnemyInstance>();
        EnemiesThatSurvived = 0;
        RoadSegments = roadSegments;
        WaveStartTime = gameTime;
        LastSpawnTime = 0.0;
    }

    public void Advance(double waveTime)
    {
        Debug.Assert(!IsCompleted);


        SpawnNewEnemies(waveTime);

        ApplyProjectileEffects(waveTime);

        bool enemiesAreActive = AdvanceEnemyPositionsAndReportIfAnyActive(waveTime);


        // Check whether wave is complete
        if ((SpawnedCount == Desc.Count) &&
            !enemiesAreActive)
        {
            IsCompleted = true;
        }
    }

    void SpawnNewEnemies(double waveTime)
    {
        // Have we spawned all the enemies yet?
        // If not, are we due to spawn one or more enemies?
        while (SpawnedCount < Desc.Count)
        {
            double nextSpawnTime = LastSpawnTime + Desc.EnemyType.SpawnRate;
            if (waveTime > nextSpawnTime)
            {
                EnemyInstance newEnemy = new EnemyInstance(Desc.EnemyType, nextSpawnTime);
                newEnemy.UpdateMapPosition(LevelDesc);
                LastSpawnTime = nextSpawnTime;
                SpawnedCount++;
                Debug.Log("     Spawned Enemy " + SpawnedCount);
            }
            else
            {
                break;
            }
        }
    }
    void ApplyProjectileEffects(double waveTime)
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            EnemyInstance enemy = Enemies[i];

            if (!enemy.IsActive)
            {
                continue;
            }

            double damage = 0.0;
            double slowdown = 0.0;

            for (int j = 0; j < enemy.ActiveEffects.Count; j++)
            {
                EffectInstance effect = enemy.ActiveEffects[j];

                double impact = effect.AdvanceAndReportImpact(waveTime);

                switch (effect.Effect.EffectType)
                {
                    case ProjectileEffectType.Damage:
                        damage += impact;
                        break;
                    case ProjectileEffectType.Slow:
                        slowdown += impact;
                        break;
                }
            }

            if (enemy.HealthRemaining <= damage)
            {
                // Enemy is dead - what else do we need to do?
                enemy.HealthRemaining = 0.0;

                enemy.ActiveEffects.Clear();
            }
            else
            {
                enemy.HealthRemaining -= damage;
            }

            if (slowdown >= Desc.EnemyType.MovementSpeed)
            {
                speedWithEffectApplied = 0.0;
            }
            else
            {
                speedWithEffectApplied = Desc.EnemyType.MovementSpeed - slowdown;
            }
        }
    }

    bool AdvanceEnemyPositionsAndReportIfAnyActive(double waveTime)
    {
        bool isActive = false;
        for (int i = 0; i < Enemies.Count; i++)
        {
            EnemyInstance enemy = Enemies[i];
            if (enemy.IsActive)
            {
                double timeSinceSpawn = waveTime - enemy.SpawnTime;
                double newPosition = timeSinceSpawn * speedWithEffectApplied;
                if (newPosition > (double)RoadSegments)
                {
                    enemy.Pos = (double)RoadSegments;
                    enemy.ReachedFinishLine = true;
                }
                else
                {
                    enemy.Pos = newPosition;
                }
                enemy.UpdateMapPosition(LevelDesc);
                isActive = true;
            }
        }

        return isActive;
    }
}

public class WaveManager
{
    LevelDescription LevelDesc;
    public int WavesStarted;
    public WaveInstance CurrentWave;
    public double StartTime;
    int RoadSegments;
    public bool IsComplete
    {
        get
        {
            return ((WavesStarted == LevelDesc.Waves.Count) && CurrentWave.IsCompleted);
        }
    }

    public WaveManager(LevelDescription level)
    {
        LevelDesc = level;
        WavesStarted = 0;
        RoadSegments = level.Road.Count;
        CurrentWave = null;
    }


    public void AdvanceToNextWave(double gameTime)
    {
        Debug.Assert(WavesStarted < LevelDesc.Waves.Count);

        CurrentWave = new WaveInstance(LevelDesc, LevelDesc.Waves[WavesStarted], RoadSegments, gameTime);

        WavesStarted++;
        Debug.Log("  Wave " + WavesStarted);
    }
}