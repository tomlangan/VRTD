using System;
using Gtk;

public partial class MainWindow : Gtk.Window
{
    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        SetDefaultSize(1024, 768);
        SetPosition(WindowPosition.Center);

        DeleteEvent += delegate { Application.Quit(); };

        Fixed fix = new Fixed();

        Tree levels = new Tree();

        Button ExportButton = new Button("Export");
        ExportButton.SetSizeRequest(80, 40);

        ExportButton.Clicked += ExportButton_Clicked;

        //fix.Put(levels, 0, 0);
        fix.Put(ExportButton, 20, 30);

        Add(fix);
        ShowAll();
    }

    private void ExportButton_Clicked(object sender, EventArgs e)
    {
        
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    void LoadTestData()
    {

    }
}
