using System;
using Gtk;
using System.Runtime.Serialization;

namespace VRTDLevelEditor
{
    public class AddLevelWindow : Window
    {
        public AddLevelWindow() :
                base(WindowType.Popup)
        {

            Resize(200, 50);

            /* Set a handler for delete_event that immediately
             * exits GTK. */
            DeleteEvent += delete_event;


            /* Create a 2x2 table */
            Table table = new Table(3, 3, true);

            /* Put the table in the main window */
            Add(table);



            Button doneButton = new Button("Done");

            doneButton.Clicked += DoneButton_Clicked;

            table.Attach(doneButton, 2, 3, 2, 3);

            doneButton.Show();

            table.Show();
            ShowAll();
        }

        private void DoneButton_Clicked(object sender, EventArgs e)
        {

        }


        /* another event */
        static void delete_event(object obj, DeleteEventArgs args)
        {
            ((AddLevelWindow)obj).Hide();
        }

        static void exit_event(object obj, EventArgs args)
        {
            Application.Quit();
        }

    }
}