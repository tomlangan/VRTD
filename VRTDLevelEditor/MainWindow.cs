﻿using System;
using Gtk;
using VRTD.Gameplay;
using System.Collections.Generic;

namespace VRTD.LevelEditor
{
    public class LevelDescView : Layout
    {
        public LevelDescription LevelDesc { get; set; }
        Label NameLabel;

        public LevelDescView() : base(null, null)
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
        ListStore LevelListStore;

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


            table = new Table(25, 5, true);

            /* Put the table in the main window */
            Add(table);

            /* Create first button */
            tree = new TreeView();

            table.Attach(tree, 0, 1, 0, 24);

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

        private void ExportButton_Clicked(object sender, EventArgs e)
        {
            
        }

        private void LoadLevel(string levelName)
        {
            LevelDesc = LevelLoader.GetTestLevel();
            LevelView.Refresh(LevelDesc);
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
