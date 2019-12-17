using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


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

        if (Effect.EffectDuration == 0.0)
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
        LastUpdateTime = 0.0F;
        FireTime = waveTimeFired;
        Position = new Vector3(sourcePos.x, sourcePos.y, sourcePos.z);
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
        }

        float hypotenuse = Vector3.Distance(Position,Enemy.Position);
        float distanceMoved = (float)(waveTime - LastUpdateTime) * ProjectileType.AirSpeed;
        LastUpdateTime = waveTime;

        // If we hit the target, apply the effects (damage, slow, etc)
        if (distanceMoved >= hypotenuse)
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

        // Move the projectile towards the target -- it's always heat-seeking
        float opposite = Enemy.Position.z - Position.z;
        float adjacent = Enemy.Position.x - Position.x;
        float angle = (float)Math.Atan(opposite / adjacent);

        // calculate distance to new position
        float xdelta = distanceMoved * (float)Math.Sin(angle);
        float zdelta = distanceMoved * (float)Math.Cos(angle);

        Position.x += xdelta;
        Position.z += zdelta;

        GameObjectFactory.SetPos(go, Position);

        // float check that applying this again will result in reaching the enemy
#if DEBUG
        float distanceToGo = hypotenuse - distanceMoved;
        Debug.Assert(Enemy.Position.z == (int)(Position.z + (distanceToGo * Math.Sin(angle))));
        Debug.Assert(Enemy.Position.x == (int)(Position.x + (distanceToGo * Math.Cos(angle))));
#endif
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
        for (int i = 0; i < ProjectilesInFlight.Count; i++)
        {
            ProjectilesInFlight[i].Advance(waveTime);

            if (ProjectilesInFlight[i].IsComplete)
            {
                ProjectilesInFlight.Remove(ProjectilesInFlight[i]);
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