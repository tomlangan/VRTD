using System.Collections;
using System.Collections.Generic;
using System;
#if LEVEL_EDITOR != true
using UnityEngine;
#else
using System.Numerics;
#endif


public class EffectInstance
{
    public ProjectileEffect Effect;
    public EnemyInstance Enemy;
    public float StartTime;
    public float LastTime;
    public bool Completed;
    GameObject go;

    public EffectInstance(EnemyInstance enemy, ProjectileEffect effect, float waveStartTime)
    {
        Enemy = enemy;
        Effect = effect;
        StartTime = waveStartTime;
        LastTime = waveStartTime;
        Completed = false;
    }

    public float AdvanceAndReportImpact(float waveTime)
    {
        Debug.Assert(!Completed);

        float impactPercent = 0.0F;

        if (Effect.EffectDuration == 0.0F)
        {
            // Entire impact at once
            impactPercent = 1.0F;
        }
        else
        {
            float elapsedTimeThisTick = waveTime - LastTime;
            float totalTimeElapsed = waveTime - StartTime;

            // If we went over the total time this tick, correct for the overage
            if (totalTimeElapsed > Effect.EffectDuration)
            {
                elapsedTimeThisTick -= (totalTimeElapsed - Effect.EffectDuration);
            }

            impactPercent = elapsedTimeThisTick / Effect.EffectDuration;
        }

        if ((waveTime - StartTime) > Effect.EffectDuration)
        {
            Completed = true;
        }
        LastTime = waveTime;

        return (impactPercent * Effect.EffectImpact);
    }

    public void Destroy()
    {
        if (null != go)
        {
            GameObjectFactory.Destroy(go);
            go = null;
        }
    }

}

class ProjectileInstance
{
    const float PROJECTILE_HEIGHT = 0.5F;
    public Projectile ProjectileType;
    public Vector3 Position;
    public float LastUpdateTime;
    public float FireTime;
    public EnemyInstance Enemy;
    public bool IsComplete;
    GameObject go = null;

    public ProjectileInstance(Projectile projectileType, Vector3 sourcePos, EnemyInstance enemy, float waveTimeFired)
    {
        ProjectileType = projectileType;
        LastUpdateTime = waveTimeFired;
        FireTime = waveTimeFired;
#if LEVEL_EDITOR
        Position = new Vector3(sourcePos.X, PROJECTILE_HEIGHT, sourcePos.X);
#else
        Position = new Vector3(sourcePos.x, PROJECTILE_HEIGHT, sourcePos.z);
#endif
        Enemy = enemy;
        IsComplete = false;
        go = GameObjectFactory.InstantiateObject(ProjectileType.Asset);
        GameObjectFactory.SetPos(go, Position);
    }

    public void Advance(float waveTime)
    {
        if (!Enemy.IsActive)
        {
            IsComplete = true;
            Destroy();
            return;
        }

        float hypotenuse = Vector3.Distance(Position,Enemy.Position);
        float distanceMovedThisFrame = (float)(waveTime - LastUpdateTime) * ProjectileType.AirSpeed;
        LastUpdateTime = waveTime;

        // If we hit the target, apply the effects (damage, slow, etc)
        if (distanceMovedThisFrame >= hypotenuse)
        {
            IsComplete = true;
            for (int j = 0; j < ProjectileType.Effects.Count; j++)
            {
                EffectInstance projectileEffect = new EffectInstance(Enemy, ProjectileType.Effects[j], waveTime);
                Enemy.ActiveEffects.Add(projectileEffect);
            }
            Destroy();
            return;
        }

        Vector3 direction = Enemy.Position - Position;
        Vector3 progress = direction * (distanceMovedThisFrame / hypotenuse);
        Position += progress;
        GameObjectFactory.SetPos(go, Position);

        /*

        // Move the projectile towards the target -- it's always heat-seeking
        float opposite = Enemy.Position.z - Position.z;
        float adjacent = Enemy.Position.x - Position.x;
        float angle = Mathf.Atan(Mathf.Abs(opposite / adjacent));

        // calculate distance to new position
        float xdelta = distanceMovedThisFrame * Mathf.Cos(angle) * Mathf.Sign(adjacent);
        float zdelta = distanceMovedThisFrame * Mathf.Sin(angle) * Mathf.Sign(opposite);

        Position.x += xdelta;
        Position.z += zdelta;

        GameObjectFactory.SetPos(go, Position);

        // float check that applying this again will result in reaching the enemy
#if DEBUG
        float distanceToGo = hypotenuse - distanceMovedThisFrame;
        float xdeltaToGoActual = Enemy.Position.x - Position.x;
        float zdeltaToGoActual = Enemy.Position.z - Position.z;
        float xdeltaToGoCalculated = distanceToGo * Mathf.Cos(angle);
        float zdeltaToGoCalculated = distanceToGo * Mathf.Sin(angle);
        if (0.2F < (xdeltaToGoActual - xdeltaToGoCalculated))
        { Debug.Break();  }
        if (0.2F < (zdeltaToGoActual - zdeltaToGoCalculated))
        { Debug.Break(); }
#endif
    */
    }

    public void Destroy()
    {
        if (null != go)
        {
            GameObjectFactory.Destroy(go);
            go = null;
        }
    }
}

class ProjectileManager
{
    public List<ProjectileInstance> ProjectilesInFlight = null;
    public ProjectileManager()
    {
        ProjectilesInFlight = new List<ProjectileInstance>();
    }

    public void Fire(TurretInstance turret, EnemyInstance target, float fireTime)
    {
        ProjectileInstance projectile = new ProjectileInstance(turret.TurretType.ProjectileType, new Vector3(turret.Position.x, 0.0F, turret.Position.z), target, fireTime);
        ProjectilesInFlight.Add(projectile);
    }

    public void AdvanceAll(float waveTime)
    {
        for (int i = ProjectilesInFlight.Count; i > 0; i--)
        {
            ProjectilesInFlight[i-1].Advance(waveTime);

            if (ProjectilesInFlight[i-1].IsComplete)
            {
                ProjectilesInFlight.Remove(ProjectilesInFlight[i-1]);
            }
        }
    }

    public void DestroyAll()
    {
        for (int i = ProjectilesInFlight.Count; i > 0; i--)
        {
            ProjectileInstance p = ProjectilesInFlight[i - 1];
            p.Destroy();
            ProjectilesInFlight.Remove(p);
        }
    }
}