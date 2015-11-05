using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System.Threading;

namespace UIModels
{
    public abstract class ModelBase : IPageModel
    {
        public event PageModelPropertyChangedHandler PropertyChanged;

        public event EventHandler Disposing;

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        protected readonly SynchronizationContext syncContext;

        protected readonly ILogger logger;

        protected bool onlyPressButtonEvents = true;

        public string Name
        {
            get;
            private set;
        }

        public bool NoDialogsAllowed
        {
            get;
            protected set;
        }

        protected ModelBase(string name, SynchronizationContext syncContext, ILogger logger)
        {
            if (syncContext == null)
                throw new ArgumentNullException("syncContext");

            Name = name;
            this.syncContext = syncContext;
            this.logger = logger;
        }

        public T GetProperty<T>(string name)
        {
            //logger.LogIfDebug(this, string.Format("Getting PageModel property '{0}'", name));

            lock (properties)
            {
                if (!properties.ContainsKey(name))
                    return default(T);

                return (T)properties[name];
            }
        }

        public void SetProperty(string name, object value)
        {
            //logger.LogIfDebug(this, string.Format("Setting PageModel property '{0}' with value of type '{1}'", name, value != null ? value.GetType().ToString() : "NULL"));

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

            if ((onlyPressButtonEvents && actionArgs.State == ButtonStates.Press) || !onlyPressButtonEvents)
            {
                logger.LogIfDebug(this, string.Format("Performing PageModel action '{0}'", actionArgs.ActionName));

                syncContext.Post(o => DoAction(o as PageModelActionEventArgs), actionArgs);
            }
            else
            {
                logger.LogIfDebug(this, string.Format("Skipping PageModel action '{0}'", actionArgs.ActionName));
            }
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
    }

    public class DialogModel : ModelBase, IDialogModel
    {
        public event Action Shown;
        public event Action<DialogResults> Closed;
        public event Action<DialogResults> ButtonClick;

        public DialogModel(string name, SynchronizationContext syncContext, ILogger logger)
            :base(name, syncContext, logger)
        {
        }

        public void OnClosed(DialogResults result)
        {
            var handler = Closed;
            if (handler != null)
                syncContext.Post(o => handler((DialogResults)o), result);
        }

        public void OnShown()
        {
            var handler = Shown;
            if (handler != null)
                syncContext.Post(o => handler(), null);
        }

        public string CaptionPropertyName
        {
            get;
            set;
        }

        public string MessagePropertyName
        {
            get { throw new NotImplementedException(); }
        }

        public Dictionary<DialogResults, string> Buttons
        {
            get;
            set;
        }

        protected override void DoAction(PageModelActionEventArgs actionArgs)
        {
        }

        protected void OnButtonClick(DialogResults result)
        {
            var handler = ButtonClick;
            if (handler != null)
                handler(result);
        }
    }
}
