using System;
using VRTD.Gameplay;
using Gtk;

namespace VRTD.LevelEditor
{
    public class LevelEditLayout : Layout
    {
        static string[] LayoutOptions = { "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "60", "65", "70", "75", "80", "85", "90", "100" };

        public LevelDescription LevelDesc { get; set; }
        VBox Layout = null;
        public delegate void TreeRefreshNeededFunc();
        public event TreeRefreshNeededFunc TreeRefreshNeeded;

        public LevelEditLayout() : base(null, null)
        {


        }

        public void SetLevel(LevelDescription desc)
        {
            GtkHelpers.FlushAllDeferredEvents();

            if (null != Layout)
            {
                Layout.HideAll();
                Layout.Destroy();
                Layout = null;
            }

            Layout = new VBox(false, 20);
            Add(Layout);

            HBox field = GtkHelpers.TextEntryField("Level Name", desc.Name, Name_Changed, true);
            Layout.Add(field);
            field.Show();

            field = GtkHelpers.ComboBox("Width", LayoutOptions, (desc.FieldWidth/5 - 1), Width_Changed, true);
            Layout.Add(field);
            field.Show();

            field = GtkHelpers.ComboBox("Depth", LayoutOptions, (desc.FieldDepth / 5 - 1), Depth_Changed, true);
            Layout.Add(field);
            field.Show();

            Table map = GetFieldTable(desc);
            Layout.Add(map);
            map.Show();

            Layout.ShowAll();
            LevelDesc = desc;
        }

        private Table GetFieldTable(LevelDescription desc)
        {
            Table t = new Table((uint)desc.FieldDepth, (uint)desc.FieldWidth, true);

            for (uint i = 0; i < desc.Map.Count; i++)
            {
                Button b = new Button();
                SetFieldButtonType(b, desc.Map[(int)i]);
                uint xpos = i % (uint)desc.FieldWidth;
                uint ypos = i / (uint)desc.FieldWidth;
                t.Attach(b, xpos, xpos + 1, ypos, ypos + 1);
                b.Show();
            }

            return t;
        }

        private void SetFieldButtonType(Button b, char c)
        { 
            Gdk.Color col = new Gdk.Color();
            string s = "";
            switch (c)
            {
                case 'D':
                    Gdk.Color.Parse("green", ref col);
                    s = "D";
                    break;
                case 'R':
                    Gdk.Color.Parse("black", ref col);
                    s = "R";
                    break;
                case 'T':
                    Gdk.Color.Parse("grey", ref col);
                    s = "T";
                    break;
            }
            b.ModifyBg(StateType.Normal, col);
            b.Label = s;
        }

        private void Name_Changed(object sender, EventArgs e)
        { 
            string newName = ((Entry)sender).Text;
            if ((newName != LevelDesc.Name) && (newName.Length > 0))
            {
                LevelManager.RenameLevel(LevelDesc.Name, newName);
                LevelDesc.Name = newName;
                WriteChanges();
                if (null != TreeRefreshNeeded)
                {
                    TreeRefreshNeeded();
                }
            }
        }


        private void Width_Changed(object sender, EventArgs e)
        {
            int newIndex = ((ComboBox)sender).Active;
            int newValue = int.Parse(LayoutOptions[newIndex]);
            if (LevelDesc.FieldWidth != newValue)
            {
                LevelDesc.FieldWidth = newValue;
                WriteChanges();
            }

            // Need to redraw the field
            int newFieldSize = LevelDesc.FieldWidth * LevelDesc.FieldDepth;
            ResizeMap(newFieldSize);
        }


        private void Depth_Changed(object sender, EventArgs e)
        {
            int newIndex = ((ComboBox)sender).Active;
            int newValue = int.Parse(LayoutOptions[newIndex]);
            if (LevelDesc.FieldDepth != newValue)
            {
                LevelDesc.FieldDepth = newValue;
                WriteChanges();
            }


            // Need to redraw the field
            int newFieldSize = LevelDesc.FieldWidth * LevelDesc.FieldDepth;
            ResizeMap(newFieldSize);
        }


        private void ResizeMap(int newFieldSize)
        {
            if (LevelDesc.Map.Count > newFieldSize)
            {
                LevelDesc.Map.RemoveRange(newFieldSize, LevelDesc.Map.Count - newFieldSize);
            }
            else if (LevelDesc.Map.Count < newFieldSize)
            {
                int itemsToAdd = newFieldSize - LevelDesc.Map.Count;
                for (int i = 0; i < itemsToAdd; i++)
                {
                    LevelDesc.Map.Add('D');
                }
            }
            SetLevel(LevelDesc);
        }

        void WriteChanges()
        {
            LevelManager.WriteLevel(LevelDesc.Name, LevelDesc);
        }
    }
}



