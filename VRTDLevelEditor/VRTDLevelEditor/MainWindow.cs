using System;
using Gtk;
using System.Runtime.Serialization;

namespace VRTDLevelEditor
{
    public partial class MainWindow : Window
    {
        public MainWindow() :
                base(WindowType.Toplevel)
        {
            Resize(800, 600);

            /* Set a handler for delete_event that immediately
             * exits GTK. */
            DeleteEvent += delete_event;


            /* Create a 2x2 table */
            Table table = new Table(25, 5, true);

            /* Put the table in the main window */
            Add(table);

            /* Create first button */
            TreeView tree = new TreeView();


            table.Attach(tree, 0, 1, 0, 24);

            tree.Show();


            Button exportButton = new Button("Export");

            /* When the button is clicked, we call the "callback" function
             * with a pointer to "button 2" as its argument */

            exportButton.Clicked += ExportButton_Clicked;

            /* Insert button 2 into the upper right quadrant of the table */
            table.Attach(exportButton, 1, 2, 24, 25);

            exportButton.Show();

            /* Create "Quit" button */
            Button quitbutton = new Button("Quit");

            /* When the button is clicked, we call the "delete_event" function
             * and the program exits */
            quitbutton.Clicked += exit_event;

            /* Insert the quit button into the both
             * lower quadrants of the table */
            table.Attach(quitbutton, 4, 5, 24, 25);

            quitbutton.Show();

            Button addLevelButton = new Button("+");

            addLevelButton.Clicked += AddLevelButton_Clicked;

            table.Attach(addLevelButton, 0, 1, 24, 25);

            addLevelButton.Show();


            table.Show();
            ShowAll();
        }

        private void AddLevelButton_Clicked(object sender, EventArgs e)
        {
            AddLevelWindow dialog = new AddLevelWindow();
            dialog.Show();
        }

        private void ExportButton_Clicked(object sender, EventArgs e)
        {
            LevelDescription ld = LevelLoader.GetTestLevel();

        }


        static void delete_event(object obj, DeleteEventArgs args)
        {
            Application.Quit();
        }

        static void exit_event(object obj, EventArgs args)
        {
            Application.Quit();
        }

    }
}
