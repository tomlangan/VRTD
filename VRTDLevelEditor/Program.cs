using System;
using Gtk;
using System.Reflection;
using System.IO;

namespace VRTD.LevelEditor
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Application.Init();
            MainWindow win = new MainWindow();
            win.Show();
            Application.Run();
        }
    }
}

