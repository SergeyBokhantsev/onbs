using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using Interfaces;
using Interfaces.UI;
using System.Diagnostics;

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
		private readonly Func<object, string> formatter;

		public LabelTextBinding(Label label, string propertyName, Func<object, string> formatter)
            :base(propertyName)
        {
            this.label = label;
			this.formatter = formatter ?? new Func<object, string> (o => { return o == null ? string.Empty : o.ToString(); });
        }

        public override void Update(object value)
        {
			label.Text = formatter(value);
        }
    }

	internal class LabelMarkupBinding : Binding
	{
		private readonly Label label;
		private readonly Func<object, string> formatter;

		public LabelMarkupBinding(Label label, string propertyName, Func<object, string> formatter)
			:base(propertyName)
		{
			this.label = label;
			this.formatter = formatter ?? new Func<object, string> (o => { return o == null ? string.Empty : o.ToString(); });
		}

		public override void Update(object value)
		{
			label.Markup = formatter(value);
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

			if (colorMap.ContainsKey (key)) 
			{
				var color = colorMap [key];

				if (IsOpaque (color)) {
					box.VisibleWindow = false;
				} else {
					box.VisibleWindow = true;
					box.ModifyBg (StateType.Normal, color);
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

	internal class FlatButtonLabelBinding : Binding
	{
		private readonly FlatButton button;
		private readonly string prefix;

		public FlatButtonLabelBinding(FlatButton button, string propertyName, string prefix)
			: base(propertyName)
		{
			this.button = button;
			this.prefix = prefix;
		}

		public override void Update(object value)
		{
			var label = value as string;
			button.Text = (!string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(prefix)) ?
				string.Concat(prefix, ": ", label)
				: label;
		}
	}

	internal class CustomActionBinding<T> : Binding
	{
		private readonly Action<T> action;

		public CustomActionBinding(Action<T> action, string propertyName)
			: base(propertyName)
		{
			this.action = action;
		}

		public override void Update(object value)
		{
            if (value == null)
                action(default(T));
            else
			    action((T)value);
		}
	}

    internal class ModelBinder
    {
        public static readonly Gdk.Color Qpaque = new Gdk.Color(137, 217, 28);

		private readonly ILogger logger;

        private readonly List<Binding> bindings = new List<Binding>();

        private bool disposed;

		public IPageModel Model {
			get;
			private set;
		}

		public ModelBinder(IPageModel model, ILogger logger)
        {
            if (model == null)
                throw new ArgumentNullException("model");

			if (logger == null)
				throw new ArgumentNullException("logger");

            this.Model = model;
			this.logger = logger;

            model.PropertyChanged += PropertyChanged;
            model.Disposing += model_Disposing;
        }

        void model_Disposing(object sender, EventArgs e)
        {
            lock (bindings)
            {
                disposed = true;
                bindings.Clear();
            }
        }

        private void PropertyChanged(string propertyName)
        {
            UpdateBindings(bindings.Where(b => b.PropertyName == propertyName));
        }

        private void UpdateBindings(IEnumerable<Binding> binds)
        {
            if (!disposed)
            {
                Application.Invoke(new EventHandler((s, a) =>
                {
                    lock (bindings) // because binds is a subset of bindings and we shall avoid concurrent changes
                    {
                        object value = null;
                        string propName = null;

                        foreach (var b in binds)
                        {
                            try
                            {
                                if (b.PropertyName != propName)
                                {
                                    value = Model.GetProperty<object>(b.PropertyName);
                                    propName = b.PropertyName;
                                }

                                b.Update(value);
                            }
                            catch (Exception ex)
                            {
                                logger.Log(this, string.Concat("Exception updating binding: ", b != null ? b.PropertyName : "NULL BINDING"), LogLevels.Error);
                                logger.Log(this, ex);
                            }
                        }
                    }
                }));
            }
		}

        private void UpdateBinding(Binding binding)
        {
			var value = Model.GetProperty<object>(binding.PropertyName);

            Application.Invoke(new EventHandler((s, a) =>
            {
						try
						{
                			binding.Update(value);
						}
						catch (Exception ex)
						{
                            logger.Log(this, string.Concat("Exception updating binding: ", binding != null ? binding.PropertyName : "NULL BINDING"), LogLevels.Error);
							logger.Log(this, ex);
						}
            }));
        }

        private void AddBinding(Binding binding)
        {
            lock (bindings)
            {
                bindings.Add(binding);
            }
        }

		public void UpdateBindings()
		{
            UpdateBindings(bindings);
		}

        [Conditional("DEBUG")]
        private void AssertDisposed()
        {
            if (disposed)
                throw new InvalidOperationException("Model is disposed");
        }

		public void BindLabelText(Label label, string propName = null, Func<object, string> formatter = null)
        {
            AssertDisposed();
            propName = propName ?? label.Name;
            var binding = new LabelTextBinding(label, propName, formatter);
            AddBinding(binding);
            UpdateBinding(binding);
        }

		public void BindLabelMarkup(Label label, string propName = null, Func<object, string> formatter = null)
		{
            AssertDisposed();
			propName = propName ?? label.Name;
			var binding = new LabelMarkupBinding(label, propName, formatter);
            AddBinding(binding);
			UpdateBinding(binding);
		}

        public void BindEventBoxBgColor(EventBox box, string propName, Dictionary<string, Gdk.Color> colorMap)
        {
            AssertDisposed();
            var binding = new EventBoxBgColorBinding(box, propName, colorMap);
            AddBinding(binding);
            UpdateBinding(binding);
        }

        public void BindButtonClick(Button button, string actionName)
        {
            AssertDisposed();
			button.Clicked += (s, a) => Model.Action(new PageModelActionEventArgs(actionName, Interfaces.Input.ButtonStates.Press));
        }

		public void BindFlatButtonClick(FlatButton button, string actionName)
		{
            AssertDisposed();
			button.Clicked += () => Model.Action(new PageModelActionEventArgs(actionName, Interfaces.Input.ButtonStates.Press));
		}

		public void BindEventBoxClick(EventBox eventBox, string actionName)
		{
            AssertDisposed();
			eventBox.ButtonPressEvent += (s, e) => Model.Action(new PageModelActionEventArgs(actionName, Interfaces.Input.ButtonStates.Press));
			eventBox.ButtonReleaseEvent += (s, e) => Model.Action(new PageModelActionEventArgs(actionName, Interfaces.Input.ButtonStates.Release));
		}

        public void BindButtonLabel(Button button, string propName, string prefix = null)
        {
            AssertDisposed();
            var binding = new ButtonLabelBinding(button, propName, prefix);
            AddBinding(binding);
            UpdateBinding(binding);
        }

		public void BindFlatButtonLabel(FlatButton button, string propName, string prefix = null)
		{
            AssertDisposed();
			var binding = new FlatButtonLabelBinding(button, propName, prefix);
            AddBinding(binding);
			UpdateBinding(binding);
		}

		public void BindCustomAction<T>(Action<T> action, string propName)
		{
            AssertDisposed();
            AddBinding(new CustomActionBinding<T>(action, propName));
		}
    }
}
