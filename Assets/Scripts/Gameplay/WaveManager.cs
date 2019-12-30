using System.Collections;
using System.Collections.Generic;
using System;
#if LEVEL_EDITOR
using System.Numerics;
#else
using UnityEngine;
#endif

public class EnemyInstance 
{
    public EnemyDescription Desc;
    public float LastMoveTime;
    public float Progress;
    public float HealthRemaining;
    public bool ReachedFinishLine;
    public float LinearProgress;
    public float SpeedSlowdown;
    public Vector3 Position;
    public List<EffectInstance> ActiveEffects;
    MapPos MapPosition;
    GameObject go;

    public bool IsActive
    {
        get
        {
            return (!ReachedFinishLine && (HealthRemaining > 0.0));
        }
    }

    public EnemyInstance(EnemyDescription desc, float spawnTime)
    {
        

        Desc = desc;
        LastMoveTime = spawnTime;
        HealthRemaining = desc.HitPoints;
        LinearProgress = 0.0F;
        Position = new Vector3();
        ReachedFinishLine = false;
        ActiveEffects = new List<EffectInstance>();
        MapPosition = null;
        go = GameObjectFactory.InstantiateObject(Desc.Asset);
    }


    public void UpdatePosition(LevelDescription levelDesc)
    {
        

        if (ReachedFinishLine)
        {
            Destroy();
        }
        else
        {
            int posIndex = (int)LinearProgress;
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

            float intraMapProgress = (LinearProgress - (float)posIndex);
#if LEVEL_EDITOR
            float progressFromCenter = Math.Abs(0.5F - intraMapProgress);
#else
            float progressFromCenter = Mathf.Abs(0.5F - intraMapProgress);
#endif
            Vector3 direction;
            
            if ((intraMapProgress < 0.5F) && (posIndex > 0))
            {
                direction = levelDesc.Road[posIndex-1].Pos - levelDesc.Road[posIndex].Pos;
            }
            else if ((intraMapProgress < 0.5F) && (posIndex == 0))
            {
                // straight down
                direction = new Vector3(0.0F, 0.0F, -1.0F);
            }
            else if ((intraMapProgress >= 0.5F) && (posIndex == (levelDesc.Road.Count - 1)))
            {
                // straight down
                direction = new Vector3(0.0F, 0.0F, 1.0F);
            }
            else
            {
                direction = levelDesc.Road[posIndex + 1].Pos - levelDesc.Road[posIndex].Pos;
            }

            Vector3 movement = direction * progressFromCenter;
            //Position = BasePosition + movement;
            Position = levelDesc.Road[posIndex].Pos + movement;

            GameObjectFactory.SetPos(go, Position);
        }
    }

    public void Destroy()
    {
        for (int i = ActiveEffects.Count; i > 0; i--)
        {
            EffectInstance ei = ActiveEffects[i - 1];
            ei.Destroy();
            ActiveEffects.Remove(ei);
        }
        ActiveEffects.Clear();
        MapPosition.EnemiesOccupying.Remove(this);
        MapPosition = null;
        if (null != go)
        {
            GameObjectFactory.Destroy(go);
            go = null;
        }
    }
}

public class WaveInstance
{
    LevelDescription LevelDesc;
    public List<EnemyInstance> Enemies;
    EnemyWave Desc;
    public int EnemiesThatSurvived;
    float WaveStartTime;
    int SpawnedCount;
    float LastSpawnTime;
    int RoadSegments;
    public bool IsCompleted;

    public WaveInstance(LevelDescription levelDesc, EnemyWave waveDescription, int roadSegments, float gameTime)
    {
        LevelDesc = levelDesc;
        Desc = waveDescription;
        Enemies = new List<EnemyInstance>();
        EnemiesThatSurvived = 0;
        RoadSegments = roadSegments;
        WaveStartTime = gameTime;
        LastSpawnTime = WaveStartTime;
    }

    public void Advance(float waveTime)
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

    void SpawnNewEnemies(float waveTime)
    {
        // Have we spawned all the enemies yet?
        // If not, are we due to spawn one or more enemies?
        while (SpawnedCount < Desc.Count)
        {
            float nextSpawnTime = LastSpawnTime + Desc.EnemyType.SpawnRate;
            if (waveTime > nextSpawnTime)
            {
                EnemyInstance newEnemy = new EnemyInstance(Desc.EnemyType, nextSpawnTime);
                newEnemy.UpdatePosition(LevelDesc);
                Enemies.Add(newEnemy);
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
    void ApplyProjectileEffects(float waveTime)
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            EnemyInstance enemy = Enemies[i];

            if (!enemy.IsActive)
            {
                continue;
            }

            float damage = 0.0F;
            float slowdown = 0.0F;

            for (int j = enemy.ActiveEffects.Count; j > 0; j--)
            {
                EffectInstance effect = enemy.ActiveEffects[j-1];

                float impact = effect.AdvanceAndReportImpact(waveTime);

                switch (effect.Effect.EffectType)
                {
                    case ProjectileEffectType.Damage:
                        damage += impact;
                        break;
                    case ProjectileEffectType.Slow:
                        slowdown += impact;
                        break;
                }

                if (effect.Completed)
                {
                    effect.Destroy();
                    enemy.ActiveEffects.Remove(effect);
                    effect = null;
                }
            }

            if (enemy.HealthRemaining <= damage)
            {
                // Enemy is dead - what else do we need to do?
                enemy.HealthRemaining = 0.0F;
                enemy.Destroy();
            }
            else
            {
                enemy.HealthRemaining -= damage;
            }

            enemy.SpeedSlowdown = slowdown;
        }
    }

    bool AdvanceEnemyPositionsAndReportIfAnyActive(float waveTime)
    {
        bool isActive = false;
        for (int i = 0; i < Enemies.Count; i++)
        {
            EnemyInstance enemy = Enemies[i];
            if (enemy.IsActive)
            {
                float timeSinceLastTick = waveTime - enemy.LastMoveTime;
                float progressSinceLastTick = timeSinceLastTick * enemy.Desc.MovementSpeed;
                if (enemy.SpeedSlowdown > progressSinceLastTick)
                {
                    progressSinceLastTick = 0.0F;
                }
                else
                {
                    progressSinceLastTick -= enemy.SpeedSlowdown;
                }
                float newPosition = enemy.LinearProgress + progressSinceLastTick;
                if (newPosition > (float)RoadSegments)
                {
                    enemy.LinearProgress = (float)RoadSegments;
                    enemy.ReachedFinishLine = true;
                    EnemiesThatSurvived++;
                }
                else
                {
                    enemy.LinearProgress = newPosition;
                }
                enemy.UpdatePosition(LevelDesc);
                enemy.LastMoveTime = waveTime;
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
    public float StartTime;
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


    public void AdvanceToNextWave(float gameTime)
    {
        Debug.Assert(WavesStarted < LevelDesc.Waves.Count);

        CurrentWave = new WaveInstance(LevelDesc, LevelDesc.Waves[WavesStarted], RoadSegments, gameTime);

        WavesStarted++;
        Debug.Log("  Wave " + WavesStarted);
    }
}