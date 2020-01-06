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
        Widget EditorWidget = null;
        ListStore ListModel;
        HBox TopLevelHBox;
        public enum EditorMode { Level, Turret, Enemy, Projectile }
        EditorMode CurrentMode;

        public MainWindow() :
                base(WindowType.Toplevel)
        {
            SetDefaultSize(800, 600);
            SetPosition(WindowPosition.Center);

            if (!LevelManager.Initialize(this))
            {
                MessageDialog md = new MessageDialog(this,
                DialogFlags.Modal, MessageType.Error,
                ButtonsType.Ok, "Couldn't find levels folder - exiting");
                md.Run();
                md.Destroy();
                Gtk.Application.Quit();
            }

            AddWidgetsAndShow(EditorMode.Level);
        }

        private void AddWidgetsAndShow(EditorMode editorMode)
        {
            /* Set a handler for delete_event that immediately
             * exits GTK. */
            DeleteEvent += delete_event;

            if (null != TopLevelHBox)
            {
                TopLevelHBox.HideAll();
                TopLevelHBox.Destroy();
                TopLevelHBox = null;
            }

            CurrentMode = editorMode;


            EditorWidget = null;

            switch (editorMode)
            {
                case EditorMode.Level:
                    EditorWidget = new LevelEditLayout();
                    ((LevelEditLayout)EditorWidget).TreeRefreshNeeded += TreeRefreshNeeded_Event;
                    break;
                case EditorMode.Turret:
                    EditorWidget = new TurretEditLayout();
                    ((TurretEditLayout)EditorWidget).TreeRefreshNeeded += TreeRefreshNeeded_Event;
                    break;
                case EditorMode.Enemy:
                    EditorWidget = new EnemyEditLayout();
                    ((EnemyEditLayout)EditorWidget).TreeRefreshNeeded += TreeRefreshNeeded_Event;
                    break;
                case EditorMode.Projectile:
                    EditorWidget = new ProjectileEditLayout();
                    ((ProjectileEditLayout)EditorWidget).TreeRefreshNeeded += TreeRefreshNeeded_Event;
                    break;
            }

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

            Button addLevelButton = new Button("+");
            addLevelButton.Clicked += AddButton_Clicked;
            vbox.PackStart(addLevelButton, false, false, 5);
            addLevelButton.Show();

            Button removeLevelButton = new Button("-");
            removeLevelButton.Clicked += RemoveButton_Clicked;
            vbox.PackStart(removeLevelButton, false, false, 5);
            removeLevelButton.Show();

            tree = new TreeView();
            vbox.PackStart(tree, true, true, 0);

            // Create a column for the artist name
            TreeViewColumn levelColumn = new TreeViewColumn();
            CellRendererText levelCellRenderer = new CellRendererText();
            levelColumn.PackStart(levelCellRenderer, true);
            levelColumn.AddAttribute(levelCellRenderer, "text", 0);
            tree.AppendColumn(levelColumn);
            ListModel = new ListStore(typeof(string));
            tree.Model = ListModel;
            tree.Selection.Mode = SelectionMode.Single;
            tree.Selection.Changed += TreeSelection_Changed;

            switch (editorMode)
            {
                case EditorMode.Level:
                    levelColumn.Title = "Levels";
                    PopulateTreeWithLevels();
                    break;
                case EditorMode.Turret:
                    levelColumn.Title = "Turrets";
                    PopulateTreeWithTurrets();
                    break;
                case EditorMode.Enemy:
                    levelColumn.Title = "Enemies";
                    PopulateTreeWithEnemies();
                    break;
                case EditorMode.Projectile:
                    levelColumn.Title = "Projectiles";
                    PopulateTreeWithProjectilees();
                    break;
            }

            tree.Show();


            //
            // Righthand column
            //

            vbox = new VBox(false, 0);
            TopLevelHBox.PackStart(vbox, true, true, 5);
            vbox.Show();

            HBox modeHbox = new HBox(true, 0);
            vbox.PackStart(modeHbox, false, true, 0);
            modeHbox.Show();

            Button modeButton = new Button("Level");
            modeButton.Show();
            modeButton.Clicked += level_clicked_event;
            modeHbox.PackStart(modeButton, false, false, 5);

            modeButton = new Button("Turret");
            modeButton.Show();
            modeButton.Clicked += turret_clicked_event;
            modeHbox.PackStart(modeButton, false, false, 5);

            modeButton = new Button("Enemy");
            modeButton.Show();
            modeButton.Clicked += enemy_clicked_event;
            modeHbox.PackStart(modeButton, false, false, 5);

            modeButton = new Button("Projectile");
            modeButton.Show();
            modeButton.Clicked += projectile_clicked_event;
            modeHbox.PackStart(modeButton, false, false, 5);


            vbox.PackStart(EditorWidget, true, true, 5);
            EditorWidget.Show();


            ShowAll();

            LevelManager.WriteAssetDirectory();
        }

        private void TreeRefreshNeeded_Event()
        {
            switch (CurrentMode)
            {
                case EditorMode.Level:
                    PopulateTreeWithLevels();
                    break;
                case EditorMode.Turret:
                    PopulateTreeWithTurrets();
                    break;
                case EditorMode.Enemy:
                    PopulateTreeWithEnemies();
                    break;
                case EditorMode.Projectile:
                    PopulateTreeWithProjectilees();
                    break;
            }
        }

        private void TreeSelection_Changed(object sender, EventArgs e)
        {
            TreeIter selected;
            if (tree.Selection.GetSelected(out selected))
            {
                string selectedStr = (string)ListModel.GetValue(selected, 0);

                switch (CurrentMode)
                {
                    case EditorMode.Level:
                        LoadLevel(selectedStr);
                        break;
                    case EditorMode.Turret:
                        ((TurretEditLayout)EditorWidget).SetTurret(selectedStr);
                        break;
                    case EditorMode.Enemy:
                        ((EnemyEditLayout)EditorWidget).SetEnemy(selectedStr);
                        break;
                    case EditorMode.Projectile:
                        ((ProjectileEditLayout)EditorWidget).SetProjectile(selectedStr);
                        break;
                }
            }
        }

        private void PopulateTreeWithLevels()
        {

            string previousValue = "";
            bool foundPrevious = false;
            TreeIter selected;
            if (tree.Selection.GetSelected(out selected))
            {
                previousValue = (string)ListModel.GetValue(selected, 0);
            }
            

            List<string> levels = LevelManager.GetLevelList();

            ListModel.Clear();

            for (int i = 0; i < levels.Count; i++)
            {
                TreeIter itr = ListModel.AppendValues(levels[i]);
                if (previousValue == levels[i])
                {
                    foundPrevious = true;
                    selected = itr;
                }
            }


            if (!foundPrevious)
            {
                // If not, here's the default
                ListModel.GetIterFirst(out selected);
            }

            tree.Selection.SelectIter(selected);
        }

        private void PopulateTreeWithTurrets()
        {
            string previousValue = "";
            bool foundPrevious = false;
            TreeIter selected;
            if (tree.Selection.GetSelected(out selected))
            {
                previousValue = (string)ListModel.GetValue(selected, 0);
            }

            List<Turret> items = LevelManager.GetTurrets();

            ListModel.Clear();

            if (null != items)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    TreeIter itr = ListModel.AppendValues(items[i].Name);
                    if (previousValue == items[i].Name)
                    {
                        foundPrevious = true;
                        selected = itr;
                    }
                }
            }

            if (!foundPrevious)
            {
                // If not, here's the default
                ListModel.GetIterFirst(out selected);
            }

            tree.Selection.SelectIter(selected);
        }

        private void PopulateTreeWithEnemies()
        {
            string previousValue = "";
            bool foundPrevious = false;
            TreeIter selected;
            if (tree.Selection.GetSelected(out selected))
            {
                previousValue = (string)ListModel.GetValue(selected, 0);
            }

            List<EnemyDescription> items = LevelManager.GetEnemies();

            ListModel.Clear();

            if (null != items)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    ListModel.AppendValues(items[i].Name);

                    TreeIter itr = ListModel.AppendValues(items[i].Name);
                    if (previousValue == items[i].Name)
                    {
                        foundPrevious = true;
                        selected = itr;
                    }
                }
            }


            if (!foundPrevious)
            {
                // If not, here's the default
                ListModel.GetIterFirst(out selected);
            }

            tree.Selection.SelectIter(selected);
        }

        private void PopulateTreeWithProjectilees()
        {
            string previousValue = "";
            bool foundPrevious = false;
            TreeIter selected;
            if (tree.Selection.GetSelected(out selected))
            {
                previousValue = (string)ListModel.GetValue(selected, 0);
            }

            List<Projectile> items = LevelManager.GetProjectiles();

            ListModel.Clear();

            if (null != items)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    ListModel.AppendValues(items[i].Name);
                    TreeIter itr = ListModel.AppendValues(items[i].Name);
                    if (previousValue == items[i].Name)
                    {
                        foundPrevious = true;
                        selected = itr;
                    }
                }
            }

            if (!foundPrevious)
            {
                // If not, here's the default
                ListModel.GetIterFirst(out selected);
            }

            tree.Selection.SelectIter(selected);
        }

        private void AddButton_Clicked(object sender, EventArgs e)
        {

            switch(CurrentMode)
            {
                case EditorMode.Level:
                    AddLevelWindow AddLevel = new AddLevelWindow(this);
                    AddLevel.Finished += AddLevel_Finished;
                    AddLevel.Show();
                    break;
                case EditorMode.Turret:
                    ((TurretEditLayout)EditorWidget).AddTurret();
                    break;
                case EditorMode.Enemy:
                    ((EnemyEditLayout)EditorWidget).AddEnemy();
                    break;
                case EditorMode.Projectile:
                    ((ProjectileEditLayout)EditorWidget).AddProjectile();
                    break;
            };
        }

        private void RemoveButton_Clicked(object sender, EventArgs e)
        {
            TreePath[] treePath = tree.Selection.GetSelectedRows();
            string itemName = "";
            TreeIter iter;

            if (treePath.Length == 0)
            {
                return;
            }

            ListModel.GetIter(out iter, treePath[0]);

            itemName = (string)ListModel.GetValue(iter, 0);

            MessageDialog md = new MessageDialog(null,
            DialogFlags.Modal, MessageType.Warning,
            ButtonsType.OkCancel, "You will perminantly delete \"" + itemName  + "\". Continue?");
            int result = md.Run();
            md.Destroy();

            if (result != -5)
            {
                return;
            }


            switch (CurrentMode)
            {
                case EditorMode.Level:
                    LevelManager.DeleteLevel(itemName);
                    ListModel.Remove(ref iter);
                    break;
                case EditorMode.Turret:
                    ((TurretEditLayout)EditorWidget).RemoveTurret(itemName);
                    break;
                case EditorMode.Enemy:
                    ((EnemyEditLayout)EditorWidget).RemoveEnemy(itemName);
                    break;
                case EditorMode.Projectile:
                    ((ProjectileEditLayout)EditorWidget).RemoveProjectile(itemName);
                    break;
            };
        }

        private void AddLevel_Finished(AddLevelEventArgs a)
        {
            LevelDescription template = LevelLoader.GetTestLevel();
            template.Name = a.Name;
            LevelManager.WriteLevel(a.Name, template);
            ListModel.AppendValues(a.Name);
        }


        private void LoadLevel(string levelName)
        {

            LevelDesc = LevelManager.ReadLevel(levelName);

            /*
             *
             * Use this code to prepopulate Enemies/Turrets/Projectiles
             * 

            
            LevelDesc = LevelLoader.GetTestLevel();

            LevelManager.WriteLevel("1-1", LevelDesc);

            LevelManager.WriteTurrets(LevelDesc.AllowedTurrets);

            List<EnemyDescription> enemies = new List<EnemyDescription>();
            enemies.Add(LevelDesc.Waves[0].EnemyType);
            enemies.Add(LevelDesc.Waves[1].EnemyType);
            LevelManager.WriteEnemies(enemies);

            List<Projectile> projectiles = new List<Projectile>();
            projectiles.Add(LevelDesc.AllowedTurrets[0].ProjectileType);
            projectiles.Add(LevelDesc.AllowedTurrets[1].ProjectileType);
            projectiles.Add(LevelDesc.AllowedTurrets[2].ProjectileType);
            LevelManager.WriteProjectiles(projectiles);

            Application.Quit();
            */

            ((LevelEditLayout)EditorWidget).SetLevel(LevelDesc);
        }

        static void delete_event(object obj, DeleteEventArgs args)
        {
            Gtk.Application.Quit();
        }

        void level_clicked_event(object obj, EventArgs args)
        {
            AddWidgetsAndShow(EditorMode.Level);
        }

        void turret_clicked_event(object obj, EventArgs args)
        {
            AddWidgetsAndShow(EditorMode.Turret);
        }

        void enemy_clicked_event(object obj, EventArgs args)
        {
            AddWidgetsAndShow(EditorMode.Enemy);
        }

        void projectile_clicked_event(object obj, EventArgs args)
        {
            AddWidgetsAndShow(EditorMode.Projectile);
        }
    }
}
