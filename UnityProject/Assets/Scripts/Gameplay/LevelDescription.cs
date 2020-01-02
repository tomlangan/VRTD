using System.Collections;
using System.Collections.Generic;
using System;
#if LEVEL_EDITOR
using System.Numerics;


    public class Vector3Int
    {
        public int x;
        public int y;
        public int z;

        public Vector3Int(int xset, int yset, int zset) { x = xset; y = yset; z = zset; }
    }
#else
using UnityEngine;
#endif



namespace VRTD.Gameplay
{

	[Serializable]
    public class EnemyDescription
    {
        public string Name { get; set; }

        public float SpawnRate { get; set; }

        public float MovementSpeed { get; set; }

        public int HitPoints { get; set; }

        public string Asset { get; set; }
    }

    [Serializable]
    public class EnemyWave
    {
        public EnemyDescription EnemyType { get; set; }

        public int Count { get; set; }

        public float DifficultyMultiplier { get; set; }

    }

    //
    // R = Road
    // T = Turret location
    // E = Entry
    // X = Exit
    // D = Decoration
    //
    [Serializable]
    public enum MapElement { R = 0, T = 1, S = 2, X = 3, D = 4 };

    [Serializable]
    public enum ProjectileEffectType { Damage, Slow }

    [Serializable]
    public class ProjectileEffect
    {
        public ProjectileEffectType EffectType { get; set; }

        public float EffectDuration { get; set; }

        public float EffectImpact { get; set; }
    }

    [Serializable]
    public class Projectile
    {
        public string Name { get; set; }

        public string Asset { get; set; }

        public float AirSpeed { get; set; }

        public List<ProjectileEffect> Effects;

        public Projectile()
        {
            Effects = new List<ProjectileEffect>();
        }
    }

    [Serializable]
    public class Turret
    {
        public string Name { get; set; }

        public string Asset { get; set; }

        public float FireRate { get; set; }

        public float Range { get; set; }

        public Projectile ProjectileType { get; set; }
    }


    [Serializable]
    public class MapPos
    {

        public int x { get; set; }
        public int z { get; set; }

        public Vector3 Pos
        {
            get { return new Vector3(x, 0, z); }
        }

        public List<EnemyInstance> EnemiesOccupying;

        public MapPos(int xpos, int zpos)
        {
            x = xpos;
            z = zpos;
            EnemiesOccupying = new List<EnemyInstance>();
        }

        public MapPos(MapPos pos)
        {
            x = pos.x;
            z = pos.z;
        }

#if LEVEL_EDITOR != true
    public float DistanceTo(MapPos pos)
    {
        return Vector3.Distance(Pos, pos.Pos);
    }

    public float DistanceTo(Vector3 pos)
    {
        return Vector3.Distance(Pos, pos);
    }
#endif
    }


    [Serializable]
    public class LevelDescription
    {
        public string Name { get; set; }

        public List<EnemyWave> Waves;

        public List<Turret> AllowedTurrets;

        public int FieldWidth { get; set; }
        public int FieldDepth { get; set; }

        public List<char> Map;

        public MapPos Entry;
        public MapPos Exit;
        public List<MapPos> Road;
        public List<MapPos> Turrets;
    }
}