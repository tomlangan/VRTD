using System;
using Gtk;
using VRTD.Gameplay;
using System.Collections.Generic;

namespace VRTD.LevelEditor
{
    public partial class MainWindow : Window
    {
        TreeView tree;
        LevelDescription LevelDesc;
        LevelEditLayout LevelView;
        ListStore LevelListStore;
        HBox TopLevelHBox;

        public MainWindow() :
                base(WindowType.Toplevel)
        {
            SetDefaultSize(800, 600);
            SetPosition(WindowPosition.Center);

            AddWidgetsAndShow();

            if (!LevelManager.Initialize(this))
            {
                MessageDialog md = new MessageDialog(this,
                DialogFlags.Modal, MessageType.Error,
                ButtonsType.Ok, "Couldn't find levels folder - exiting");
                md.Run();
                md.Destroy();
                Application.Quit();
            }

            PopulateLevelTree();
        }

        private void AddWidgetsAndShow()
        {
            /* Set a handler for delete_event that immediately
             * exits GTK. */
            DeleteEvent += delete_event;

            TopLevelHBox = new HBox(false, 0);
            Add(TopLevelHBox);
            TopLevelHBox.Show();

            //
            // Lefthand column
            //

            VBox vbox = new VBox(false, 0);
            TopLevelHBox.PackStart(vbox, false, true, 5);
            vbox.SetSizeRequest(100, -1);
            vbox.Show();

            tree = new TreeView();
            vbox.PackStart(tree, true, true, 0);

            // Create a column for the artist name
            TreeViewColumn levelColumn = new TreeViewColumn();
            levelColumn.Title = "Levels";
            CellRendererText levelCellRenderer = new CellRendererText();
            levelColumn.PackStart(levelCellRenderer, true);
            levelColumn.AddAttribute(levelCellRenderer, "text", 0);
            tree.AppendColumn(levelColumn);
            LevelListStore = new ListStore(typeof(string));
            tree.Model = LevelListStore;
            tree.Selection.Mode = SelectionMode.Single;
            tree.Selection.Changed += LevelTreeSelection_Changed;
            tree.Show();

            Button addLevelButton = new Button("+");

            addLevelButton.Clicked += AddLevelButton_Clicked;
            vbox.PackEnd(addLevelButton, false, false, 5);
            addLevelButton.Show();

            //
            // Righthand column
            //

            vbox = new VBox(false, 0);
            TopLevelHBox.PackStart(vbox, true, true, 5);
            vbox.Show();


            LevelView = new LevelEditLayout();
            vbox.PackStart(LevelView, true, true, 5);

            LevelView.TreeRefreshNeeded += LevelView_TreeRefreshNeeded;
            LevelView.Show();


            Button quitbutton = new Button("Quit");
            quitbutton.Show();
            quitbutton.Clicked += exit_event;

            vbox.PackStart(quitbutton, false, false, 5);

            ShowAll();
        }

        private void LevelView_TreeRefreshNeeded()
        {
            PopulateLevelTree();
        }

        private void LevelTreeSelection_Changed(object sender, EventArgs e)
        {
            TreeIter selected;
            if (tree.Selection.GetSelected(out selected))
            {
                string levelSelected = (string)LevelListStore.GetValue(selected, 0);

                LoadLevel(levelSelected);
            }
        }

        private void PopulateLevelTree()
        {
            List<string> levels = LevelManager.GetList();

            LevelListStore.Clear();

            for (int i = 0; i < levels.Count; i++)
            {
                LevelListStore.AppendValues(levels[i]);
            }
        }

        private void AddLevelButton_Clicked(object sender, EventArgs e)
        {
            AddLevelWindow AddLevel = new AddLevelWindow(this);
            AddLevel.Finished += AddLevel_Finished;
            AddLevel.Show();
        }

        private void AddLevel_Finished(AddLevelEventArgs a)
        {
            LevelDescription template = LevelLoader.GetTestLevel();
            template.Name = a.Name;
            LevelManager.WriteLevel(a.Name, template);
            LevelListStore.AppendValues(a.Name);
        }


        private void LoadLevel(string levelName)
        {
            LevelDesc = LevelManager.ReadLevel(levelName);
            LevelView.SetLevel(LevelDesc);
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
