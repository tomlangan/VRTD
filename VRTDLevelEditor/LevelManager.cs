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
        public static string FolderPath;

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

            return (true == FolderPath.EndsWith("Levels"));
        }

        public static List<string> GetList()
        {
            List<string> levelList = new List<string>();

            string[] files = Directory.GetFiles(FolderPath);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith(".lvl"))
                {
                    levelList.Add(Path.GetFileNameWithoutExtension(files[i]));
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
        }

        public static void RenameLevel(string oldName, string newName)
        {
            File.Move(LevelToPath(oldName), LevelToPath(newName));
        }

        public static LevelDescription ReadLevel(string level)
        {
            FileStream fs = File.Open(LevelToPath(level), FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            string jsonBlob = sr.ReadToEnd();
            LevelDescription desc = JsonConvert.DeserializeObject<LevelDescription>(jsonBlob);
            sr.Close();
            fs.Close();
            fs.Dispose();
            return desc;
        }

        static string LevelToPath(string level)
        {
            return FolderPath + Path.DirectorySeparatorChar + level + ".lvl";
        }

        static string AttemptToFindLevelDirectory()
        {
            string directory = Directory.GetCurrentDirectory();

            while (directory.Length > 0 && !directory.EndsWith("TD"))
            {
                try
                {
                    directory = Directory.GetParent(directory).ToString();
                } catch
                {
                    break;
                }
            }

            if (directory.EndsWith("TD"))
            {
                directory += Path.DirectorySeparatorChar + "UnityProject" +
                    Path.DirectorySeparatorChar + "Assets" +
                    Path.DirectorySeparatorChar + "Levels";
            }
            else
            {
                return null;
            }

            return directory;
        }
    }
}
