using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyDescription
{
    public string Name { get; set; }

    public float SpawnRate { get; set; }

    public float MovementSpeed { get; set; }

    public int HitPoints { get; set; }

    public string Asset { get; set; }
}

public class EnemyWave
{
    public EnemyDescription EnemyType { get; set; }

    public int Count { get; set; }

    public float DifficultyMultiplier { get; set; }

    public Time CalculatedTime { get; set; }
}

    //
    // R = Road
    // T = Turret location
    // E = Entry
    // X = Exit
    // D = Decoration
    //
public enum MapElement { R = 0, T = 1, S = 2, X = 3, D = 4 };

public enum ProjectileEffectType {  Damage, Slow }

public class ProjectileEffect
{
    public ProjectileEffectType EffectType { get; set; }

    public float EffectDuration { get; set;  }

    public float EffectImpact { get; set; }
}

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

public class Turret
{
    public string Name { get; set; }

    public string Asset { get; set; }

    public float FireRate { get; set; }

    public float Range { get; set; }

    public Projectile ProjectileType { get; set; }
}


public class MapPos
{
    public Vector3Int Pos;
    public List<EnemyInstance> EnemiesOccupying;

    public int x { get { return Pos.x;  } }
    public int z { get { return Pos.z; } }

    public MapPos(int xpos, int zpos)
    {
        Pos.x = xpos;
        Pos.z = zpos;
        EnemiesOccupying = new List<EnemyInstance>();
    }

    public MapPos(MapPos pos)
    {
        Pos.x = pos.Pos.x;
        Pos.z = pos.Pos.z;
    }

    public float DistanceTo(MapPos pos)
    {
        // Pythagorian theorem:  a^2 + b^2 = c^2
        float aSquared = (float)Math.Pow(Math.Abs(Pos.x - pos.x), 2);
        float bSquared = (float)Math.Pow(Math.Abs(Pos.z - pos.z), 2);
        return (float)Math.Sqrt(aSquared + bSquared);
    }

    public float DistanceTo(Vector3 pos)
    {
        // Pythagorian theorem:  a^2 + b^2 = c^2
        float aSquared = (float)Math.Pow(Math.Abs(Pos.x - pos.x), 2);
        float bSquared = (float)Math.Pow(Math.Abs(Pos.z - pos.z), 2);
        return (float)Math.Sqrt(aSquared + bSquared);
    }
}



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