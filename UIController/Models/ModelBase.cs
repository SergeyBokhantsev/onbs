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

        private readonly IDispatcher dispatcher;

        protected readonly ILogger logger;

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
        }

        public T GetProperty<T>(string name)
        {
            logger.Log(string.Format("Getting PageModel property '{0}'", name), LogLevels.Debug);

            lock (properties)
            {
                if (!properties.ContainsKey(name))
                    return default(T);

                return (T)properties[name];
            }
        }

        public void SetProperty(string name, object value)
        {
            logger.Log(string.Format("Setting PageModel property '{0}' with value of type '{1}'", name, value != null ? value.GetType().ToString() : "NULL"), LogLevels.Debug);

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

            logger.Log(string.Format("Performing PageModel action '{0}'", actionArgs.ActionName), LogLevels.Debug);
            dispatcher.Invoke(null, null, new EventHandler((s, a) => DoAction(actionArgs)));
        }

        /// <summary>
        /// Ececute action logic in Main thread
        /// </summary>
        /// <param name="actionArgs"></param>
        protected abstract void DoAction(PageModelActionEventArgs actionArgs);

        public void Dispose()
        {
            logger.Log(string.Format("Performing PageModel disposing: '{0}'", this.Name), LogLevels.Debug);

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

        public void Button(Buttons button, ButtonSates state)
        {
            switch (button)
            {
                case Buttons.Accept:
                    dispatcher.Invoke(null, null, new EventHandler((s, a) => OnAcceptButton(state)));
                    break;

                default:
                    throw new NotImplementedException(button.ToString());
            }
        }

        protected virtual void OnAcceptButton(ButtonSates state)
        {
        }
    }
}
