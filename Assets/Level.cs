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
// E = Entry
// X = Exit
// D = Decoration
//
public enum MapElement { R=0, T=1, S=2, X=3, D=4};

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

public class EnemyInstance
{
    public EnemyDescription Desc;
    public double Pos;
    public double Health;
    public double NextMovement;
}

public class WaveInstance
{
    public List<EnemyInstance> Enemies;
    public EnemyWave Desc;
    public double WaveStartTime;
    public int SpawnedCount;
    public double LastSpawnTime;

    public WaveInstance(EnemyWave waveDescription)
    {
        Desc = waveDescription;
        Enemies = new List<EnemyInstance>();
    }


}

public class WaveManager
{
    public List<EnemyWave> Waves;
    public int WavesStarted;
    public WaveInstance CurrentWave;

    public WaveManager(LevelDescription level)
    {
        WavesStarted = 0;
        TotalTime = 0.0;
        Waves = level.Waves;
    }

    public void AdvanceToNextWave()
    {
        Debug.Assert(WavesStarted < Waves.Count);

        CurrentWave = new WaveInstance(Waves[WavesStarted]);

        WavesStarted++;
    }
}

public enum LevelState { Loading, WaveCountdown, Playing, StatsScreen }

public class Level : MonoBehaviour
{
    const int WAVE_COUNTDOWN_TIME = 5.0;
    public LevelDescription LevelDesc;
    RoadPos Entry;
    RoadPos Exit;
    public List<RoadPos> Road;
    public List<TurretPos> Turrets;
    public delegate bool OnFound(int x, int y);
    WaveManager Waves;
    public double GameTime;
    public LevelState State = LevelState.Loading;

    // Start is called before the first frame update
    void Start()
    {
        State = LevelState.Loading;

    }

    // Update is called once per frame
    void Update()
    {

        switch (State)
        {
            case LevelState.Loading:
                LevelDesc = GetTestLevel();
                LoadAndValidateLevel(LevelDesc);
                State = LevelState.WaveCountdown;
                GameTime = 0.0;
                break;

            case LevelState.WaveCountdown:
                TickWaveCountdown();
                break;

            case LevelState.Playing:
                TickGameplay();
                break;

            case LevelState.StatsScreen:

                break;
        }


        AdvanceEnemies(elapsed);
        SpawnEnemies(elapsed);
        CheckEndConditions();
    }

    void TickWaveCountdown()
    {
        double elapsed = Time.deltaTime;
        GameTime += elapsed;

        if (GameTime > WAVE_COUNTDOWN_TIME)
        {
            GameTime = 0.0;
            Waves.Ad
            State = LevelState.Playing;
        }
    }

    void TickGameplay()
    {
        double elapsed = Time.deltaTime;
        GameTime += elapsed;


    }

    void SpawnEnemies()
    {

    }

    void AdvanceEnemies()
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            EnemyInstance e = Enemies[i];


        }
    }

    void CheckEndConditions()
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


    enum WalkDir { Up, Down, Left, Right, None };

    List<RoadPos> WalkAndValidateRoad(List<char> map, int width, int height, RoadPos entry, RoadPos exit)
    {
        WalkDir previousDirection = WalkDir.None;
        WalkDir thisDirection = WalkDir.None;
        List<RoadPos> r = new List<RoadPos>();
        RoadPos last = null;
        RoadPos next = null;
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
                (('R' == map[(last.y * width) + (last.x - 1)])
                ||
                // is the item to the left the exit?
                ((exit.x == (last.x-1)) && ( exit.y == last.y))))
            {
                thisDirection = WalkDir.Left;
                next = new RoadPos(last.x - 1, last.y);
            }

            //check right if we're not at the bottom
            if ((last.x < (width - 1))
                &&
                // This isn't the previous item we just saw
                (previousDirection != WalkDir.Left)
                &&
                (('R' == map[((last.y) * width) + (last.x + 1)])
                ||
                ((exit.x == (last.x + 1)) && (exit.y == last.y))))
            {
                Debug.Assert(null == next);
                thisDirection = WalkDir.Right;
                next = new RoadPos(last.x + 1, last.y);
            }

            //check down if we're not at the bottom
            if ((last.y < (height-1))
                &&
                // This isn't the previous item we just saw
                (previousDirection != WalkDir.Up)
                &&
                (('R' == map[((last.y+1) * width) + (last.x)])
                ||
                // is the item downwards the exit?
                ((exit.x == (last.x)) && (exit.y == (last.y+1)))))
            {
                Debug.Assert(null == next);
                thisDirection = WalkDir.Down;
                next = new RoadPos(last.x, last.y+1);
            }

            //check up if we're not at the bottom
            if ((last.y > 0)
                &&
                // This isn't the previous item we just saw
                (previousDirection != WalkDir.Down)
                &&
                (('R' == map[((last.y -1) * width) + (last.x)])
                ||
                // is the item upwards the exit?
                ((exit.x == (last.x)) && (exit.y == (last.y - 1)))))
            {
                Debug.Assert(null == next);
                thisDirection = WalkDir.Up;
                next = new RoadPos(last.x, last.y - 1);
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
            if ((exit.x == last.x) && (exit.y == last.y)) { break; }
        }

        // Something went wrong if we found this many items!
        Debug.Assert(found != max);
        // Something went wrong if we got here and never found the exit.
        Debug.Assert(((exit.x == last.x) && (exit.y == last.y)));

        return r;
    }

    void LoadAndValidateLevel(LevelDescription level)
    {
        Entry = null;
        Exit = null;
        Road = null;
        Turrets = new List<TurretPos>();

        // Find the entry, ensure there are no dupes
        FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'E',  (x,y) =>
        {
            Debug.Assert(null == Entry);
            Entry = new RoadPos(x, y);
            return true;
        });


        // Find the exit, ensure there are no dupes
        FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'X', (x, y) =>
        {
            Debug.Assert(null == Exit);
            Exit = new RoadPos(x, y);
            return true;
        });

        // Find all the turrets
        FindMapLocations(level.Map, level.FieldWidth, level.FieldDepth, 'T', (x, y) =>
        {
            Turrets.Add(new TurretPos(x, y));
            return true;
        });

        Road = WalkAndValidateRoad(level.Map, level.FieldWidth, level.FieldDepth, Entry, Exit);

        Waves = new WaveManager(level);
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
            'D','D','D','D','D','D','X','D','D','D',
        };


        return level;
    }
}
