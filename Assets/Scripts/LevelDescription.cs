using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

public class Turret
{
    public string Name { get; set; }

    public string Asset { get; set; }

    public double FireRate { get; set; }

    public double Damage { get; set; }

    public double Range { get; set; }
}


public class RoadPos
{
    public int x;
    public int y;
    public int e;

    public RoadPos(int xpos, int ypos)
    {
        x = xpos;
        y = ypos;
        e = 0;
    }
}


public class TurretPos
{
    public int x;
    public int y;
    public int e;

    public TurretPos(int xpos, int ypos)
    {
        x = xpos;
        y = ypos;
        e = 0;
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

    public RoadPos Entry;
    public RoadPos Exit;
    public List<RoadPos> Road;
    public List<TurretPos> Turrets;
}