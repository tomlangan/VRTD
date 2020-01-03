using System;
using Gtk;
using System.Timers;
using System.Collections.Generic;

namespace VRTD.LevelEditor
{
    public class DeferredEventHelper
    {
        Timer t;
        EventHandler Callback;
        object o;
        EventArgs a;
        List<DeferredEventHelper> DeferredEventsList;


        public DeferredEventHelper(EventHandler callback, List<DeferredEventHelper> list)
        {
            Callback = callback;
            t = new Timer(500);
            t.Elapsed += T_Elapsed;
            DeferredEventsList = list;
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Close();
            if (null != Callback)
            {
                Gtk.Application.Invoke(delegate
                {
                    Callback(o, a);
                });
            }
            DeferredEventsList.Remove(this);
        }

        public void Fired(object sender, EventArgs args)
        {
            o = sender;
            a = args;
            t.Stop();
            t.Start();
        }

        public void Flush()
        {
            t.Stop();
            t.Close();
            if (null != Callback)
            {
                Callback(o, a);
            }
            DeferredEventsList.Remove(this);
        }
    }

    public class GtkHelpers
    {
        public static List<DeferredEventHelper> DeferredEvents = new List<DeferredEventHelper>();

        public static void FlushAllDeferredEvents()
        {
            for (int i = DeferredEvents.Count; i > 0; i--)
            {
                DeferredEvents[i - 1].Flush();
            }
        }

        public static HBox TextEntryField(string fieldName, string value, EventHandler callback, bool deferred = false)
        {
            HBox box = new HBox(false, 20);

            Label label = new Label(fieldName);
            box.PackStart(label, false, false, 0);
            label.Show();

            Entry entry = new Entry();
            entry.WidthRequest = 200;
            box.PackStart(entry, false, false, 0);
            entry.Text = value;
            if (deferred)
            {
                DeferredEventHelper h = new DeferredEventHelper(callback, DeferredEvents);
                entry.Changed += h.Fired;
            }
            else
            { 
                entry.Changed += callback;
            }
            entry.Show();

            return box;
        }


        public static HBox ComboBox(string fieldName, string[] values, int initialValueIndex, EventHandler callback, bool deferred = false)
        {
            HBox box = new HBox(false, 20);

            Label label = new Label(fieldName);
            box.PackStart(label, false, false, 0);
            label.Show();

            ComboBox dropdown = new ComboBox(values);
            dropdown.WidthRequest = 200;
            dropdown.Active = initialValueIndex;
            box.PackStart(dropdown, false, false, 0);
            if (deferred)
            {
                DeferredEventHelper h = new DeferredEventHelper(callback, DeferredEvents);
                dropdown.Changed += h.Fired;
            }
            else
            {
                dropdown.Changed += callback;
            }
            dropdown.Show();

            return box;
        }

        public static Gdk.Color Color(string name)
        {
            Gdk.Color col = new Gdk.Color();
            Gdk.Color.Parse(name, ref col);
            return col;
        }

    }
}
