using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class EffectInstance
{
    public ProjectileEffect Effect;
    public EnemyInstance Enemy;
    public double StartTime;
    public double LastTime;
    public bool Completed;

    public EffectInstance(EnemyInstance enemy, ProjectileEffect effect, double waveStartTime)
    {
        Enemy = enemy;
        Effect = effect;
        StartTime = waveStartTime;
        LastTime = waveStartTime;
        Completed = false;
    }

    public double AdvanceAndReportImpact(double waveTime)
    {
        Debug.Assert(!Completed);

        double impactPercent = 0.0;

        if (Effect.EffectDuration == 0.0)
        {
            // Entire impact at once
            impactPercent = 1.0;
        }
        else
        {
            double elapsedTimeThisTick = waveTime - LastTime;
            double totalTimeElapsed = waveTime - StartTime;

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
}

class ProjectileInstance
{
    public Projectile ProjectileType;
    public RealPos Position;
    public double LastUpdateTime;
    public double FireTime;
    public EnemyInstance Enemy;
    public bool IsComplete;


    public ProjectileInstance(Projectile projectileType, RealPos sourcePos, EnemyInstance enemy, double waveTimeFired)
    {
        ProjectileType = projectileType;
        LastUpdateTime = 0.0;
        FireTime = waveTimeFired;
        Position = new RealPos(sourcePos);
        Enemy = enemy;
        IsComplete = false;
    }

    public void Advance(double waveTime)
    {
        if (!Enemy.IsActive)
        {
            IsComplete = true;
        }

        double hypotenuse = Position.DistanceTo(Enemy.MapPosition);
        double distanceMoved = (waveTime - LastUpdateTime) * ProjectileType.AirSpeed;
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
        }

        // Move the projectile towards the target -- it's always heat-seeking
        double opposite = Enemy.MapPosition.y - Position.y;
        double adjacent = Enemy.MapPosition.x - Position.x;
        double angle = Math.Atan(opposite / adjacent);

        // calculate distance to new position
        double xdelta = distanceMoved * Math.Sin(angle);
        double ydelta = distanceMoved * Math.Cos(angle);

        Position.x += xdelta;
        Position.y += ydelta;

        // Double check that applying this again will result in reaching the enemy
#if DEBUG
        double distanceToGo = hypotenuse - distanceMoved;
        Debug.Assert(Enemy.MapPosition.y == (int)(Position.y + (distanceToGo * Math.Sin(angle))));
        Debug.Assert(Enemy.MapPosition.x == (int)(Position.x + (distanceToGo * Math.Cos(angle))));
#endif
    }
}

class ProjectileManager
{
    public List<ProjectileInstance> ProjectilesInFlight = null;
    public ProjectileManager()
    {
        ProjectilesInFlight = new List<ProjectileInstance>();
    }

    public void Fire(TurretInstance turret, EnemyInstance target, double fireTime)
    {
        ProjectileInstance projectile = new ProjectileInstance(turret.TurretType.ProjectileType, new RealPos(turret.Position.x, turret.Position.y), target, fireTime);
        ProjectilesInFlight.Add(projectile);
    }

    public void AdvanceAll(double waveTime)
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
}