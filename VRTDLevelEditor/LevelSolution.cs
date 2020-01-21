using System;
using System.Collections.Generic;
using VRTD.Gameplay;

namespace VRTD.LevelEditor
{
    [Serializable]
    public class WaveSolutionTurret
    {
        public WaveSolutionTurret()
        {
            pos = new MapPos();
        }

        public MapPos pos;
        public string Name;
    }

    [Serializable]
    public class WaveSolution
    {
        public WaveSolution()
        {
            Turrets = new List<WaveSolutionTurret>();
        }

        public int WaveIndex;
        public List<WaveSolutionTurret> Turrets;


        public bool Same(WaveSolution s)
        {
            if (s.WaveIndex != WaveIndex)
            {
                return false;
            }

            if (s.Turrets.Count != Turrets.Count)
            {
                return false;
            }

            for (int i = 0; i < Turrets.Count; i++)
            {
                if (Turrets[i].Name != s.Turrets[i].Name)
                {
                    return false;
                }
                if (Turrets[i].pos.x != s.Turrets[i].pos.x)
                {
                    return false;
                }
                if (Turrets[i].pos.z != s.Turrets[i].pos.z)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [Serializable]
    public class LevelSolution
    {
        public LevelSolution()
        {
            WaveSolutions = new List<WaveSolution>();
        }

        public List<WaveSolution> WaveSolutions;

    }
}
