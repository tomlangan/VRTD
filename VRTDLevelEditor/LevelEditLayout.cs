using System;
using VRTD.Gameplay;
using System.Timers;
using Gtk;
using System.Collections.Generic;

namespace VRTD.LevelEditor
{
    public class LevelEditLayout : ScrolledWindow
    {
        static string[] LayoutOptions = { "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "60", "65", "70", "75", "80", "85", "90", "100" };

        public LevelDescription LevelDesc { get; set; }
        VBox Layout = null;
        public delegate void TreeRefreshNeededFunc();
        public event TreeRefreshNeededFunc TreeRefreshNeeded;
        Table MapTable;
        Dictionary<Button, int> MapMappings;
        Entry ErrorEntry;

        public LevelEditLayout() : base(null, null)
        {
        }


        public void SetLevel(LevelDescription desc)
        {
            GtkHelpers.FlushAllDeferredEvents();

            if (null != Layout)
            {
                Layout.Hide();
                Layout.Destroy();
                Layout = null;
            }

            Layout = new VBox(false, 0);
            AddWithViewport(Layout);

            HBox field = GtkHelpers.TextEntryField("Level Name", desc.Name, Name_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.ComboBox("Width", LayoutOptions, (desc.FieldWidth/5 - 1), Width_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            field = GtkHelpers.ComboBox("Depth", LayoutOptions, (desc.FieldDepth / 5 - 1), Depth_Changed, true);
            Layout.PackStart(field, false, false, 0);
            field.Show();

            Table map = GetFieldTable(desc);
            Layout.PackStart(map, false, true, 0);
            map.Show();

            ErrorEntry = new Entry(500);
            ErrorEntry.Editable = false;
            ErrorEntry.Text = "No issues";
            ErrorEntry.ModifyText(StateType.Normal, GtkHelpers.Color("green"));
            Layout.PackStart(ErrorEntry, false, false, 10);
            ErrorEntry.Show();

            ValidateDescriptionAndReportIssues();

            Show();
            ShowAll();
            LevelDesc = desc;
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

            char newchar = 'D';
            if (index < LevelDesc.FieldWidth)
            {
                //
                // First row --> allow Entry
                // 
                switch (LevelDesc.Map[index])
                {
                    case 'D':
                        newchar = 'R';
                        break;
                    case 'R':
                        newchar = 'T';
                        break;
                    case 'T':
                        newchar = 'E';
                        break;
                    case 'E':
                        newchar = 'D';
                        break;
                }
            }
            else if ((index / LevelDesc.FieldWidth) == (LevelDesc.FieldDepth - 1))
            {
                //
                // Last row --> allow Exit
                // 
                switch (LevelDesc.Map[index])
                {
                    case 'D':
                        newchar = 'R';
                        break;
                    case 'R':
                        newchar = 'T';
                        break;
                    case 'T':
                        newchar = 'X';
                        break;
                    case 'X':
                        newchar = 'D';
                        break;
                }
            }
            else
            {
                //
                // Only the 3 main states
                // 
                switch (LevelDesc.Map[index])
                {
                    case 'D':
                        newchar = 'R';
                        break;
                    case 'R':
                        newchar = 'T';
                        break;
                    case 'T':
                        newchar = 'D';
                        break;
                }
            }

            MapTable.Remove(b);
            b = new Button();
            LevelDesc.Map[index] = newchar;
            SetFieldButtonType(b, newchar);
            SetButtonOnTable(b, LevelDesc, index);
            WriteChanges();
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
                    s = "T";
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

        bool ValidateDescriptionAndReportIssues()
        {
            bool issuesFound = false;
            bool warningsFound = false;
            string issueText = "No issues";
            string warningText = "";



            if (issuesFound)
            {
                ErrorEntry.ModifyText(StateType.Normal, GtkHelpers.Color("red"));
                ErrorEntry.Text = "Issues: " + Environment.NewLine + issueText;
            }
            else if (warningsFound)
            {
                ErrorEntry.ModifyText(StateType.Normal, GtkHelpers.Color("orange"));
                ErrorEntry.Text = "Warnings: " + Environment.NewLine + warningText;
            }
            else
            {
                ErrorEntry.ModifyText(StateType.Normal, GtkHelpers.Color("green"));
                ErrorEntry.Text = "No issues";
            }

            return true;
        }
    }
}



