using System;
using VRTD.Gameplay;
using System.Timers;
using Gtk;
using System.Collections.Generic;

namespace VRTD.LevelEditor
{

    public class WaveStats
    {
        public float MaxDPSDealtSingleEnemy;
        public float MaxHPPSProducted;
        public float MaxDPSOverall;
    }

    public class TurretStats
    {
        public Turret t;
        public TurretInstance Instance;
        public float MaxDPSOverall;
    }

    public class LevelAnalysisLayout : ScrolledWindow
    {
        public LevelDescription LevelDesc { get; set; }
        Dictionary<int, int> TurretSelections;
        VBox Layout = null;
        public delegate void TreeRefreshNeededFunc();
        public event TreeRefreshNeededFunc TreeRefreshNeeded;
        Table MapTable;
        Dictionary<Button, int> MapMappings;
        TreeView WavesTree;
        ListStore WavesModel;
        TreeView AllowedTurretTree;
        ListStore AllowedTurretModel;
        List<TurretStats> TurretStatList;
        List<WaveStats> WaveStatList;

        public LevelAnalysisLayout() : base(null, null)
        {
        }


        public void SetLevel(LevelDescription desc)
        {
            GtkHelpers.FlushAllDeferredEvents();

            Destroyed += LevelAnalysisLayout_Destroyed;

            if (null != Layout)
            {
                Layout.Hide();
                Layout.Destroy();
                Layout = null;
            }

            LevelDesc = desc;

            TurretSelections = new Dictionary<int, int>();
            RecalculateAllStats();

            Layout = new VBox(false, 0);
            AddWithViewport(Layout);

            HBox field = GtkHelpers.ReadOnlyTextField("Level Name", desc.Name);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.ReadOnlyTextField("Lives", desc.Lives.ToString());
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.ReadOnlyTextField("Starting Coins", desc.StartingCoins.ToString());
            Layout.PackStart(field, false, false, 0);
            field.Show();

            WavesTree = new TreeView();
            Layout.PackStart(WavesTree, false, false, 0);
            WavesTree.Show();

            List<EnemyDescription> enemies = LevelManager.GetEnemies();
            ListStore comboModel = new ListStore(typeof(string));
            ComboBox comboBox = new ComboBox(comboModel);
            foreach (EnemyDescription enemy in enemies)
            {
                comboBox.AppendText(enemy.Name);
            }
            comboBox.Active = 0;


            TreeViewColumn enemyCoumn = new TreeViewColumn();
            TreeViewColumn countColumn = new TreeViewColumn();
            TreeViewColumn difficultyColumn = new TreeViewColumn();
            TreeViewColumn singleEnemyColumn = new TreeViewColumn();
            TreeViewColumn maxHPPSColumn = new TreeViewColumn();
            TreeViewColumn maxDPSColumn = new TreeViewColumn();

            CellRendererCombo textCellRenderer = new CellRendererCombo();
            textCellRenderer.Editable = false;

            enemyCoumn.PackStart(textCellRenderer, true);
            enemyCoumn.Title = "Enemy";
            enemyCoumn.AddAttribute(textCellRenderer, "text", 1);
            WavesTree.AppendColumn(enemyCoumn);

            countColumn.PackStart(textCellRenderer, true);
            countColumn.Title = "Count";
            countColumn.AddAttribute(textCellRenderer, "text", 2);
            WavesTree.AppendColumn(countColumn);

            difficultyColumn.PackStart(textCellRenderer, true);
            difficultyColumn.Title = "Difficulty Multiplier";
            difficultyColumn.AddAttribute(textCellRenderer, "text", 3);
            WavesTree.AppendColumn(difficultyColumn);

            //
            // Add column:  Max damage that can be dished to a single enemy given turret coverage + enemy speed
            //

            singleEnemyColumn.PackStart(textCellRenderer, true);
            singleEnemyColumn.Title = "Single Enemy DPS";
            singleEnemyColumn.AddAttribute(textCellRenderer, "text", 4);
            WavesTree.AppendColumn(singleEnemyColumn);

            //
            // Add column: Max hit points of the wave
            //

            maxHPPSColumn.PackStart(textCellRenderer, true);
            maxHPPSColumn.Title = "Max Hitpoints/s";
            maxHPPSColumn.AddAttribute(textCellRenderer, "text", 5);
            WavesTree.AppendColumn(maxHPPSColumn);

            //
            // Add column: Max damage of all the turrets combined firing at full capacity
            //

            maxDPSColumn.PackStart(textCellRenderer, true);
            maxDPSColumn.Title = "Max DPS overall";
            maxDPSColumn.AddAttribute(textCellRenderer, "text", 6);
            WavesTree.AppendColumn(maxDPSColumn);

            //
            // Add column: Given all the factors, theoretical headroom of DPS
            //

            WavesModel = new ListStore(typeof(int), typeof(string), typeof(int), typeof(float), typeof(float), typeof(float), typeof(float));
            WavesTree.Model = WavesModel;
            WavesTree.Selection.Mode = SelectionMode.Single;

            PopulateTreeWithWaves(desc);

            field = new HBox(false, 5);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            Button b = new Button("+");
            b.Clicked += NewWave_Clicked;
            b.Show();
            field.PackStart(b, false, false, 0);

            b = new Button("-");
            b.Clicked += RemoveWave_Clicked;
            b.Show();
            field.PackStart(b, false, false, 0);


            Table map = GetFieldTable(desc);
            Layout.PackStart(map, false, true, 0);
            map.Show();


            //
            // Allowed turrets
            //

            field = new HBox(true, 10);
            Layout.PackStart(field, true, false, 0);
            field.Show();

            AllowedTurretTree = new TreeView();

            TreeViewColumn availCol = new TreeViewColumn();
            TreeViewColumn allowedCol = new TreeViewColumn();


            allowedCol.PackStart(textCellRenderer, true);
            allowedCol.Title = "Allowed Turrets";
            allowedCol.AddAttribute(textCellRenderer, "text", 1);
            AllowedTurretTree.AppendColumn(allowedCol);

            AllowedTurretModel = new ListStore(typeof(int), typeof(string));
            AllowedTurretTree.Model = AllowedTurretModel;
            AllowedTurretTree.Selection.Mode = SelectionMode.Multiple;

            field.PackEnd(AllowedTurretTree, true, true, 0);
            AllowedTurretTree.Show();

            PopulateTurretTrees(desc);


            Show();
            ShowAll();
        }


        private void PopulateTurretTrees(LevelDescription desc)
        {
            AllowedTurretModel.Clear();

            List<Turret> AvailTurrets = LevelManager.GetTurrets();

            for (int i = 0; i < desc.AllowedTurrets.Count; i++)
            {
                object[] values = { i, desc.AllowedTurrets[i] };
                AllowedTurretModel.AppendValues(values);
            }
        }

        private void RemoveWave_Clicked(object sender, EventArgs e)
        {
            TreeIter selected;
            if (WavesTree.Selection.GetSelected(out selected))
            {
                int row = (int)WavesModel.GetValue(selected, 0);


                MessageDialog md = new MessageDialog(null,
                DialogFlags.Modal, MessageType.Warning,
                ButtonsType.OkCancel, "Are you sure you want to delete wave #" + (row + 1) + "?");
                int result = md.Run();
                md.Destroy();

                if (result == -5)
                {
                    LevelDesc.Waves.RemoveAt(row);
                    WavesModel.Remove(ref selected);

                    WriteChanges();
                }
            }
        }

        private void NewWave_Clicked(object sender, EventArgs e)
        {
            EnemyWave wave = new EnemyWave();
            wave.Enemy = LevelManager.GetEnemies()[0].Name;
            wave.Count = 10;
            wave.DifficultyMultiplier = 1.0F;
            LevelDesc.Waves.Add(wave);

            WriteChanges();

            PopulateTreeWithWaves(LevelDesc);
        }

        private void ComboCellRenderer_Edited(object o, EditedArgs args)
        {
            TreeIter iter;
            if (WavesModel.GetIterFromString(out iter, args.Path))
            {
                int row = (int)WavesModel.GetValue(iter, 0);
                if (args.NewText != LevelDesc.Waves[row].Enemy)
                {
                    WavesModel.SetValue(iter, 1, args.NewText);
                    LevelDesc.Waves[row].Enemy = args.NewText;
                    WriteChanges();
                }
            }
        }

        private void CountCell_Edited(object o, EditedArgs args)
        {
            TreeIter iter;
            if (WavesModel.GetIterFromString(out iter, args.Path))
            {
                try
                {
                    int newValue = int.Parse(args.NewText);
                    int row = (int)WavesModel.GetValue(iter, 0);
                    int currentValue = LevelDesc.Waves[row].Count;
                    if (newValue != currentValue)
                    {
                        WavesModel.SetValue(iter, 2, newValue);
                        LevelDesc.Waves[row].Count = newValue;
                        WriteChanges();
                    }
                }
                catch (Exception ex) { }
            }
        }


        private void DifficultyCell_Edited(object o, EditedArgs args)
        {
            TreeIter iter;
            if (WavesModel.GetIterFromString(out iter, args.Path))
            {
                try
                {
                    float newValue = float.Parse(args.NewText);
                    int row = (int)WavesModel.GetValue(iter, 0);
                    float currentValue = LevelDesc.Waves[row].DifficultyMultiplier;
                    if (newValue != currentValue)
                    {
                        WavesModel.SetValue(iter, 3, newValue);
                        LevelDesc.Waves[row].DifficultyMultiplier = newValue;
                        WriteChanges();
                    }
                }
                catch (Exception ex) { }
            }
        }

        private void PopulateTreeWithWaves(LevelDescription desc)
        {
            WavesModel.Clear();

            for (int i = 0; i < desc.Waves.Count; i++)
            {
                object[] values = { i, desc.Waves[i].Enemy, desc.Waves[i].Count, desc.Waves[i].DifficultyMultiplier, WaveStatList[i].MaxDPSDealtSingleEnemy, WaveStatList[i].MaxHPPSProducted, WaveStatList[i].MaxDPSOverall };
                WavesModel.AppendValues(values);
            }
        }

        private void LevelAnalysisLayout_Destroyed(object sender, EventArgs e)
        {
            GtkHelpers.FlushAllDeferredEvents();
        }

        private Table GetFieldTable(LevelDescription desc)
        {
            MapTable = new Table((uint)desc.FieldDepth, (uint)desc.FieldWidth, true);
            MapMappings = new Dictionary<Button, int>();

            for (uint i = 0; i < desc.Map.Count; i++)
            {
                Button b = new Button();
                SetFieldButtonType(b, desc.Map[(int)i]);
                SetButtonOnTable(b, desc, (int)i);
            }

            return MapTable;
        }

        private void Map_Clicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            int index = MapMappings[b];
            string turretString = "None";

            //
            // Only the 3 main states
            // 
            if (LevelDesc.Map[index] == 'T')
            {
                int turretIndex = 0;

                if (!TurretSelections.TryGetValue(index, out turretIndex))
                {
                    if (LevelDesc.AllowedTurrets.Count > 0)
                    {
                        turretIndex = 0;
                        TurretSelections[index] = turretIndex;
                        turretString = LevelDesc.AllowedTurrets[turretIndex];
                    }
                }
                else
                {
                    turretIndex = (turretIndex + 1) % LevelDesc.AllowedTurrets.Count;
                    TurretSelections[index] = turretIndex;
                    turretString = LevelDesc.AllowedTurrets[turretIndex];

                }

                RecalculateAllStats();
                PopulateTreeWithWaves(LevelDesc);

                // Update location on map
                MapTable.Remove(b);
                b = new Button();
                UpdateFieldTurretButton(b, turretString);
                SetButtonOnTable(b, LevelDesc, index);
            }
        }


        private void UpdateFieldTurretButton(Button b, string name)
        {
            Gdk.Color col = new Gdk.Color();
            col = GtkHelpers.Color("grey");
            b.ModifyBg(StateType.Normal, col);
            b.Label = name;
        }

        private void SetFieldButtonType(Button b, char c)
        {
            string s = "";
            Gdk.Color col = new Gdk.Color();
            switch (c)
            {
                case 'D':
                    col = GtkHelpers.Color("green");
                    s = "D";
                    break;
                case 'R':
                    col = GtkHelpers.Color("black");
                    s = "R";
                    break;
                case 'T':
                    col = GtkHelpers.Color("grey");
                    s = "None";
                    break;
                case 'E':
                    col = GtkHelpers.Color("red");
                    s = "E";
                    break;
                case 'X':
                    col = GtkHelpers.Color("blue");
                    s = "X";
                    break;
            }
            b.ModifyBg(StateType.Normal, col);
            b.Label = s;
        }

        private void SetButtonOnTable(Button b, LevelDescription desc, int index)
        {
            uint xpos = (uint)(index % desc.FieldWidth);
            uint ypos = (uint)(index / desc.FieldWidth);
            MapMappings.Add(b, index);
            MapTable.Attach(b, xpos, xpos + 1, ypos, ypos + 1);
            b.Clicked += Map_Clicked;
            b.Show();

        }

        private void CalculateStatsForTurrets()
        {
            TurretStatList = new List<TurretStats>();

            for (int i = 0; i < LevelDesc.Map.Count; i++)
            {
                int turretIndex = 0;
                if (TurretSelections.TryGetValue(i, out turretIndex))
                {
                    Turret t = LevelManager.LookupTurret(LevelDesc.AllowedTurrets[turretIndex]);

                    TurretStats stats = new TurretStats();
                    stats.t = t;
                    stats.Instance = new TurretInstance(t, new MapPos(i % LevelDesc.FieldWidth, i / LevelDesc.FieldWidth), LevelDesc, null);
                    stats.MaxDPSOverall = TurretEditLayout.CalculateDamagePerShot(t);

                    TurretStatList.Add(stats);
                }
            }
        }


        private void CalculateStatsForWaves(List<TurretStats> Turrets)
        {
            WaveStatList = new List<WaveStats>();

            float maxDPSOverall = 0.0F;
            for (int j = 0; j < Turrets.Count; j++)
            {
                maxDPSOverall += Turrets[j].MaxDPSOverall;
            }

            for (int i=0; i<LevelDesc.Waves.Count; i++)
            {
                EnemyWave wave = LevelDesc.Waves[i];

                WaveStats stats = new WaveStats();

                stats.MaxDPSDealtSingleEnemy = 0.0F;
                stats.MaxDPSOverall = maxDPSOverall;
                stats.MaxHPPSProducted = EnemyEditLayout.CalculateDPSforWave(LevelManager.LookupEnemy(wave.Enemy));


                WaveStatList.Add(stats);
            }
        }

        private void RecalculateAllStats()
        {
            CalculateStatsForTurrets();
            CalculateStatsForWaves(TurretStatList);
        }


        void WriteChanges()
        {
            LevelManager.WriteLevel(LevelDesc.Name, LevelDesc);
        }

    }
}



