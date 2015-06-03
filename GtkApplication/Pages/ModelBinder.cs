using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using Interfaces;
using Interfaces.UI;

namespace GtkApplication.Pages
{
    internal abstract class Binding
    {
        public string PropertyName { get; private set; }

        public Binding(string propertyName)
        {
            PropertyName = propertyName;
        }

        public abstract void Update(object value);
    }

    internal class LabelTextBinding : Binding
    {
        private readonly Label label;

        public LabelTextBinding(Label label, string propertyName)
            :base(propertyName)
        {
            this.label = label;
        }

        public override void Update(object value)
        {
            label.Text = value != null ? value.ToString() : string.Empty;
        }
    }

    internal abstract class ColorBinding : Binding
    {
        protected ColorBinding(string propertyName)
            :base(propertyName)
        {
        }

        protected bool IsEqual(Gdk.Color c1, Gdk.Color c2)
        {
            return c1.Red == c2.Red && c1.Green == c2.Green && c1.Blue == c2.Blue;
        }

        protected bool IsOpaque(Gdk.Color color)
        {
            return IsEqual(ModelBinder.Qpaque, color);
        }
    }

    internal class EventBoxBgColorBinding : ColorBinding
    {
        private readonly EventBox box;
        private readonly Dictionary<string, Gdk.Color> colorMap;

        public EventBoxBgColorBinding(EventBox box, string propertyName, Dictionary<string, Gdk.Color> colorMap)
            :base(propertyName)
        {
            this.box = box;
            this.colorMap = colorMap;
        }

        public override void Update(object value)
        {
            var key = value as string;

            if (colorMap.ContainsKey(key))
            {
                var color = colorMap[key];

                if (IsOpaque(color))
                {
                    box.VisibleWindow = false;
                }
                else
                {
                    box.VisibleWindow = true;
                    box.ModifyBg(StateType.Normal, color);
                }
            }
        }
    }

    internal class ButtonLabelBinding : Binding
    {
        private readonly Button button;
        private readonly string prefix;

        public ButtonLabelBinding(Button button, string propertyName, string prefix)
            : base(propertyName)
        {
            this.button = button;
            this.prefix = prefix;
        }

        public override void Update(object value)
        {
            var label = value as string;
            button.Label = (!string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(prefix)) ?
                            string.Concat(prefix, ": ", label)
                            : label;
        }
    }

    internal class MetricsBinding : Binding
    {
        private readonly Action<IMetrics> updater;

        public MetricsBinding(Action<IMetrics> updater, string propertyName)
            : base(propertyName)
        {
            this.updater = updater;
        }

        public override void Update(object value)
        {
            updater(value as IMetrics);
        }
    }

    internal class ModelBinder
    {
        public static readonly Gdk.Color Qpaque = new Gdk.Color(137, 217, 28);

        private readonly IPageModel model;

        private readonly List<Binding> bindings = new List<Binding>();

        public ModelBinder(IPageModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.model = model;

            model.PropertyChanged += PropertyChanged;
        }

        private void PropertyChanged(string propertyName)
        {
            UpdateBindings(bindings.Where(b => b.PropertyName == propertyName));
        }

        private void UpdateBindings(IEnumerable<Binding> binds)
        {
            if (binds.Any())
            {
                var value = model.GetProperty<object>(binds.First().PropertyName);

                Application.Invoke(new EventHandler((s, a) =>
                {
                    foreach(var b in binds)
                        b.Update(value);
                }));
            }
        }

        private void UpdateBinding(Binding binding)
        {
            var value = model.GetProperty<object>(binding.PropertyName);

            Application.Invoke(new EventHandler((s, a) =>
            {
                binding.Update(value);
            }));
        }

        public void BindLabelText(Label label, string propName = null)
        {
            propName = propName ?? label.Name;
            var binding = new LabelTextBinding(label, propName);
            bindings.Add(binding);
            UpdateBinding(binding);
        }

        public void BindEventBoxBgColor(EventBox box, string propName, Dictionary<string, Gdk.Color> colorMap)
        {
            var binding = new EventBoxBgColorBinding(box, propName, colorMap);
            bindings.Add(binding);
            UpdateBinding(binding);
        }

        public void BindButtonClick(Button button, string actionName)
        {
            button.Clicked += (s, a) => model.Action(new PageModelActionEventArgs(actionName));
        }

        public void BindButtonLabel(Button button, string propName, string prefix = null)
        {
            var binding = new ButtonLabelBinding(button, propName, prefix);
            bindings.Add(binding);
            UpdateBinding(binding);
        }

        public void BindMetrics(Action<IMetrics> updater, string propName)
        {
            bindings.Add(new MetricsBinding(updater, propName));
        }
    }
}
