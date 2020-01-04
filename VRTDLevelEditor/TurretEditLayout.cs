using System;
using VRTD.Gameplay;
using System.Timers;
using Gtk;
using System.Collections.Generic;

namespace VRTD.LevelEditor
{
    public class TurretEditLayout : ScrolledWindow
    {
        public delegate void TreeRefreshNeededFunc();
        public event TreeRefreshNeededFunc TreeRefreshNeeded;
        public Turret Turret { get; set; }
        public List<Turret> Turrets;
        List<string> ProjectileNames;
        VBox Layout = null;

        public TurretEditLayout() : base(null, null)
        {
        }


        public void SetTurret(string selectedStr)
        {
            GtkHelpers.FlushAllDeferredEvents();

            Destroyed += TurretEditLayout_Destroyed;

            if (null != Layout)
            {
                Layout.Hide();
                Layout.Destroy();
                Layout = null;
            }

            Turrets = LevelManager.GetTurrets();
            Turret = null;
            for (int i = 0; i < Turrets.Count; i++)
            {
                if (Turrets[i].Name == selectedStr)
                {
                    Turret = Turrets[i];
                }
            }

            if (Turret == null)
            {
                throw new Exception("Could not find turret " + selectedStr);
            }

            Layout = new VBox(false, 0);
            AddWithViewport(Layout);

            HBox field = GtkHelpers.TextEntryField("Turret Name", Turret.Name, Name_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.TextEntryField("Asset", Turret.Asset, Asset_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            field = GtkHelpers.TextEntryField("Fire Rate", Turret.FireRate.ToString(), FireRate_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            field = GtkHelpers.TextEntryField("Range", Turret.Range.ToString(), Range_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            List<Projectile> projectiles = LevelManager.GetProjectiles();
            ProjectileNames = new List<string>();
            int currIndex = 0;
            for (int i = 0; i < projectiles.Count; i++)
            {
                ProjectileNames.Add(projectiles[i].Name);
                if (projectiles[i].Name == Turret.Projectile)
                {
                    currIndex = i;
                }
            }

            field = GtkHelpers.ComboBox("Projectile", ProjectileNames.ToArray(), currIndex, Projectile_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            Show();
            ShowAll();
        }

        private void TurretEditLayout_Destroyed(object sender, EventArgs e)
        {
            GtkHelpers.FlushAllDeferredEvents();
        }

        private void Name_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if ((newName != Turret.Name) && (newName.Length > 0))
            {
                Turret.Name = newName;
                WriteChanges();


                if (null != TreeRefreshNeeded)
                {
                    TreeRefreshNeeded();
                }
            }
        }

        private void Asset_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if ((newName != Turret.Asset) && (newName.Length > 0))
            {
                Turret.Asset = newName;
                WriteChanges();
            }
        }

        private void FireRate_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    float newVal = float.Parse(newName);
                    Turret.FireRate = newVal;
                    WriteChanges();
                }
                catch(Exception ex)
                {
                }
            }
        }


        private void Range_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    float newVal = float.Parse(newName);
                    Turret.Range = newVal;
                    WriteChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }


        private void Projectile_Changed(object sender, EventArgs e)
        {
            if (null == sender)
            {
                return;
            }
            int newIndex = ((ComboBox)sender).Active;
            string newValue = ProjectileNames[newIndex];
            if (Turret.Projectile != newValue)
            {
                Turret.Projectile = newValue;
                WriteChanges();
            }
        }

        void WriteChanges()
        {
            LevelManager.WriteTurrets(Turrets);
        }
    }
}



