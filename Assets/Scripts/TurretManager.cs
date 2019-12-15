using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



class TurretInstance
{
    Turret TurretType;
    public RoadPos Position;
    List<RoadPos> RoadSegmentsInRange;
    double LastShotTime;

    public TurretInstance(Turret turretType, RoadPos position, LevelDescription levelDesc)
    {
        TurretType = turretType;
        Position = position;
        RoadSegmentsInRange = new List<RoadPos>();
        LastShotTime = 0.0;
        CalculateRoadSegmentsInRangeByDistance(levelDesc);
    }

    double RoadPosDistance(RoadPos roadPos)
    {
        // Pythagorian theorem:  a^2 + b^2 = c^2
        double aSquared = Math.Pow(Math.Abs(Position.x - roadPos.x), 2);
        double bSquared = Math.Pow(Math.Abs(Position.y - roadPos.y), 2);
        return Math.Sqrt(aSquared + bSquared);
    }

    void CalculateRoadSegmentsInRangeByDistance(LevelDescription levelDesc)
    {
        List<double> distances = new List<double>();

        for (int i = 0; i < levelDesc.Road.Count; i++)
        {
            RoadPos rp = levelDesc.Road[i];

            // Calculate distance to road segment
            double distance = RoadPosDistance(rp);

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

    public void CheckForEnemiesAndFire(ProjectileManager projectileManager, double waveTime)
    {
        double timeSinceLastShot = (waveTime - LastShotTime);
        while (timeSinceLastShot >= TurretType.FireRate)
        {
            EnemyInstance nearestEnemyInRange = FindNearestEnemyInRange();
            if (null == nearestEnemyInRange)
            {
                break;
            }

            projectileManager.Fire();

            timeSinceLastShot = (waveTime - LastShotTime);
        }
    }
}


class TurretManager
{
    public ProjectileManager Projectiles = new ProjectileManager();
    LevelDescription LevelDesc;
    public List<TurretInstance> Turrets = new List<TurretInstance>();

    public TurretManager(LevelDescription levelDesc)
    {
        LevelDesc = levelDesc;
    }

    public void AddTurret(Turret turretType, RoadPos position)
    {
        Turrets.Add(new TurretInstance(turretType, position, LevelDesc));
    }

    public void Fire(double gameTime)
    {
        for (int i = 0; i < Turrets.Count; i++)
        {
            Turrets[i].CheckForEnemiesAndFire(Projectiles, gameTime);
        }
    }
}
