using System;
using System.IO;
using Gtk;
using System.Collections.Generic;
using VRTD.Gameplay;
using Newtonsoft.Json;

namespace VRTD.LevelEditor
{

    public class LevelManager
    {
        public static string LevelPath = "";
        public static string SolutionPath = "";
        private static AssetDirectory AssetLocations = null;


        static string FilenameToLevelPath(string filename)
        {
            return LevelPath + Path.DirectorySeparatorChar + filename;
        }

        static string FilenameToSolutionPath(string filename)
        {
            return SolutionPath + Path.DirectorySeparatorChar + filename;
        }

        static string LevelToPath(string level)
        {
            return FilenameToLevelPath(level + "-lvl.txt");
        }
        static string LevelToSolutionPath(string level)
        {
            return FilenameToSolutionPath(level + "-solution.txt");
        }

        static T ReadObjectFromFile<T>(string filename)
        {
            T fromjson;
            using (FileStream fs = File.Open(filename, FileMode.OpenOrCreate))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    string jsonBlob = sr.ReadToEnd();
                    fromjson = JsonConvert.DeserializeObject<T>(jsonBlob);
                }
            }
            return fromjson;
        }

        
        static void WriteObjectToFile<T>(string fileName, T objectToWrite)
        {
            string json = JsonConvert.SerializeObject(objectToWrite);
            using (FileStream fs = File.Open(fileName, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }
        }
        


        public static bool Initialize(Window parent)
        {

            LevelPath = AttemptToFindLevelDirectory();

            if (null == LevelPath)
            {
                FileChooserDialog fcd = new FileChooserDialog("Provide path to TD directory", null, FileChooserAction.SelectFolder);
                fcd.AddButton(Stock.Cancel, ResponseType.Cancel);
                fcd.AddButton(Stock.Open, ResponseType.Ok);
                fcd.DefaultResponse = ResponseType.Ok;
                fcd.SelectMultiple = false;

                ResponseType response = (ResponseType)fcd.Run();
                if (response == ResponseType.Ok)
                {
                    LevelPath = fcd.Filename;
                }
                else
                {
                    LevelPath = null;
                }
                fcd.Destroy();
            }

            if (LevelPath.EndsWith("Levels"))
            {
                AssetLocations = ReadObjectFromFile<AssetDirectory>(FilenameToLevelPath(AssetDirectory.ThisFile));
                if (null == AssetLocations)
                {
                    AssetLocations = new AssetDirectory();
                    AssetLocations.LevelFiles = GetLevelList();
                    WriteAssetDirectory();
                }
                SolutionPath = Directory.GetParent(LevelPath).Parent.Parent.Parent.FullName + Path.DirectorySeparatorChar + "Solutions";
                if (!Directory.Exists(SolutionPath))
                {
                    Directory.CreateDirectory(SolutionPath);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        static string AttemptToFindLevelDirectory()
        {
            string directory = Directory.GetCurrentDirectory();

            while (directory.Length > 0 && !directory.EndsWith("TD"))
            {
                try
                {
                    directory = Directory.GetParent(directory).ToString();
                }
                catch
                {
                    break;
                }
            }

            if (directory.EndsWith("TD"))
            {
                directory += Path.DirectorySeparatorChar + "UnityProject" +
                    Path.DirectorySeparatorChar + "Assets" +
                    Path.DirectorySeparatorChar + "Resources" +
                    Path.DirectorySeparatorChar + "Levels";
            }
            else
            {
                return null;
            }

            return directory;
        }

        public static List<string> GetLevelList()
        {
            List<string> levelList = new List<string>();

            string[] files = Directory.GetFiles(LevelPath);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith("-lvl.txt"))
                {
                    string levelwithextra = Path.GetFileNameWithoutExtension(files[i]);
                    string levelname = levelwithextra.Substring(0, levelwithextra.Length - 4);
                    levelList.Add(levelname);
                }
            }

            levelList.Sort();

            return levelList;
        }

        public static void WriteLevel(string level, LevelDescription desc)
        {
            string levelAsJSon = JsonConvert.SerializeObject(desc);
            FileStream fs = File.Open(LevelToPath(level), FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(levelAsJSon);
            sw.Close();
            fs.Close();
            fs.Dispose();

            WriteAssetDirectory();
        }

        public static void RenameLevel(string oldName, string newName)
        {
            File.Move(LevelToPath(oldName), LevelToPath(newName));
            File.Move(LevelToSolutionPath(oldName), LevelToSolutionPath(newName));
            WriteAssetDirectory();
        }

        public static LevelDescription ReadLevel(string level)
        {
            LevelDescription desc = ReadObjectFromFile<LevelDescription>(LevelToPath(level));

            

            return desc;
        }

        public static void DeleteLevel(string level)
        {
            string path = LevelToPath(level);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }



        public static List<Turret> GetTurrets()
        {
            return ReadObjectFromFile<List<Turret>>(FilenameToLevelPath(AssetLocations.TurretFile));
        }

        public static Turret LookupTurret(string turretName)
        {
            Turret t = null;
            List<Turret> Turrets = GetTurrets();
            for (int i = 0; i < Turrets.Count; i++)
            {
                if (Turrets[i].Name == turretName)
                {
                    t = Turrets[i];
                }
            }

            return t;
        }

        public static void WriteTurrets(List<Turret> turrets)
        {
            if (null == turrets)
            {
                throw new Exception("turret list should never be null");
            }
            string levelAsJSon = JsonConvert.SerializeObject(turrets);
            using (FileStream fs = File.Open(FilenameToLevelPath(AssetLocations.TurretFile), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(levelAsJSon);
                }
            }
        }


        public static List<EnemyDescription> GetEnemies()
        {
            return ReadObjectFromFile<List<EnemyDescription>>(FilenameToLevelPath(AssetLocations.EnemyFile));
        }


        public static EnemyDescription LookupEnemy(string name)
        {
            EnemyDescription enemy = null;
            List<EnemyDescription> Enemies = GetEnemies();
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i].Name == name)
                {
                    enemy = Enemies[i];
                }
            }

            return enemy;
        }

        public static void WriteEnemies(List<EnemyDescription> enemies)
        {
            string levelAsJSon = JsonConvert.SerializeObject(enemies);
            using (FileStream fs = File.Open(FilenameToLevelPath(AssetLocations.EnemyFile), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(levelAsJSon);
                }
            }
        }

        public static List<Projectile> GetProjectiles()
        {
            return ReadObjectFromFile<List<Projectile>>(FilenameToLevelPath(AssetLocations.ProjectileFile));
        }

        public static Projectile LookupProjectile(string name)
        {
            Projectile p = null;
            List<Projectile> Projectiles = GetProjectiles();
            for (int i = 0; i < Projectiles.Count; i++)
            {
                if (Projectiles[i].Name == name)
                {
                    p = Projectiles[i];
                }
            }

            return p;
        }

        public static void WriteProjectiles(List<Projectile> projectiles)
        {
            string levelAsJSon = JsonConvert.SerializeObject(projectiles);
            using (FileStream fs = File.Open(FilenameToLevelPath(AssetLocations.ProjectileFile), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(levelAsJSon);
                }
            }
        }


        public static void WriteAssetDirectory()
        {
            AssetDirectory directory = new AssetDirectory();
            directory.LevelFiles = GetLevelList();
            string json = JsonConvert.SerializeObject(directory);
            using (FileStream fs = File.Open(FilenameToLevelPath(AssetDirectory.ThisFile), FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(json);
                sw.Close();
            }
        }

        public static void WriteLevelSolution(string level, LevelSolution solutions)
        {
            WriteObjectToFile<LevelSolution>(LevelToSolutionPath(level), solutions);
        }


        public static LevelSolution ReadLevelSolution(string level)
        {
            return ReadObjectFromFile<LevelSolution>(LevelToSolutionPath(level));
        }

    }
}
