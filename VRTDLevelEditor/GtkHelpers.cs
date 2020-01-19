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
        GtkHelpers.ValueType ValueTypeToValidate = GtkHelpers.ValueType.Str;


        public DeferredEventHelper(EventHandler callback, List<DeferredEventHelper> list, GtkHelpers.ValueType valueType)
        {
            Callback = callback;
            t = new Timer(500);
            t.Elapsed += T_Elapsed;
            DeferredEventsList = list;
            ValueTypeToValidate = valueType;
        }

        private void ValidateAndSetTextColor()
        {
            if (ValueTypeToValidate == GtkHelpers.ValueType.None ||
                ValueTypeToValidate == GtkHelpers.ValueType.None)
            {
                return;
            }

            Entry entry = (Entry)o;
            string val = entry.Text;

            Gdk.Color col = GtkHelpers.Color("black");
            entry.TooltipText = "";

            try
            {
                switch (ValueTypeToValidate)
                {
                    case GtkHelpers.ValueType.Int:
                        int.Parse(val);
                        break;
                    case GtkHelpers.ValueType.Float:
                        float.Parse(val);
                        break;
                }
            }
            catch(Exception ex)
            {
                entry.TooltipText = ex.Message;
                col = GtkHelpers.Color("red");
            }

            entry.ModifyText(StateType.Normal, col);
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            DeferredEventsList.Remove(this);
            if (null != t)
            {
                t.Stop();
            }
            if (null != Callback)
            {
                Gtk.Application.Invoke(delegate
                {
                    Callback(o, a);
                });
            }
        }

        public void Fired(object sender, EventArgs args)
        {
            if (null != t && null != Callback)
            {
                o = sender;
                a = args;
                t.Stop();
                t.Start();
                ValidateAndSetTextColor();
                DeferredEventsList.Add(this);
            }
        }

        public void Flush()
        {
            DeferredEventsList.Remove(this);
            if (null != t)
            {
                t.Close();
                t = null;
            }
            if (null != Callback)
            {
                Callback(o, a);
                Callback = null;
            }
        }
    }

    public class GtkHelpers
    {
        public enum ValueType { Str, Int, Float, None }
        public static List<DeferredEventHelper> DeferredEvents = new List<DeferredEventHelper>();

        public static void FlushAllDeferredEvents()
        {
            for (int i = DeferredEvents.Count; i > 0; i--)
            {
                DeferredEvents[i - 1].Flush();
            }
        }

        public static HBox TextEntryField(string fieldName, string value, EventHandler callback, bool deferred = false, ValueType valType = ValueType.Str)
        {
            HBox box = new HBox(false, 20);

            Label label = new Label(fieldName);
            box.PackStart(label, false, false, 0);
            label.Show();

            Entry entry = new Entry();
            entry.WidthRequest = 200;
            box.PackStart(entry, false, false, 0);
            entry.Text = value;

            if (valType != ValueType.Str)
            {
                deferred = true;
            }

            if (deferred)
            {
                DeferredEventHelper h = new DeferredEventHelper(callback, DeferredEvents, valType);
                entry.Changed += h.Fired;
            }
            else
            { 
                entry.Changed += callback;
            }
            entry.Show();

            return box;
        }

        public static HBox ReadOnlyTextField(string fieldName, string value)
        {
            HBox box = new HBox(false, 20);

            Label label = new Label(fieldName);
            box.PackStart(label, false, false, 0);
            label.Show();

            Label entry = new Label();
            entry.WidthRequest = 200;
            box.PackStart(entry, false, false, 0);
            entry.Text = value;

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
                DeferredEventHelper h = new DeferredEventHelper(callback, DeferredEvents, ValueType.None);
                dropdown.Changed += h.Fired;
                DeferredEvents.Add(h);
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
