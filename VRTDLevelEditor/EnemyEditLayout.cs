using System;
using VRTD.Gameplay;
using System.Timers;
using Gtk;
using System.Collections.Generic;

namespace VRTD.LevelEditor
{
    public class EnemyEditLayout : ScrolledWindow
    {
        public delegate void TreeRefreshNeededFunc();
        public event TreeRefreshNeededFunc TreeRefreshNeeded;
        public EnemyDescription Enemy { get; set; }
        public List<EnemyDescription> Enemies;
        List<string> ProjectileNames;
        VBox Layout = null;

        public EnemyEditLayout() : base(null, null)
        {
        }


        public void SetEnemy(string selectedStr)
        {
            GtkHelpers.FlushAllDeferredEvents();

            Destroyed += EnemyEditLayout_Destroyed;

            if (null != Layout)
            {
                Layout.Hide();
                Layout.Destroy();
                Layout = null;
            }

            Enemies = LevelManager.GetEnemies();
            Enemy = null;
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i].Name == selectedStr)
                {
                    Enemy = Enemies[i];
                }
            }

            if (Enemy == null)
            {
                throw new Exception("Could not find Enemy " + selectedStr);
            }

            Layout = new VBox(false, 0);
            AddWithViewport(Layout);

            HBox field = GtkHelpers.TextEntryField("Enemy Name", Enemy.Name, Name_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.TextEntryField("Asset", Enemy.Asset, Asset_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            field = GtkHelpers.TextEntryField("Hit Points", Enemy.HitPoints.ToString(), HitPoints_Changed, true, GtkHelpers.ValueType.Int);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            field = GtkHelpers.TextEntryField("Movement Speed", Enemy.MovementSpeed.ToString(), MovementSpeed_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            field = GtkHelpers.TextEntryField("Spawn Rate", Enemy.SpawnRate.ToString(), SpawnRate_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.TextEntryField("Coins", Enemy.Coins.ToString(), Coins_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            Show();
            ShowAll();
        }

        private void EnemyEditLayout_Destroyed(object sender, EventArgs e)
        {
            GtkHelpers.FlushAllDeferredEvents();
        }

        private void Name_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if ((newName != Enemy.Name) && (newName.Length > 0))
            {
                Enemy.Name = newName;
                WriteChanges();


                TreeRefreshNeeded?.Invoke();
            }
        }

        private void Asset_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if ((newName != Enemy.Asset) && (newName.Length > 0))
            {
                Enemy.Asset = newName;
                WriteChanges();
            }
        }

        private void HitPoints_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    int newVal = int.Parse(newName);
                    Enemy.HitPoints = newVal;
                    WriteChanges();
                }
                catch(Exception ex)
                {
                }
            }
        }


        private void MovementSpeed_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    float newVal = float.Parse(newName);
                    Enemy.MovementSpeed = newVal;
                    WriteChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }


        private void SpawnRate_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    float newVal = float.Parse(newName);
                    Enemy.SpawnRate = newVal;
                    WriteChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }


        private void Coins_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    int newVal = int.Parse(newName);
                    Enemy.Coins = newVal;
                    WriteChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void AddEnemy()
        {

            EnemyDescription enemy = new EnemyDescription();
            enemy.Name = "New Enemy";
            enemy.Asset = "";
            enemy.HitPoints = 1;
            enemy.MovementSpeed = 1.0F;
            enemy.SpawnRate = 1.0F;
            Enemies.Add(enemy);

            WriteChanges();

            TreeRefreshNeeded?.Invoke();
        }

        public void RemoveEnemy(string name)
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i].Name == name)
                {
                    Enemies.Remove(Enemies[i]);
                    WriteChanges();
                    TreeRefreshNeeded?.Invoke();
                    break;
                }
            }
        }

        void WriteChanges()
        {
            LevelManager.WriteEnemies(Enemies);
        }
    }
}



