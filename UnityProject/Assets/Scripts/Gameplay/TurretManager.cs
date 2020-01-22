using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if LEVEL_EDITOR
using System.Numerics;
#else
using UnityEngine;
#endif


namespace VRTD.Gameplay
{
    public class TurretInstance
    {
        ProjectileManager Projectiles;
        public Turret TurretType;
        public MapPos Position;
        List<MapPos> RoadSegmentsInRange;
        float LastShotTime;
        public GameObject go;

        public TurretInstance(Turret turretType, MapPos position, LevelDescription levelDesc, ProjectileManager projectiles)
        {
            Projectiles = projectiles;
            TurretType = turretType;
            Position = position;
            RoadSegmentsInRange = new List<MapPos>();
            LastShotTime = 0.0F;
            CalculateRoadSegmentsInRangeByDistance(levelDesc);
            go = GameObjectFactory.InstantiateObject(turretType.Asset);
#if LEVEL_EDITOR == false
            go.tag = "TurretInstance";
#endif
            GameObjectFactory.SetMapPos(go, Position);
        }


        void CalculateRoadSegmentsInRangeByDistance(LevelDescription levelDesc)
        {
            List<float> distances = new List<float>();

            for (int i = 0; i < levelDesc.Road.Count; i++)
            {
                MapPos rp = levelDesc.Road[i];

                // Calculate distance to road segment
                float distance = Vector3.Distance(Position.Pos, rp.Pos);

                if (distance <= TurretType.Range)
                {
                    int index = 0;

                    // Find the index where this distance is higher
                    // than the previous, lower than the next
                    for (int j = 0; j < distances.Count; j++)
                    {
                        if (distance < distances[j])
                        {
                            break;
                        }
                        index++;
                    }

                    distances.Insert(index, distance);
                    RoadSegmentsInRange.Insert(index, rp);
                }
            }
        }

        public EnemyInstance FindNearestEnemyInRange()
        {
            EnemyInstance nearestEnemyFound = null;
            for (int i = 0; i < RoadSegmentsInRange.Count; i++)
            {
                for (int j = 0; j < RoadSegmentsInRange[i].EnemiesOccupying.Count; j++)
                {
                    if (RoadSegmentsInRange[i].EnemiesOccupying[j].IsActive)
                    {
                        nearestEnemyFound = RoadSegmentsInRange[i].EnemiesOccupying[j];
                        break;
                    }
                }
                if(
                    null != nearestEnemyFound)
                {
                    break;
                }
            }
            return nearestEnemyFound;
        }

        public void CheckForEnemiesAndFire(float waveTime)
        {
            float timeSinceLastShot = (waveTime - LastShotTime);
            while (timeSinceLastShot >= TurretType.FireRate)
            {
                EnemyInstance nearestEnemyInRange = FindNearestEnemyInRange();
                if (null == nearestEnemyInRange)
                {
                    break;
                }

                LastShotTime = LastShotTime + TurretType.FireRate;

                Debug.Log("TURRET FIRE: " + this.TurretType.Name);
                Projectiles.Fire(this, nearestEnemyInRange, LastShotTime);

                timeSinceLastShot = (waveTime - LastShotTime);
            }
        }

        public void Destroy()
        {
            GameObjectFactory.Destroy(go);
            go = null;
        }
    }


    public class TurretManager
    {
        LevelDescription LevelDesc;
        public List<TurretInstance> Turrets = new List<TurretInstance>();

        public TurretManager(LevelDescription levelDesc)
        {
            LevelDesc = levelDesc;
        }



        public void AddTurret(Turret turret, MapPos position, ProjectileManager projectiles)
        {
            Turrets.Add(new TurretInstance(turret, position, LevelDesc, projectiles));
        }

        public void RemoveTurret(TurretInstance turret)
        {
            turret.Destroy();
            Turrets.Remove(turret);
        }

        public void Fire(float gameTime)
        {
            for (int i = 0; i < Turrets.Count; i++)
            {
                Turrets[i].CheckForEnemiesAndFire(gameTime);
            }
        }

        public TurretInstance GetTurretAtPosition(MapPos pos)
        {
            TurretInstance found = null;
            for (int j = 0; j < Turrets.Count; j++)
            {
                if ((Turrets[j].Position.x == pos.x) &&
                    (Turrets[j].Position.z == pos.z))
                {
                    found = Turrets[j];
                    break;
                }
            }
            return found;
        }

        public void DestroyAll()
        {
            for (int i = Turrets.Count; i > 0; i--)
            {
                TurretInstance t = Turrets[i - 1];
                t.Destroy();
                Turrets.Remove(t);
            }
        }
    }

}