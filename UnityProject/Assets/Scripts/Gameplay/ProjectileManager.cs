using System.Collections;
using System.Collections.Generic;
using System;
#if LEVEL_EDITOR != true
using UnityEngine;
#else
using System.Numerics;
#endif


namespace VRTD.Gameplay
{
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

    public class ProjectileInstance
    {
        const float PROJECTILE_HEIGHT = 0.5F;
        public Projectile ProjectileType;
        public Vector3 Position;
        public float LastUpdateTime;
        public float FireTime;
        public EnemyInstance Enemy;
        public bool IsComplete;
        GameObject go = null;

        public ProjectileInstance(Projectile projectile, Vector3 sourcePos, EnemyInstance enemy, float waveTimeFired)
        {
            ProjectileType = projectile;
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
            GameObjectFactory.SetMapPos(go, Position);
        }

        public void Advance(float waveTime)
        {
            if (!Enemy.IsActive)
            {
                Debug.Log("  enemy no longer active (projectile dead)");
                IsComplete = true;
                Destroy();
                return;
            }

            float hypotenuse = Vector3.Distance(Position, Enemy.Position);
            float distanceMovedThisFrame = (float)(waveTime - LastUpdateTime) * ProjectileType.AirSpeed;
            LastUpdateTime = waveTime;

            // If we hit the target, apply the effects (damage, slow, etc)
            if (distanceMovedThisFrame >= hypotenuse)
            {
                IsComplete = true;
                Debug.Log("PROJECTILE HIT: " + this.ProjectileType.Name);
                for (int j = 0; j < ProjectileType.Effects.Count; j++)
                {
                    EffectInstance projectileEffect = new EffectInstance(Enemy, ProjectileType.Effects[j], waveTime);
                    Enemy.ActiveEffects.Add(projectileEffect);
                }
                Destroy();
                return;
            }

#if LEVEL_EDITOR
            Vector3 direction = Enemy.Position - Position;
            direction = VectorHelpers.Normalize(direction);
#else
            Vector3 direction = (Enemy.Position - Position).normalized;
#endif
            Vector3 progress = direction * distanceMovedThisFrame;
            Position += progress;
            GameObjectFactory.SetMapPos(go, Position);

            Debug.Log("   projectile distance: " + (distanceMovedThisFrame - hypotenuse).ToString() + " --> " + Vector3.Distance(Position, Enemy.Position).ToString());

        }

        public void Destroy()
        {
            if (null != go)
            {
                try
                {
                    GameObjectFactory.Destroy(go);
                }catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                go = null;
            }
        }
    }

    public class ProjectileManager
    {
        public delegate Projectile ProjectileDefCallback(string projectileName);
        public ProjectileDefCallback ProjectileReader;

        public List<ProjectileInstance> ProjectilesInFlight = null;

        public ProjectileManager(ProjectileDefCallback readerCallback = null)
        {
            ProjectilesInFlight = new List<ProjectileInstance>();

            if (null == readerCallback)
            {
                ProjectileReader = LevelLoader.LookupProjectile;
            }
            else
            {
                ProjectileReader = readerCallback;
            }
        }


        public void Fire(TurretInstance turret, EnemyInstance target, float fireTime)
        {
            ProjectileInstance projectile = new ProjectileInstance(ProjectileReader(turret.TurretType.Projectile), new Vector3(turret.Position.x, 0.0F, turret.Position.z), target, fireTime);
            Debug.Assert(target.HealthRemaining > 0.0F);
            Debug.Log("  fired " + projectile.ProjectileType.Name);
            ProjectilesInFlight.Add(projectile);
        }

        public void AdvanceAll(float waveTime)
        {
            for (int i = ProjectilesInFlight.Count; i > 0; i--)
            {
                ProjectilesInFlight[i - 1].Advance(waveTime);

                if (ProjectilesInFlight[i - 1].IsComplete)
                {
                    ProjectilesInFlight.Remove(ProjectilesInFlight[i - 1]);
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
}