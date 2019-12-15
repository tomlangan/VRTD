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
    public RoadPos RoadPosition = null;
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

    public void UpdateRoadPosition(LevelDescription levelDesc)
    {
        if (ReachedFinishLine)
        {
            RoadPosition.EnemiesOccupying.Remove(this);
            RoadPosition = null;
        }
        else
        {
            int posIndex = (int)Pos;
            RoadPos newRoadPos = null;

            Debug.Assert(posIndex < levelDesc.Road.Count);

            newRoadPos = levelDesc.Road[posIndex];

            if (RoadPosition != newRoadPos)
            {
                if (null != RoadPosition)
                {
                    RoadPosition.EnemiesOccupying.Remove(this);
                }

                newRoadPos.EnemiesOccupying.Add(this);
                RoadPosition = newRoadPos;
            }
        }
    }
}

public class WaveInstance
{
    public LevelDescription LevelDesc;
    public List<EnemyInstance> Enemies;
    public EnemyWave Desc;
    public int EnemiesThatSurvived;
    public double WaveStartTime;
    public int SpawnedCount;
    public double LastSpawnTime;
    public int RoadSegments;
    double speedWithEffectApplied;

    public WaveInstance(LevelDescription levelDesc, EnemyWave waveDescription, int roadSegments, double gameTime)
    {
        LevelDesc = levelDesc;
        Desc = waveDescription;
        Enemies = new List<EnemyInstance>();
        EnemiesThatSurvived = 0;
        RoadSegments = roadSegments;
        WaveStartTime = gameTime;
    }

    public void Advance(double waveTime)
    {
        SpawnNewEnemies(waveTime);

        ApplyProjectileEffects(waveTime);

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
                newEnemy.UpdateRoadPosition(LevelDesc);
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

    void AdvanceEnemyPositions(double waveTime)
        {
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
                enemy.UpdateRoadPosition(LevelDesc);
            }
        }
    }
}

public class WaveManager
{
    LevelDescription LevelDesc;
    public int WavesStarted;
    private WaveInstance _currentWave;
    public double StartTime;
    int RoadSegments;

    public WaveInstance CurrentWave
    {
        get
        {
            Debug.Assert(null != CurrentWave);
            return CurrentWave;
        }
    }

    public WaveManager(LevelDescription level)
    {
        LevelDesc = level;
        WavesStarted = 0;
        RoadSegments = level.Road.Count;
    }


    public void AdvanceToNextWave(double gameTime)
    {
        Debug.Assert(WavesStarted < LevelDesc.Waves.Count);

        _currentWave = new WaveInstance(LevelDesc, LevelDesc.Waves[WavesStarted], RoadSegments, gameTime);

        WavesStarted++;
    }
}