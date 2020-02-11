using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;
#if LEVEL_EDITOR
using System.Numerics;
public class Debug
{
    public static void Assert(bool b)
    {
        if (!b)
        {
            throw new Exception("Error");
        }
    }
    public static void Log(string s)
    {
        //System.Diagnostics.Debug.WriteLine(s);
    }
    public static void LogError(string s)
    {
        //System.Diagnostics.Debug.WriteLine(s);
    }
}
#else
using UnityEngine;
#endif

namespace VRTD.Gameplay
{

#if LEVEL_EDITOR
    public class Application
    {
        public static string streamingAssetsPath = "Assets";
    }



public class GameObject { }

    public class GameObjectFactory
    {
        public static GameObject InstantiateObject(string asset) { return new GameObject(); }
        public static void SetMapPos(GameObject go, VRTD.Gameplay.MapPos Position) { }
        public static void SetMapPos(GameObject go, Vector3 Position) { }
        public static MapPos WorldVec3ToMapPos(Vector3 vec3) { return new MapPos(); }
        public static void Destroy(GameObject go) { }
        public static void SetWorldPos(GameObject go, Vector3 vec3) { }

        public Vector3 forward;
    }
#endif



    [Serializable]
    public class AssetDirectory
    {
        public static string ThisFile = "assetdirectory.txt";

        public List<string> LevelFiles;

        public string TurretFile = "turrets.txt";

        public string EnemyFile = "enemies.txt";

        public string ProjectileFile = "projectiles.txt";
    }

    public class LevelLoadException : Exception
    {
        public LevelLoadException(string message) : base(message)
        {

        }
    }

    public class LevelLoader
    {
        static string RootLevelPath = "Levels";
        private static List<Turret> turrets = null;
        private static List<EnemyDescription> enemies = null;
        private static List<Projectile> projectiles = null;
        private static AssetDirectory AssetLocations
        {
            get
            {
                if (null == _al)
                {
                    _al = ReadObjectFromFile<AssetDirectory>(AssetDirectory.ThisFile);
                }
                return _al;
            }
        }
        private static AssetDirectory _al = null;

        public static List<Turret> Turrets
        {
            get
            {
                if (null == turrets)
                {
                    turrets = GetTurrets();
                }
                return turrets;
            }
        }
        public static List<EnemyDescription> Enemies
        {
            get
            {
                if (null == enemies)
                {
                    enemies = GetEnemies();
                }
                return enemies;
            }
        }
        public static List<Projectile> Projectiles
        {
            get
            {
                if (null == projectiles)
                {
                    projectiles = GetProjectiles();
                }
                return projectiles;
            }
        }


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

                //check right if we're not at the far right
                if ((last.x < (width - 1))
                    &&
                    // This isn't the previous item we just saw
                    (previousDirection != WalkDir.Left)
                    &&
                    (('R' == map[(int)((last.z) * width) + (last.x + 1)])
                    ||
                    ((exit.x == (last.x + 1)) && (exit.z == last.z))))
                {
                    if(null != next)
                    {
                        throw new LevelLoadException("Found multiple paths while walking the road - is there a fork?");
                    }
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
                    if (null != next)
                    {
                        throw new LevelLoadException("Found multiple paths while walking the road - is there a fork?");
                    }
                    thisDirection = WalkDir.Down;
                    next = new MapPos(last.x, last.z + 1);
                }

                //check up if we're not at the top
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
                    if (null != next)
                    {
                        throw new LevelLoadException("Found multiple paths while walking the road - is there a fork?");
                    }
                    thisDirection = WalkDir.Up;
                    next = new MapPos(last.x, last.z - 1);
                }


                // We didn't find a next road segment!
                if (null == next)
                {
                    throw new LevelLoadException("Didn't find a road segment while walking it - is there a dead end or gap?");
                }

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
            if(found == max)
            {
                throw new LevelLoadException("Found more road than there are tiles - possible cycle in the road");
            }
            // Something went wrong if we got here and never found the exit.
            if (((exit.x != last.x) || (exit.z != last.z)))
            {
                throw new LevelLoadException("The last road segment was not the exit - is the exit connected to the road?");
            }

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
                    if (null != level.Entry)
                    {
                        throw new LevelLoadException("Duplicate Entries found in the map");
                    }
                    level.Entry = new MapPos(x, y);
                    return true;
                });


            // Find the exit, ensure there are no dupes
            FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'X', (x, y) =>
            {
                if (null != level.Exit)
                {
                    throw new LevelLoadException("Duplicate Exits found in the map");
                }
                level.Exit = new MapPos(x, y);
                return true;
            });

            // Find all the turrets
            FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'T', (x, y) =>
            {
                level.Turrets.Add(new MapPos(x, y));
                return true;
            });

            if (null == level.Entry)
            {
                throw new LevelLoadException("Entry not found!");
            }


            if (null == level.Exit)
            {
                throw new LevelLoadException("Exit not found!");
            }



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


        static string LevelToFilename(string level)
        {
            return level + "-lvl.txt";
        }

        public static List<string> GetAllLevels()
        {
            List<string> levelList = AssetLocations.LevelFiles;

            levelList.Sort();

            return levelList;
        }

        public static LevelDescription GetLevel(string name)
        {
            return ReadObjectFromFile<LevelDescription>(LevelToFilename(name));
        }
        public static List<Turret> GetTurrets()
        {
            return ReadObjectFromFile<List<Turret>>(AssetLocations.TurretFile);
        }

        public static Turret LookupTurret(string name)
        {
            Turret found = null;
            if (null != Turrets)
            {
                for (int i = 0; i < Turrets.Count; i++)
                {
                    if (Turrets[i].Name == name)
                    {
                        found = Turrets[i];
                        break;
                    }
                }
            }

            return found;
        }

        public static List<EnemyDescription> GetEnemies()
        {
            return ReadObjectFromFile<List<EnemyDescription>>(AssetLocations.EnemyFile);
        }


        public static EnemyDescription LookupEnemy(string name)
        {
            EnemyDescription found = null;
            if (null != Enemies)
            {
                for (int i = 0; i < Enemies.Count; i++)
                {
                    if (Enemies[i].Name == name)
                    {
                        found = Enemies[i];
                        break;
                    }
                }
            }

            return found;
        }

        public static List<Projectile> GetProjectiles()
        {
            return ReadObjectFromFile<List<Projectile>>(AssetLocations.ProjectileFile);
        }


        public static Projectile LookupProjectile(string name)
        {
            Projectile found = null;
            if (null != Projectiles)
            {
                for (int i = 0; i < Projectiles.Count; i++)
                {
                    if (Projectiles[i].Name == name)
                    {
                        found = Projectiles[i];
                        break;
                    }
                }
            }

            return found;
        }



        public static string ReadAllBytesFromTextAsset(string filePath)
        {
            string result = "";
#if LEVEL_EDITOR != true
            if (!filePath.EndsWith(".txt"))
            {
                Utilities.LogError("File does not end with '.txt': " + filePath);
            }
            string pathWithoutJsonExtension = filePath.Substring(0, filePath.Length - 4);
            TextAsset fileAsset = Resources.Load<TextAsset>(pathWithoutJsonExtension);
            if (null != fileAsset)
            {
                using (StreamReader sr = new StreamReader(new MemoryStream(fileAsset.bytes)))
                {
                    result = sr.ReadToEnd();
                }
            }
            else
            {
                Utilities.LogError("Failed to open file: " + pathWithoutJsonExtension);
            }
            
#endif
            return result;
        }

        public static T ReadObjectFromFile<T>(string filePath)
        {
            T fromJson;
            string path = RootLevelPath + Path.DirectorySeparatorChar + filePath;


            string json = ReadAllBytesFromTextAsset(path);
            fromJson = JsonConvert.DeserializeObject<T>(json);

            return fromJson;
        }




        public static List<Projectile> GetTestProjectiles()
        {
            List<Projectile> list = new List<Projectile>();
            Projectile projectile = new Projectile();
            projectile.AirSpeed = 4.0F;
            projectile.Asset = "BasicBullet";
            projectile.Name = "Basic Bullet";
            ProjectileEffect basicTurretDamangeEffect = new ProjectileEffect();
            basicTurretDamangeEffect.EffectType = ProjectileEffectType.Damage;
            basicTurretDamangeEffect.EffectDuration = 0.0F;
            basicTurretDamangeEffect.EffectImpact = 1.0F;
            projectile.Effects.Add(basicTurretDamangeEffect);
            list.Add(projectile);


            projectile = new Projectile();
            projectile.AirSpeed = 4.0F;
            projectile.Asset = "IceBullet";
            projectile.Name = "Ice Bullet";
            ProjectileEffect iceTurretEffect = new ProjectileEffect();
            iceTurretEffect.EffectType = ProjectileEffectType.Slow;
            iceTurretEffect.EffectDuration = 4.0F;
            iceTurretEffect.EffectImpact = 3.0F;
            projectile.Effects.Add(iceTurretEffect);
            list.Add(projectile);


            projectile = new Projectile();
            projectile.AirSpeed = 3.0F;
            projectile.Asset = "FireBullet";
            projectile.Name = "Fire Bullet";
            ProjectileEffect fireTurretEffect = new ProjectileEffect();
            fireTurretEffect.EffectType = ProjectileEffectType.Damage;
            fireTurretEffect.EffectDuration = 3.0F;
            fireTurretEffect.EffectImpact = 1.0F;
            projectile.Effects.Add(iceTurretEffect);
            list.Add(projectile);

            return list;
        }

        public static List<EnemyDescription> GetTestEnemies()
        {
            List<EnemyDescription> list = new List<EnemyDescription>();


            EnemyDescription EnemyType = new EnemyDescription();
            EnemyType.Name = "Basic Enemy";
            EnemyType.MovementSpeed = 1.0F;
            EnemyType.SpawnRate = 1.0F;
            EnemyType.HitPoints = 2;
            EnemyType.Asset = "BasicEnemy";
            list.Add(EnemyType);


            EnemyType = new EnemyDescription();
            EnemyType.Name = "Swarm Enemy";
            EnemyType.MovementSpeed = 5.0F;
            EnemyType.SpawnRate = 5.0F;
            EnemyType.HitPoints = 1;
            EnemyType.Asset = "SwarmEnemy";
            list.Add(EnemyType);

            return list;
        }


        public static List<Turret> GetTestTurrets()
        {
            List<Turret> list = new List<Turret>();


            Turret turret = new Turret();
            turret.Name = "Basic Turret";
            turret.Asset = "BasicTurret";
            turret.FireRate = 2.0F;
            turret.Range = 5;
            turret.Projectile = "Basic Bullet";
            list.Add(turret);


            turret = new Turret();
            turret.Name = "Ice Shot";
            turret.Asset = "IceTurret";
            turret.FireRate = 5.0F;
            turret.Range = 3;
            turret.Projectile = "Ice Bullet";
            list.Add(turret);


            turret = new Turret();
            turret.Name = "Fire Shot";
            turret.Asset = "FireTurret";
            turret.FireRate = 2.0F;
            turret.Range = 6;
            turret.Projectile = "Fire Bullet";
            list.Add(turret);

            return list;
        }

            public static LevelDescription GetTestLevel()
        {
            LevelDescription level = new LevelDescription();

            level.Name = "0-0";

            level.Waves = new List<EnemyWave>();

            EnemyWave wave = new EnemyWave();
            wave.Enemy = "Basic Enemy";
            wave.Count = 10;
            wave.DifficultyMultiplier = 1.0F;

            level.Waves.Add(wave);

            wave = new EnemyWave();
            wave.Enemy = "Swarm Enemy";
            wave.Count = 20;
            wave.DifficultyMultiplier = 1.0F;

            level.Waves.Add(wave);

            level.AllowedTurrets = new List<string>();

            level.AllowedTurrets.Add("Basic Turret");

            level.AllowedTurrets.Add("Ice Shot");

            level.AllowedTurrets.Add("Fire Shot");



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