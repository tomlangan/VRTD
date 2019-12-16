using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyDescription
{
    public string Name { get; set; }

    public double SpawnRate { get; set; }

    public double MovementSpeed { get; set; }

    public int HitPoints { get; set; }

    public string Asset { get; set; }
}

public class EnemyWave
{
    public EnemyDescription EnemyType { get; set; }

    public int Count { get; set; }

    public double DifficultyMultiplier { get; set; }

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

    public double EffectDuration { get; set;  }

    public double EffectImpact { get; set; }
}

public class Projectile
{ 
    public string Name { get; set; }

    public string Asset { get; set; }

    public double AirSpeed { get; set; }

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

    public double FireRate { get; set; }

    public double Range { get; set; }

    public Projectile ProjectileType { get; set; }
}


public class MapPos
{
    public int x;
    public int y;
    public List<EnemyInstance> EnemiesOccupying;

    public MapPos(int xpos, int ypos)
    {
        x = xpos;
        y = ypos;
    }

    public MapPos(MapPos pos)
    {
        x = pos.x;
        y = pos.y;
    }

    public double DistanceTo(MapPos pos)
    {
        // Pythagorian theorem:  a^2 + b^2 = c^2
        double aSquared = Math.Pow(Math.Abs(x - pos.x), 2);
        double bSquared = Math.Pow(Math.Abs(y - pos.y), 2);
        return Math.Sqrt(aSquared + bSquared);
    }

    public double DistanceTo(RealPos pos)
    {
        // Pythagorian theorem:  a^2 + b^2 = c^2
        double aSquared = Math.Pow(Math.Abs(x - pos.x), 2);
        double bSquared = Math.Pow(Math.Abs(y - pos.y), 2);
        return Math.Sqrt(aSquared + bSquared);
    }
}


public class RealPos
{
    public double x;
    public double y;
    public List<EnemyInstance> EnemiesOccupying;

    public RealPos(double xpos, double ypos)
    {
        x = xpos;
        y = ypos;
    }

    public RealPos(RealPos pos)
    {
        x = pos.x;
        y = pos.y;
    }

    public double DistanceTo(RealPos pos)
    {
        // Pythagorian theorem:  a^2 + b^2 = c^2
        double aSquared = Math.Pow(Math.Abs(x - pos.x), 2);
        double bSquared = Math.Pow(Math.Abs(y - pos.y), 2);
        return Math.Sqrt(aSquared + bSquared);
    }
    public double DistanceTo(MapPos pos)
    {
        // Pythagorian theorem:  a^2 + b^2 = c^2
        double aSquared = Math.Pow(Math.Abs(x - pos.x), 2);
        double bSquared = Math.Pow(Math.Abs(y - pos.y), 2);
        return Math.Sqrt(aSquared + bSquared);
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