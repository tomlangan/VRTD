using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



class TurretInstance
{
    ProjectileManager Projectiles;
    public Turret TurretType;
    public MapPos Position;
    List<MapPos> RoadSegmentsInRange;
    float LastShotTime;

    public TurretInstance(Turret turretType, MapPos position, LevelDescription levelDesc, ProjectileManager projectiles)
    {
        Projectiles = projectiles;
        TurretType = turretType;
        Position = position;
        RoadSegmentsInRange = new List<MapPos>();
        LastShotTime = 0.0F;
        CalculateRoadSegmentsInRangeByDistance(levelDesc);
    }


    void CalculateRoadSegmentsInRangeByDistance(LevelDescription levelDesc)
    {
        List<float> distances = new List<float>();

        for (int i = 0; i < levelDesc.Road.Count; i++)
        {
            MapPos rp = levelDesc.Road[i];

            // Calculate distance to road segment
            float distance = Position.DistanceTo(rp);

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

    EnemyInstance FindNearestEnemyInRange()
    {
        EnemyInstance nearestEnemyFound = null;
        for (int i = 0; i < RoadSegmentsInRange.Count; i++)
        {
            if (RoadSegmentsInRange[i].EnemiesOccupying.Count > 0)
            {
                nearestEnemyFound = RoadSegmentsInRange[i].EnemiesOccupying[0];
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
            Projectiles.Fire(this, nearestEnemyInRange, LastShotTime);
            timeSinceLastShot = (waveTime - LastShotTime);
        }
    }
}


class TurretManager
{
    LevelDescription LevelDesc;
    public List<TurretInstance> Turrets = new List<TurretInstance>();

    public TurretManager(LevelDescription levelDesc)
    {
        LevelDesc = levelDesc;
    }

    public void AddTurret(Turret turretType, MapPos position, ProjectileManager projectiles)
    {
        Turrets.Add(new TurretInstance(turretType, position, LevelDesc, projectiles));
    }

    public void Fire(float gameTime)
    {
        for (int i = 0; i < Turrets.Count; i++)
        {
            Turrets[i].CheckForEnemiesAndFire(gameTime);
        }
    }
}
