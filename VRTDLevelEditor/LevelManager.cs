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
        public static string FolderPath = "";
        private static AssetDirectory AssetLocations = null;


        static string FilenameToPath(string filename)
        {
            return FolderPath + Path.DirectorySeparatorChar + filename;
        }


        static string LevelToPath(string level)
        {
            return FilenameToPath(level + "-lvl.txt");
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

        static void WriteObjectToile<T>(T objectToWrite, string fileName)
        {
            string json = JsonConvert.SerializeObject(objectToWrite);
            using (FileStream fs = File.Open(FilenameToPath(fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }
        }

        public static bool Initialize(Window parent)
        {

            FolderPath = AttemptToFindLevelDirectory();

            if (null == FolderPath)
            {
                FileChooserDialog fcd = new FileChooserDialog("Provide path to TD directory", null, FileChooserAction.SelectFolder);
                fcd.AddButton(Stock.Cancel, ResponseType.Cancel);
                fcd.AddButton(Stock.Open, ResponseType.Ok);
                fcd.DefaultResponse = ResponseType.Ok;
                fcd.SelectMultiple = false;

                ResponseType response = (ResponseType)fcd.Run();
                if (response == ResponseType.Ok)
                {
                    FolderPath = fcd.Filename;
                }
                else
                {
                    FolderPath = null;
                }
                fcd.Destroy();
            }

            if (FolderPath.EndsWith("Levels"))
            {
                AssetLocations = ReadObjectFromFile<AssetDirectory>(FilenameToPath(AssetDirectory.ThisFile));
                if (null == AssetLocations)
                {
                    AssetLocations = new AssetDirectory();
                    AssetLocations.LevelFiles = GetLevelList();
                    WriteAssetDirectory();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<string> GetLevelList()
        {
            List<string> levelList = new List<string>();

            string[] files = Directory.GetFiles(FolderPath);

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
            WriteAssetDirectory();
        }

        public static LevelDescription ReadLevel(string level)
        {
            return ReadObjectFromFile<LevelDescription>(LevelToPath(level));
        }

        public static void DeleteLevel(string level)
        {
            string path = LevelToPath(level);
            if (File.Exists(path))
            {
                File.Delete(path);
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

        public static List<Turret> GetTurrets()
        {
            return ReadObjectFromFile<List<Turret>>(FilenameToPath(AssetLocations.TurretFile));
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
            using (FileStream fs = File.Open(FilenameToPath(AssetLocations.TurretFile), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(levelAsJSon);
                }
            }
        }


        public static List<EnemyDescription> GetEnemies()
        {
            return ReadObjectFromFile<List<EnemyDescription>>(FilenameToPath(AssetLocations.EnemyFile));
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
            using (FileStream fs = File.Open(FilenameToPath(AssetLocations.EnemyFile), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(levelAsJSon);
                }
            }
        }


        public static List<Projectile> GetProjectiles()
        {
            return ReadObjectFromFile<List<Projectile>>(FilenameToPath(AssetLocations.ProjectileFile));
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
            using (FileStream fs = File.Open(FilenameToPath(AssetLocations.ProjectileFile), FileMode.Create))
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
            using (FileStream fs = File.Open(FilenameToPath(AssetDirectory.ThisFile), FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(json);
                sw.Close();
            }
        }
    }
}
