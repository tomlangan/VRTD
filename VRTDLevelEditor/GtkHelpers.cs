using System;
using Gtk;
using System.Timers;

namespace VRTD.LevelEditor
{
    public class DeferredEventHelper
    {
        Timer t;
        EventHandler Callback;
        object o;
        EventArgs a;


        public DeferredEventHelper(EventHandler callback)
        {
            Callback = callback;
            t = new Timer(500);
            t.Elapsed += T_Elapsed;
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (null != Callback)
            {
                Callback(o, a);
            }
        }

        public void Fired(object sender, EventArgs args)
        {
            o = sender;
            a = args;
            t.Stop();
            t.Start();
        }
    }

    public class GtkHelpers
    {

        public static HBox TextEntryField(string fieldName, string value, EventHandler callback, bool deferred = false)
        {
            HBox box = new HBox(false, 20);

            Label label = new Label(fieldName);
            box.Add(label);
            label.Show();

            Entry entry = new Entry();
            entry.WidthRequest = 200;
            box.Add(entry);
            entry.Text = value;
            if (deferred)
            {
                DeferredEventHelper h = new DeferredEventHelper(callback);
                entry.Changed += h.Fired;
            }
            else
            { 
                entry.Changed += callback;
            }
            entry.Show();

            return box;
        }

    }
}
