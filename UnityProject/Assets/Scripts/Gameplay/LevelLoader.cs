﻿using System.Collections.Generic;
#if LEVEL_EDITOR
using System.Numerics;

public class Debug
{
    public static void Assert(bool b) { }
    public static void Log(string s) { }
}


public class GameObject { }

public class GameObjectFactory
{
    public static GameObject InstantiateObject(string asset) { return new GameObject(); }
    public static void SetPos(GameObject go, VRTD.Gameplay.MapPos Position) { }
    public static void SetPos(GameObject go, Vector3 Position) { }
    public static void Destroy(GameObject go) { }
}
#else
using UnityEngine;
#endif


namespace VRTD.Gameplay
{
    public class LevelLoader
    {

        enum WalkDir { Up, Down, Left, Right, None };

        public delegate bool OnFound(int x, int y);

        public static List<MapPos> WalkAndValidateRoad(List<char> map, int width, int height, MapPos entry, MapPos exit)
        {
            WalkDir previousDirection = WalkDir.None;
            WalkDir thisDirection = WalkDir.None;
            List<MapPos> r = new List<MapPos>();
            MapPos last = null;
            MapPos next = null;
            int found = 0;
            int max = (width * height) - 2;

            r.Add(entry);
            last = entry;


            // Loop until we've found the exit
            while (found < max)
            {
                //
                // First check for road
                //

                // check left if we're not on the edge
                if ((last.x > 0)
                    &&
                    // This isn't the previous item we just saw
                    (previousDirection != WalkDir.Right)
                    &&
                    // is the item to the left a road segment?
                    (('R' == map[(last.z * width) + (last.x - 1)])
                    ||
                    // is the item to the left the exit?
                    ((exit.x == (last.x - 1)) && (exit.z == last.z))))
                {
                    thisDirection = WalkDir.Left;
                    next = new MapPos(last.x - 1, last.z);
                }

                //check right if we're not at the bottom
                if ((last.x < (width - 1))
                    &&
                    // This isn't the previous item we just saw
                    (previousDirection != WalkDir.Left)
                    &&
                    (('R' == map[(int)((last.z) * width) + (last.x + 1)])
                    ||
                    ((exit.x == (last.x + 1)) && (exit.z == last.z))))
                {
                    Debug.Assert(null == next);
                    thisDirection = WalkDir.Right;
                    next = new MapPos(last.x + 1, last.z);
                }

                //check down if we're not at the bottom
                if ((last.z < (height - 1))
                    &&
                    // This isn't the previous item we just saw
                    (previousDirection != WalkDir.Up)
                    &&
                    (('R' == map[((last.z + 1) * width) + (last.x)])
                    ||
                    // is the item downwards the exit?
                    ((exit.x == (last.x)) && (exit.z == (last.z + 1)))))
                {
                    Debug.Assert(null == next);
                    thisDirection = WalkDir.Down;
                    next = new MapPos(last.x, last.z + 1);
                }

                //check up if we're not at the bottom
                if ((last.z > 0)
                    &&
                    // This isn't the previous item we just saw
                    (previousDirection != WalkDir.Down)
                    &&
                    (('R' == map[((last.z - 1) * width) + (last.x)])
                    ||
                    // is the item upwards the exit?
                    ((exit.x == (last.x)) && (exit.z == (last.z - 1)))))
                {
                    Debug.Assert(null == next);
                    thisDirection = WalkDir.Up;
                    next = new MapPos(last.x, last.z - 1);
                }


                // We didn't find a next road segment!
                Debug.Assert(null != next);

                r.Add(next);
                last = next;
                next = null;
                previousDirection = thisDirection;
                thisDirection = WalkDir.None;

                found++;

                // If we found the exit, break
                if ((exit.x == last.x) && (exit.z == last.z)) { break; }
            }

            // Something went wrong if we found this many items!
            Debug.Assert(found != max);
            // Something went wrong if we got here and never found the exit.
            Debug.Assert(((exit.x == last.x) && (exit.z == last.z)));

            return r;
        }

        public static void LoadAndValidateLevel(LevelDescription level)
        {
            level.Entry = null;
            level.Exit = null;
            level.Road = null;
            level.Turrets = new List<MapPos>();


            // Find the entry, ensure there are no dupes
            FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'E', (x, y) =>
                {
                    Debug.Assert(null == level.Entry);
                    level.Entry = new MapPos(x, y);
                    return true;
                });


            // Find the exit, ensure there are no dupes
            FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'X', (x, y) =>
            {
                Debug.Assert(null == level.Exit);
                level.Exit = new MapPos(x, y);
                return true;
            });

            // Find all the turrets
            FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'T', (x, y) =>
            {
                level.Turrets.Add(new MapPos(x, y));
                return true;
            });

            level.Road = WalkAndValidateRoad(level.Map, level.FieldWidth, level.FieldDepth, level.Entry, level.Exit);

        }

        public static void FindMapLocations(List<char> map, int width, int height, char c, OnFound callback)
        {

            // Scan the first line for the entry,
            // make sure there is only one
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (c == map[(y * width) + x])
                    {
                        if (!callback(x, y))
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void ReadFromFile(string levelFile)
        {

        }

        public static LevelDescription GetTestLevel()
        {
            LevelDescription level = new LevelDescription();

            level.Name = "0-0";

            level.Waves = new List<EnemyWave>();

            EnemyWave wave = new EnemyWave();
            wave.EnemyType = new EnemyDescription();
            wave.EnemyType.Name = "BasicEnemy";
            wave.EnemyType.MovementSpeed = 1.0F;
            wave.EnemyType.SpawnRate = 1.0F;
            wave.EnemyType.HitPoints = 2;
            wave.EnemyType.Asset = "BasicEnemy";
            wave.Count = 10;
            wave.DifficultyMultiplier = 1.0F;

            level.Waves.Add(wave);

            wave = new EnemyWave();
            wave.EnemyType = new EnemyDescription();
            wave.EnemyType.Name = "SwarmEnemy";
            wave.EnemyType.MovementSpeed = 5.0F;
            wave.EnemyType.SpawnRate = 5.0F;
            wave.EnemyType.HitPoints = 1;
            wave.EnemyType.Asset = "SwarmEnemy";
            wave.Count = 20;
            wave.DifficultyMultiplier = 1.0F;

            level.Waves.Add(wave);

            level.AllowedTurrets = new List<Turret>();

            Turret turret = new Turret();
            turret.Name = "Basic Turret";
            turret.Asset = "BasicTurret";
            turret.FireRate = 2.0F;
            turret.Range = 5;
            Projectile projectile = new Projectile();
            projectile.AirSpeed = 4.0F;
            projectile.Asset = "BasicBullet";
            projectile.Name = "Basic Bullet";
            ProjectileEffect basicTurretDamangeEffect = new ProjectileEffect();
            basicTurretDamangeEffect.EffectType = ProjectileEffectType.Damage;
            basicTurretDamangeEffect.EffectDuration = 0.0F;
            basicTurretDamangeEffect.EffectImpact = 1.0F;
            projectile.Effects.Add(basicTurretDamangeEffect);
            turret.ProjectileType = projectile;
            level.AllowedTurrets.Add(turret);

            turret = new Turret();
            turret.Name = "Ice Shot";
            turret.Asset = "IceTurret";
            turret.FireRate = 5.0F;
            turret.Range = 3;
            projectile = new Projectile();
            projectile.AirSpeed = 4.0F;
            projectile.Asset = "IceBullet";
            projectile.Name = "Ice Bullet";
            ProjectileEffect iceTurretEffect = new ProjectileEffect();
            iceTurretEffect.EffectType = ProjectileEffectType.Slow;
            iceTurretEffect.EffectDuration = 4.0F;
            iceTurretEffect.EffectImpact = 3.0F;
            projectile.Effects.Add(iceTurretEffect);
            turret.ProjectileType = projectile;
            level.AllowedTurrets.Add(turret);

            turret = new Turret();
            turret.Name = "Fire Shot";
            turret.Asset = "FireTurret";
            turret.FireRate = 2.0F;
            turret.Range = 6;
            projectile = new Projectile();
            projectile.AirSpeed = 3.0F;
            projectile.Asset = "FireBullet";
            projectile.Name = "Fire Bullet";
            ProjectileEffect fireTurretEffect = new ProjectileEffect();
            fireTurretEffect.EffectType = ProjectileEffectType.Damage;
            fireTurretEffect.EffectDuration = 3.0F;
            fireTurretEffect.EffectImpact = 1.0F;
            projectile.Effects.Add(iceTurretEffect);
            turret.ProjectileType = projectile;
            level.AllowedTurrets.Add(turret);


            level.FieldWidth = 10;
            level.FieldDepth = 20;

            level.Map = new List<char>
    {
        'D','D','D','D','D','D','E','D','D','D',
        'D','D','D','R','R','R','R','D','D','D',
        'D','D','D','R','D','D','D','D','D','D',
        'D','D','T','R','D','D','D','D','D','D',
        'D','D','D','R','D','D','D','D','D','D',
        'D','D','D','R','R','R','R','R','D','D',
        'D','D','D','D','D','T','D','R','D','D',
        'D','D','D','D','D','D','D','R','D','D',
        'D','D','D','D','D','D','D','R','T','D',
        'D','D','D','D','D','D','D','R','D','D',
        'D','D','D','R','R','R','R','R','D','D',
        'D','D','D','R','D','T','D','D','D','D',
        'D','D','D','R','D','D','D','D','D','D',
        'D','D','D','R','D','D','D','D','D','D',
        'D','D','D','R','D','D','D','D','D','D',
        'D','D','D','R','R','R','R','D','D','D',
        'D','D','D','D','T','D','R','D','D','D',
        'D','D','D','D','D','D','R','T','D','D',
        'D','D','D','D','D','D','R','D','D','D',
        'D','D','D','D','D','D','X','D','D','D',
    };


            return level;
        }
    }


}