using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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

public class EffectManager
{

    public static void ReportImpact(EnemyInstance enemy, ProjectileEffect effect, double waveImpactTime)
    {
        EffectInstance ef = new EffectInstance(enemy, effect, waveImpactTime);
        enemy.ActiveEffects.Add(ef);
    }
}


class ProjectilePosition
{
    public double x;
    public double y;
}

class ProjectileInstance
{
    public Projectile ProjectileType;
    public ProjectilePosition ProjectilePos;
    public double AirTime;
    public double FireTime;


    public ProjectileInstance(Projectile projectileType, TurretPos firingTurret, double waveTimeFired)
    {
        ProjectileType = projectileType;
        AirTime = 0.0;
        FireTime = waveTimeFired;
        ProjectilePos = new ProjectilePosition();
        ProjectilePos.x = firingTurret.x;
        ProjectilePos.y = firingTurret.y;
    }
}

class ProjectileManager
{

    public ProjectileManager()
    {

    }

    public void Fire()
    {

    }
}