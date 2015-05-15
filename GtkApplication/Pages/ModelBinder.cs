using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
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

    internal class ModelBinder
    {
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

        public void BindButtonClick(Button button, string actionName)
        {
            button.Clicked += (s, a) => model.Action(new PageModelActionEventArgs(actionName));
        }
    }
}
