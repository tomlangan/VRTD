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
        TreeView EffectsTree;
        ListStore EffectsModel;
        VBox Layout = null;

        public ProjectileEditLayout() : base(null, null)
        {
        }


        public void SetProjectile(string selectedStr)
        {
            GtkHelpers.FlushAllDeferredEvents();

            Destroyed += ProjectileEditLayout_Destroyed;

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

            field = GtkHelpers.TextEntryField("Impact Asset", Projectile.ImpactAsset, ImpactAsset_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.TextEntryField("AirSpeed", Projectile.AirSpeed.ToString(), AirSpeed_Changed, true, GtkHelpers.ValueType.Float);
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


            TreeViewColumn typeCoumn = new TreeViewColumn();
            TreeViewColumn impactColumn = new TreeViewColumn();
            TreeViewColumn durationColumn = new TreeViewColumn();
            TreeViewColumn radiusColumn = new TreeViewColumn();

            CellRendererText durationCellRenderer = new CellRendererText();
            durationCellRenderer.Editable = true;
            durationCellRenderer.Edited += DurationCell_Edited;


            CellRendererText impactCellRenderer = new CellRendererText();
            impactCellRenderer.Editable = true;
            impactCellRenderer.Edited += ImpactCell_Edited;

            CellRendererText radiusCellRenderer = new CellRendererText();
            radiusCellRenderer.Editable = true;
            radiusCellRenderer.Edited += RadiusCellRenderer_Edited;

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


            radiusColumn.PackStart(radiusCellRenderer, true);
            radiusColumn.Title = "Radius";
            radiusColumn.AddAttribute(radiusCellRenderer, "text", 4);
            EffectsTree.AppendColumn(radiusColumn);

            EffectsModel = new ListStore(typeof(int), typeof(string), typeof(float), typeof(float), typeof(float));
            EffectsTree.Model = EffectsModel;
            EffectsTree.Selection.Mode = SelectionMode.Single;

            PopulateTreeWithEffects();


            field = new HBox(false, 5);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            Button b = new Button("+");
            b.Clicked += NewEffect_Clicked;
            b.Show();
            field.PackStart(b, false, false, 0);

            b = new Button("-");
            b.Clicked += RemoveEffect_Clicked;
            b.Show();
            field.PackStart(b, false, false, 0);

            Show();
            ShowAll();
        }


        private void RemoveEffect_Clicked(object sender, EventArgs e)
        {
            TreeIter selected;
            if (EffectsTree.Selection.GetSelected(out selected))
            {
                int row = (int)EffectsModel.GetValue(selected, 0);

                if (Projectile.Effects.Count == 1)
                {
                    MessageDialog md = new MessageDialog(null,
                    DialogFlags.Modal, MessageType.Info,
                    ButtonsType.Ok, "Can't delete - you need at least one effect");
                    int result = md.Run();
                    md.Destroy();
                }
                else
                {
                    MessageDialog md = new MessageDialog(null,
                    DialogFlags.Modal, MessageType.Warning,
                    ButtonsType.OkCancel, "Are you sure you want to delete effect #" + (row + 1) + "?");
                    int result = md.Run();
                    md.Destroy();

                    if (result == -5)
                    {
                        Projectile.Effects.RemoveAt(row);
                        EffectsModel.Remove(ref selected);

                        WriteChanges();
                    }
                }
            }
        }

        private void NewEffect_Clicked(object sender, EventArgs e)
        {
            ProjectileEffect effect = new ProjectileEffect();
            effect.EffectType = ProjectileEffectType.Damage;
            effect.EffectDuration = 0.0F;
            effect.EffectImpact = 1.0F;
            Projectile.Effects.Add(effect);

            WriteChanges();

            PopulateTreeWithEffects();
        }

        private void ProjectileEditLayout_Destroyed(object sender, EventArgs e)
        {
            GtkHelpers.FlushAllDeferredEvents();
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


        private void RadiusCellRenderer_Edited(object o, EditedArgs args)
        {
            TreeIter iter;
            if (EffectsModel.GetIterFromString(out iter, args.Path))
            {
                try
                {
                    float newValue = float.Parse(args.NewText);
                    int row = (int)EffectsModel.GetValue(iter, 0);
                    float currentValue = Projectile.Effects[row].EffectRadius;
                    if (newValue != currentValue)
                    {
                        EffectsModel.SetValue(iter, 4, newValue);
                        Projectile.Effects[row].EffectRadius = newValue;
                        WriteChanges();
                    }
                }
                catch (Exception ex) { }
            }
        }

        private void PopulateTreeWithEffects()
        {
            EffectsModel.Clear();

            for (int i = 0; i < Projectile.Effects.Count; i++)
            {
                string effect = Enum.GetName(typeof(ProjectileEffectType), Projectile.Effects[i].EffectType);
                //string effect = (Projectile.Effects[i].EffectType == ProjectileEffectType.Damage ? "Damage" : "Slow");
                object[] values = { i, effect, Projectile.Effects[i].EffectDuration, Projectile.Effects[i].EffectImpact, Projectile.Effects[i].EffectRadius };
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

        private void ImpactAsset_Changed(object sender, EventArgs e)
        {
            string newName = ((Entry)sender).Text;
            if ((newName != Projectile.ImpactAsset) && (newName.Length > 0))
            {
                Projectile.ImpactAsset = newName;
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


        public void AddProjectile()
        {

            Projectile p = new Projectile();
            p.Name = "New Projectile";
            p.Asset = "";
            p.AirSpeed = 1.0F;

            ProjectileEffect pe = new ProjectileEffect();
            pe.EffectType = ProjectileEffectType.Damage;
            pe.EffectDuration = 0.0F;
            pe.EffectImpact = 1.0F;
            p.Effects.Add(pe);

            Projectiles.Add(p);

            WriteChanges();

            TreeRefreshNeeded?.Invoke();
        }


        public void RemoveProjectile(string name)
        {
            for (int i = 0; i < Projectiles.Count; i++)
            {
                if (Projectiles[i].Name == name)
                {
                    Projectiles.Remove(Projectiles[i]);
                    WriteChanges();
                    TreeRefreshNeeded?.Invoke();
                    break;
                }
            }
        }


        void WriteChanges()
        {
            LevelManager.WriteProjectiles(Projectiles);
        }
    }
}



