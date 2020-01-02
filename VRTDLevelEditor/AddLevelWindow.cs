using System;
using Gtk;
using System.Runtime.Serialization;

namespace VRTD.LevelEditor
{
    public class AddLevelEventArgs
    {
        public string Name;
    }

    public class AddLevelWindow : Window
    {
        public delegate void AddLevelEventFunc(AddLevelEventArgs a);
        public event AddLevelEventFunc Finished;

        Entry NameInput;

        public AddLevelWindow(Window parent) :
                base(WindowType.Toplevel)
        {
            Parent = parent;
            Modal = true;
            
            SetDefaultSize(200, 50);
            SetPosition(WindowPosition.Center);

            /* Set a handler for delete_event that immediately
             * exits GTK. */
            DeleteEvent += delete_event;


            /* Create a 2x2 table */
            HBox layout = new HBox(false, 30);

            /* Put the table in the main window */
            Add(layout);

            Label NameLabel = new Label("Level Name");
            layout.Add(NameLabel);
            NameLabel.Show();

         
            NameInput = new Entry();
            NameInput.WidthRequest = 200;
            layout.Add(NameInput);
            NameInput.Show();

            Button cancelButton = new Button("Cancel");
            cancelButton.Clicked += CancelButton_Clicked;
            cancelButton.WidthRequest = 50;
            layout.Add(cancelButton);
            cancelButton.Show();

            Button doneButton = new Button("Done");
            doneButton.Clicked += DoneButton_Clicked;
            doneButton.WidthRequest = 50;
            layout.Add(doneButton);
            doneButton.Show();

            layout.Show();
            ShowAll();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            this.Destroy();
        }

        private void DoneButton_Clicked(object sender, EventArgs e)
        {
            if (NameInput.Text.Length == 0)
            {
                MessageDialog md = new MessageDialog(this,
                DialogFlags.Modal, MessageType.Error,
                ButtonsType.Ok, "Zero length name");
                md.Run();
                md.Destroy();
                return;
            }

            if (null != Finished)
            {
                AddLevelEventArgs a = new AddLevelEventArgs();
                a.Name = NameInput.Text;
                Finished(a);
            }

            this.Destroy();
            return;
        }


        /* another event */
        static void delete_event(object obj, DeleteEventArgs args)
        {
            ((AddLevelWindow)obj).Destroy();
        }

    }
}