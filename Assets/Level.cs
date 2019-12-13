using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDescription
{
    public string Name { get; set; }

    public double Rate { get; set; }

    public double Speed { get; set; }

    public int HitPoints { get; set; }

    public string Asset { get; set;  }
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
// S = Source
// F = Finish line
// D = Decoration
//
public enum MapElement { R=0, T=1, S=2, F=3, D=4};

public class Turret
{
    public string Name { get; set;  }

    public string Asset { get; set;  }

    public double FireRate { get; set; }

    public double Damage { get; set; }

    public double Range { get; set;  }
}

public class LevelDescription
{
    public string Name { get; set; }

    public List<EnemyWave> Waves;

    public List<Turret> AllowedTurrets;

    public int FieldWidth { get; set; }
    public int FieldDepth { get; set; }

    public List<char> Map;
}


public class RoadPos
{
    public int[] p;
    public int e;

    public RoadPos(int x, int y)
    {
        p = new int[2];
        p[0] = x;
        p[1] = y;
        e = 0;
    } 
}


public class TurretPos
{
    public int[] p;
    public int e;

    public TurretPos(int x, int y)
    {
        p = new int[2];
        p[0] = x;
        p[y] = y;
        e = 0;
    }
}

public class Level : MonoBehaviour
{
    public LevelDescription LevelDesc;
    RoadPos entry;
    RoadPos exit;
    public List<RoadPos> road;
    public List<TurretPos> turrets;
    public delegate bool OnFound(int x, int y);

    // Start is called before the first frame update
    void Start()
    {
        LevelDesc = GetTestLevel();
        LoadAndValidateLevel(LevelDesc);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FindMapLocations(List<char> map, int width, int height, char c, OnFound callback)
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

    void LoadAndValidateLevel(LevelDescription level)
    {
        entry = null;
        exit = null;
        road = new List<RoadPos>();
        turrets = new List<TurretPos>();

        // Find the entry, ensure there are no dupes
        FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'E',  (x,y) =>
        {
            Debug.Assert(null == entry);
            entry = new RoadPos(x, y);
            return true;
        });


        // Find the exit, ensure there are no dupes
        FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'F', (x, y) =>
        {
            Debug.Assert(null == exit);
            exit = new RoadPos(x, y);
            return true;
        });

    }

    void ReadFromFile(string levelFile)
    {

    }

    LevelDescription GetTestLevel()
    {
        LevelDescription level = new LevelDescription();

        level.Name = "0-0";

        level.Waves = new List<EnemyWave>();

        EnemyWave wave = new EnemyWave();
        wave.EnemyType = new EnemyDescription();
        wave.EnemyType.Name = "Easy";
        wave.EnemyType.Speed = 1.0;
        wave.EnemyType.Rate = 1.0;
        wave.EnemyType.HitPoints = 2;
        wave.EnemyType.Asset = "Cube";
        wave.Count = 10;
        wave.DifficultyMultiplier = 1.0;

        level.Waves.Add(wave);

        wave = new EnemyWave();
        wave.EnemyType = new EnemyDescription();
        wave.EnemyType.Name = "Swarm";
        wave.EnemyType.Speed = 5.0;
        wave.EnemyType.Rate = 5.0;
        wave.EnemyType.HitPoints = 1;
        wave.EnemyType.Asset = "Cylinder";
        wave.Count = 20;
        wave.DifficultyMultiplier = 1.0;

        level.Waves.Add(wave);

        level.AllowedTurrets = new List<Turret>();

        Turret turret = new Turret();
        turret.Name = "Basic";
        turret.Asset = "None";
        turret.Damage = 1.0;
        turret.FireRate = 1.0;
        turret.Range = 5;

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
            'D','D','D','D','D','D','F','D','D','D',
        };


        return level;
    }
}
