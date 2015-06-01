using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Input;
using Interfaces.UI;

namespace UIController.Models
{
    public abstract class ModelBase : IPageModel
    {
        public event PageModelPropertyChangedHandler PropertyChanged;

        public event EventHandler Disposing;

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        protected readonly IDispatcher dispatcher;

        protected readonly ILogger logger;

        private readonly Dictionary<Buttons, Action<ButtonStates>> buttonHandlersMap;

        public string Name
        {
            get;
            private set;
        }

        protected ModelBase(string name, IDispatcher dispatcher, ILogger logger)
        {
            Name = name;
            this.dispatcher = dispatcher;
            this.logger = logger;

            buttonHandlersMap = new Dictionary<Buttons, Action<ButtonStates>>()
            {
                { Buttons.Accept, new Action<ButtonStates>(OnAcceptButton) },
                { Buttons.Cancel, new Action<ButtonStates>(OnCancelButton) },
                { Buttons.F1, new Action<ButtonStates>(OnF1Button) },
                { Buttons.F2, new Action<ButtonStates>(OnF2Button) },
                { Buttons.F3, new Action<ButtonStates>(OnF3Button) },
                { Buttons.F4, new Action<ButtonStates>(OnF4Button) },
                { Buttons.F5, new Action<ButtonStates>(OnF5Button) },
                { Buttons.F6, new Action<ButtonStates>(OnF6Button) },
                { Buttons.F7, new Action<ButtonStates>(OnF7Button) },
                { Buttons.F8, new Action<ButtonStates>(OnF8Button) },
            };
        }

        public T GetProperty<T>(string name)
        {
            logger.LogIfDebug(this, string.Format("Getting PageModel property '{0}'", name));

            lock (properties)
            {
                if (!properties.ContainsKey(name))
                    return default(T);

                return (T)properties[name];
            }
        }

        public void SetProperty(string name, object value)
        {
            logger.LogIfDebug(this, string.Format("Setting PageModel property '{0}' with value of type '{1}'", name, value != null ? value.GetType().ToString() : "NULL"));

            lock (properties)
            {
                properties[name] = value;
            }

            OnPropertyChanged(name);
        }

        public void Action(PageModelActionEventArgs actionArgs)
        {
            if (actionArgs == null)
                throw new ArgumentNullException("actionArgs");

            logger.LogIfDebug(this, string.Format("Performing PageModel action '{0}'", actionArgs.ActionName));

            if (dispatcher.Check())
                DoAction(actionArgs);
            else
                dispatcher.Invoke(null, null, new EventHandler((s, a) => DoAction(actionArgs)));
        }

        /// <summary>
        /// Ececute action logic in Main thread
        /// </summary>
        /// <param name="actionArgs"></param>
        protected abstract void DoAction(PageModelActionEventArgs actionArgs);

        public void Dispose()
        {
            logger.LogIfDebug(this, string.Format("Performing PageModel disposing: '{0}'", this.Name));

            var handler = Disposing;
            if (handler != null)
                handler(null, null);

            PropertyChanged = null;
            Disposing = null;
        }

        public void RefreshAllProps()
        {
            foreach(var pname in properties.Keys)
            {
                OnPropertyChanged(pname);
            }
        }

        private void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(name);
        }

        public void Button(Buttons button, ButtonStates state)
        {
            Action<ButtonStates> action = buttonHandlersMap[button];

            if (dispatcher.Check())
                action(state);
            else
                dispatcher.Invoke(null, null, new EventHandler((s, a) => action(state)));
        }

        protected virtual void OnAcceptButton(ButtonStates state)
        {
        }

        protected virtual void OnCancelButton(ButtonStates state)
        {
        }

        protected virtual void OnF1Button(ButtonStates state)
        {
        }

        protected virtual void OnF2Button(ButtonStates state)
        {
        }

        protected virtual void OnF3Button(ButtonStates state)
        {
        }

        protected virtual void OnF4Button(ButtonStates state)
        {
        }

        protected virtual void OnF5Button(ButtonStates state)
        {
        }

        protected virtual void OnF6Button(ButtonStates state)
        {
        }

        protected virtual void OnF7Button(ButtonStates state)
        {
        }

        protected virtual void OnF8Button(ButtonStates state)
        {
        }
    }
}
