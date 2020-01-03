using System;
using VRTD.Gameplay;
using Gtk;

namespace VRTD.LevelEditor
{
    public class LevelEditLayout : Layout
    {
        public LevelDescription LevelDesc { get; set; }
        VBox Layout;
        public delegate void TreeRefreshNeededFunc();
        public event TreeRefreshNeededFunc TreeRefreshNeeded;

        public LevelEditLayout() : base(null, null)
        {


        }

        public void SetLevel(LevelDescription desc)
        {
            Layout = new VBox(false, 20);
            Add(Layout);

            HBox field = GtkHelpers.TextEntryField("Level Name", desc.Name, Name_Changed, true);
            Layout.Add(field);
            field.Show();

            

            Layout.ShowAll();

            LevelDesc = desc;
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

        void WriteChanges()
        {
            LevelManager.WriteLevel(LevelDesc.Name, LevelDesc);
        }
    }
}



