using System;
using Gtk;
using Newtonsoft.Json;
using VRTD.Gameplay;


namespace VRTD.LevelEditor
{
    public class LevelDescView : Container
    {
        public LevelDescription LevelDesc { get; set; }
        Label NameLabel;

        public LevelDescView()
        {
            Table table = new Table(24, 4, true);

            Add(table);

            NameLabel = new Label();
            table.Attach(NameLabel, 0, 4, 0, 1);
            NameLabel.Show();

        }
           
        public void Refresh(LevelDescription desc)
        {
            NameLabel.Text = desc.Name;

            LevelDesc = desc;
        }
    }

    public partial class MainWindow : Window
    {
        TreeView tree;
        Table table;
        LevelDescription LevelDesc;
        LevelDescView LevelView;

        public MainWindow() :
                base(WindowType.Toplevel)
        {
            Resize(800, 600);
            SetPosition(WindowPosition.Center);

            AddWidgetsAndShow();

            PopulateLevelTree();

            LoadLevel("0-0");
        }

        private void AddWidgetsAndShow()
        {
            /* Set a handler for delete_event that immediately
             * exits GTK. */
            DeleteEvent += delete_event;


            table = new Table(25, 5, true);

            /* Put the table in the main window */
            Add(table);

            /* Create first button */
            tree = new TreeView();

            table.Attach(tree, 0, 1, 0, 24);

            tree.Show();


            Button exportButton = new Button("Export");

            /* When the button is clicked, we call the "callback" function
             * with a pointer to "button 2" as its argument */

            exportButton.Clicked += ExportButton_Clicked;

            /* Insert button 2 into the upper right quadrant of the table */
            table.Attach(exportButton, 1, 2, 24, 25);

            exportButton.Show();

            /* Create "Quit" button */
            Button quitbutton = new Button("Quit");

            /* When the button is clicked, we call the "delete_event" function
             * and the program exits */
            quitbutton.Clicked += exit_event;

            /* Insert the quit button into the both
             * lower quadrants of the table */
            table.Attach(quitbutton, 4, 5, 24, 25);

            quitbutton.Show();

            Button addLevelButton = new Button("+");

            addLevelButton.Clicked += AddLevelButton_Clicked;
            table.Attach(addLevelButton, 0, 1, 24, 25);
            addLevelButton.Show();

            LevelView = new LevelDescView();
            table.Attach(LevelView, 1, 5, 0, 24);
            LevelView.Show();

            table.Show();
            ShowAll();
        }

        private void PopulateLevelTree()
        {
        }

        private void AddLevelButton_Clicked(object sender, EventArgs e)
        {
            AddLevelWindow dialog = new AddLevelWindow(this);
            dialog.Show();
        }

        private void ExportButton_Clicked(object sender, EventArgs e)
        {
            WriteCurrentLevel();
        }

        private void LoadLevel(string levelName)
        {
            LevelDesc = LevelLoader.GetTestLevel();
            LevelView.Refresh(LevelDesc);
        }

        private void WriteCurrentLevel()
        {
            string levelAsJSon = JsonConvert.SerializeObject(LevelDesc);
            //StreamWriter writer = new StreamWriter();
        }

        static void delete_event(object obj, DeleteEventArgs args)
        {
            Application.Quit();
        }

        static void exit_event(object obj, EventArgs args)
        {
            Application.Quit();
        }

    }
}
