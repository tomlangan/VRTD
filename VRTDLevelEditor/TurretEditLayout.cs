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
        HBox DPSfield = null;
        HBox FireRateField = null;

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
                FireRateField = null;
                DPSfield = null;
            }

            Turrets = LevelManager.GetTurrets();
            for (int i = 0; i < Turrets.Count; i++)
            {
                if (Turrets[i].Name == selectedStr)
                {
                    Turret = Turrets[i];
                }
            }

            Layout = new VBox(false, 0);
            AddWithViewport(Layout);

            HBox field = GtkHelpers.TextEntryField("Turret Name", Turret.Name, Name_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.TextEntryField("Asset", Turret.Asset, Asset_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            FireRateField = GtkHelpers.TextEntryField("Fire Rate", Turret.FireRate.ToString(), FireRate_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(FireRateField, false, false, 0);
            FireRateField.Show();


            field = GtkHelpers.TextEntryField("Range", Turret.Range.ToString(), Range_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            field = GtkHelpers.TextEntryField("Cost", Turret.Cost.ToString(), Cost_Changed, true, GtkHelpers.ValueType.Float);
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

            DPSfield = GtkHelpers.TextEntryField("Damage per Sec", Turret.Cost.ToString(), DPS_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(DPSfield, false, false, 0);
            DPSfield.Show();

            RecalculateDPS();

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
                    RecalculateDPS();
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


        private void Cost_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    int newVal = int.Parse(newName);
                    Turret.Cost = newVal;
                    WriteChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void DPS_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    float newDPS = float.Parse(newName);
                    Turret.FireRate = CalculateDamagePerShot(Turret) / newDPS;
                    WriteChanges();
                    ((Entry)FireRateField.Children[1]).Text = Turret.FireRate.ToString();
                }
                catch (Exception ex)
                {
                }
            }
        }

        public static float CalculateDamagePerShot(Turret t)
        {
            Projectile p = LevelManager.LookupProjectile(t.Projectile);
            float damagePerShot = 0.0F;
            for (int i = 0; i < p.Effects.Count; i++)
            {
                if (p.Effects[i].EffectType == ProjectileEffectType.Damage)
                {
                    damagePerShot += p.Effects[i].EffectImpact;
                }
            }
            return damagePerShot;
        }

        private void RecalculateDPS()
        {
            float shotsPerSec = 1.0F / Turret.FireRate;
            float DPS = CalculateDamagePerShot(Turret) * shotsPerSec;

            ((Entry)DPSfield.Children[1]).Text = DPS.ToString();
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
                RecalculateDPS();
            }
        }

        public void AddTurret()
        {
            Turret t = new Turret();
            t.Name = "New Turret";
            t.Asset = "";
            t.Projectile = LevelManager.GetProjectiles()[0].Name;
            t.Range = 1.0F;
            t.FireRate = 1.0F;
            Turrets.Add(t);

            WriteChanges();

            TreeRefreshNeeded?.Invoke();
        }

        public void RemoveTurret(string name)
        {
            for (int i = 0; i < Turrets.Count; i++)
            {
                if (Turrets[i].Name == name)
                {
                    Turrets.Remove(Turrets[i]);
                    WriteChanges();
                    TreeRefreshNeeded?.Invoke();
                    break;
                }
            }

        }

        void WriteChanges()
        {
            LevelManager.WriteTurrets(Turrets);
        }
    }
}



