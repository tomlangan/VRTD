using System;
using VRTD.Gameplay;
using Gtk;
using System.Collections.Generic;

namespace VRTD.LevelEditor
{
    public class ProjectileEditLayout : ScrolledWindow
    {
        public delegate void TreeRefreshNeededFunc();
        public event TreeRefreshNeededFunc TreeRefreshNeeded;
        public Projectile Projectile { get; set; }
        public List<Projectile> Projectiles;
        List<string> ProjectileNames;
        TreeView EffectsTree;
        ListStore EffectsModel;
        VBox Layout = null;

        public ProjectileEditLayout() : base(null, null)
        {
        }


        public void SetProjectile(string selectedStr)
        {
            GtkHelpers.FlushAllDeferredEvents();

            if (null != Layout)
            {
                Layout.Hide();
                Layout.Destroy();
                Layout = null;
            }

            Projectiles = LevelManager.GetProjectiles();
            Projectile = null;
            for (int i = 0; i < Projectiles.Count; i++)
            {
                if (Projectiles[i].Name == selectedStr)
                {
                    Projectile = Projectiles[i];
                }
            }

            if (Projectile == null)
            {
                throw new Exception("Could not find Projectile " + selectedStr);
            }

            Layout = new VBox(false, 0);
            AddWithViewport(Layout);

            HBox field = GtkHelpers.TextEntryField("Projectile Name", Projectile.Name, Name_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.TextEntryField("Asset", Projectile.Asset, Asset_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();


            field = GtkHelpers.TextEntryField("Hit Points", Projectile.AirSpeed.ToString(), AirSpeed_Changed, true, GtkHelpers.ValueType.Float);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            EffectsTree = new TreeView();
            Layout.PackStart(EffectsTree, false, false, 0);
            EffectsTree.Show();

            ListStore comboModel = new ListStore(typeof(string));
            ComboBox comboBox = new ComboBox(comboModel);
            foreach (string name in Enum.GetNames(typeof(ProjectileEffectType)))
            {
                comboBox.AppendText(name);
            }
            comboBox.Active = 0;

            // Create a column for the artist name
            TreeViewColumn typeCoumn = new TreeViewColumn();
            TreeViewColumn impactColumn = new TreeViewColumn();
            TreeViewColumn durationColumn = new TreeViewColumn();

            CellRendererText durationCellRenderer = new CellRendererText();
            durationCellRenderer.Editable = true;
            durationCellRenderer.Edited += DurationCell_Edited;


            CellRendererText impactCellRenderer = new CellRendererText();
            impactCellRenderer.Editable = true;
            impactCellRenderer.Edited += ImpactCell_Edited;

            CellRendererCombo comboCellRenderer = new CellRendererCombo();
            comboCellRenderer.Editable = true;
            comboCellRenderer.Edited += ComboCellRenderer_Edited;
            comboCellRenderer.Model = comboModel;
            comboCellRenderer.TextColumn = 0;
            comboCellRenderer.HasEntry = false;

            typeCoumn.PackStart(comboCellRenderer, true);
            typeCoumn.Title = "Type";
            typeCoumn.AddAttribute(comboCellRenderer, "text", 1);
            EffectsTree.AppendColumn(typeCoumn);

            durationColumn.PackStart(durationCellRenderer, true);
            durationColumn.Title = "Duration";
            durationColumn.AddAttribute(durationCellRenderer, "text", 2);
            EffectsTree.AppendColumn(durationColumn);

            impactColumn.PackStart(impactCellRenderer, true);
            impactColumn.Title = "Impact";
            impactColumn.AddAttribute(impactCellRenderer, "text", 3);
            EffectsTree.AppendColumn(impactColumn);

            EffectsModel = new ListStore(typeof(int), typeof(string), typeof(float), typeof(float));
            EffectsTree.Model = EffectsModel;
            EffectsTree.Selection.Mode = SelectionMode.Single;

            PopulateTreeWithEffects();

            Show();
            ShowAll();
        }

        private void ComboCellRenderer_Edited(object o, EditedArgs args)
        {
            ProjectileEffectType selectedType = ProjectileEffectType.Damage;
            TreeIter iter;
            if (EffectsModel.GetIterFromString(out iter, args.Path))
            {
                var values = Enum.GetValues(typeof(ProjectileEffectType));
                var names = Enum.GetNames(typeof(ProjectileEffectType));
                for (int i = 0; i < names.Length; i++)
                {
                    if (args.NewText == names[i])
                    {
                        selectedType = (ProjectileEffectType)values.GetValue(i);
                        break;
                    }
                }
                int row = (int)EffectsModel.GetValue(iter, 0);
                ProjectileEffectType currentType = Projectile.Effects[row].EffectType;
                if (selectedType != currentType)
                {
                    EffectsModel.SetValue(iter, 1, args.NewText);
                    Projectile.Effects[row].EffectType = selectedType;
                    WriteChanges();
                }
            }
        }

        private void DurationCell_Edited(object o, EditedArgs args)
        {
            TreeIter iter;
            if (EffectsModel.GetIterFromString(out iter, args.Path))
            {
                try
                {
                    float newValue = float.Parse(args.NewText);
                    int row = (int)EffectsModel.GetValue(iter, 0);
                    float currentValue = Projectile.Effects[row].EffectDuration;
                    if (newValue != currentValue)
                    {
                        EffectsModel.SetValue(iter, 2, newValue);
                        Projectile.Effects[row].EffectDuration = newValue;
                        WriteChanges();
                    }
                }
                catch (Exception ex) { }
            }
        }


        private void ImpactCell_Edited(object o, EditedArgs args)
        {
            TreeIter iter;
            if (EffectsModel.GetIterFromString(out iter, args.Path))
            {
                try
                {
                    float newValue = float.Parse(args.NewText);
                    int row = (int)EffectsModel.GetValue(iter, 0);
                    float currentValue = Projectile.Effects[row].EffectImpact;
                    if (newValue != currentValue)
                    {
                        EffectsModel.SetValue(iter, 3, newValue);
                        Projectile.Effects[row].EffectImpact = newValue;
                        WriteChanges();
                    }
                }
                catch(Exception ex) { }
            }
        }

        private void PopulateTreeWithEffects()
        {
            EffectsModel.Clear();

            for (int i = 0; i < Projectile.Effects.Count; i++)
            {
                string effect = (Projectile.Effects[i].EffectType == ProjectileEffectType.Damage ? "Damage" : "Slow");
                object[] values = { i, effect, Projectile.Effects[i].EffectDuration, Projectile.Effects[i].EffectImpact };
                EffectsModel.AppendValues(values);
            }
        }

        private void Name_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if ((newName != Projectile.Name) && (newName.Length > 0))
            {
                Projectile.Name = newName;
                WriteChanges();


                TreeRefreshNeeded?.Invoke();
            }
        }

        private void Asset_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if ((newName != Projectile.Asset) && (newName.Length > 0))
            {
                Projectile.Asset = newName;
                WriteChanges();
            }
        }

        private void AirSpeed_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if (newName.Length > 0)
            {
                try
                {
                    float newVal = float.Parse(newName);
                    Projectile.AirSpeed = newVal;
                    WriteChanges();
                }
                catch (Exception ex)
                {
                }
            }
        }


        void WriteChanges()
        {
            LevelManager.WriteProjectiles(Projectiles);
        }
    }
}



