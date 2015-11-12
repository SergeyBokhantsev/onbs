using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System.Threading;
using UIController;
using System.Reflection;

namespace UIModels
{
    public abstract class ModelBase : IPageModel
    {
        public event PageModelPropertyChangedHandler PropertyChanged;

        public event EventHandler Disposing;

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        protected readonly IHostController hc;
        protected readonly ApplicationMap map;

        protected bool Disposed 
        { 
            get; 
            private set; 
        }

        public string ViewName
        {
            get;
            private set;
        }

        public bool NoDialogsAllowed
        {
            get;
            protected set;
        }

        protected ModelBase(string viewName, IHostController hc, ApplicationMap map)
        {
            if (hc == null)
                throw new ArgumentNullException("IHostController");
            
            if (map == null)
                throw new ArgumentNullException("ApplicationMap");

            ViewName = viewName;
            this.map = map;
            this.hc = hc;
        }

        protected virtual object GetArgumentForPage(MappedPageAction mappedPageAction)
        {
            return null;
        }

        protected virtual void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            hc.Logger.Log(this, string.Format("No '{0}' action exists to execute for button {1}, state {2}.", name, actionArgs.ActionName, actionArgs.State), LogLevels.Warning);
        }

        public T GetProperty<T>(string name)
        {
            hc.Logger.LogIfDebug(this, string.Format("Getting PageModel property '{0}'", name));

            lock (properties)
            {
                if (!properties.ContainsKey(name))
                    return default(T);

                return (T)properties[name];
            }
        }

        public void SetProperty(string name, object value)
        {
            hc.Logger.LogIfDebug(this, string.Format("Setting PageModel property '{0}' with value of type '{1}'", name, value != null ? value.GetType().ToString() : "NULL"));

            lock (properties)
            {
                properties[name] = value;
            }

            OnPropertyChanged(name);
        }

        public static IPageModel CreateModel(IHostController hc, ApplicationMap map, string modelTypeName = null, string viewName = null, object arg = null)
        {
            try
            {
                if (modelTypeName == null)
                {
                    modelTypeName = map.DefaultPageModelTypeName;
                }

                var type = Assembly.GetExecutingAssembly().GetType(string.Concat("UIModels.", modelTypeName));
                if (type != null)
                {
                    var constructor = type.GetConstructor(new Type[] { typeof(string), typeof(IHostController), typeof(ApplicationMap), typeof(object) });
                    if (constructor != null)
                    {
                        if (viewName == null)
                        {
                            viewName = map.GetPage(modelTypeName).DefaultViewName;
                        }

                        var model = constructor.Invoke(new[] { viewName, hc, map, arg }) as IPageModel;
                        map.SetCaptions(model);
                        return model;
                    }
                    else
                    {
                        hc.Logger.Log(typeof(ModelBase), string.Format("Apropriate constructor is not found for model '{0}'", modelTypeName), LogLevels.Warning);
                        return null;
                    }
                }
                else
                {
                    hc.Logger.Log(typeof(ModelBase), string.Format("Page model is not found '{0}'", modelTypeName), LogLevels.Warning);
                    return null;
                }
            }
            catch (Exception ex)
            {
                hc.Logger.Log(typeof(ModelBase), string.Format("Exception constructing page model '{0}'. Exception was: {1}", modelTypeName, ex.Message), LogLevels.Error);
                return null;
            }
        }

        public void Action(PageModelActionEventArgs actionArgs)
        {
            hc.Logger.LogIfDebug(this, string.Format("Performing PageModel action '{0}'", actionArgs.ActionName));

            var mappedAction = map.GetMappedActionFor(this, actionArgs);

            if (mappedAction != null)
            {
                hc.SyncContext.Post((o) =>
                {
                    var pageAction = o as MappedPageAction;

                    if (pageAction != null)
                    {
                        if (actionArgs.State == ButtonStates.Press)
                        {
                            var model = CreateModel(hc, map, pageAction.ModelTypeName, pageAction.ViewName, GetArgumentForPage(pageAction));
                            if (model != null)
                            {
                                hc.GetController<IUIController>().ShowPage(model);
                            }
                        }
                    }
                    else
                    {
                        var customAction = o as MappedCustomAction;

                        if (customAction != null && 
                            (customAction.ActionBehavior == MappedActionBehaviors.All
                            || (int)customAction.ActionBehavior == (int)actionArgs.State
                            || (customAction.ActionBehavior == MappedActionBehaviors.PressOrHold && actionArgs.State == ButtonStates.Press)
                            || (customAction.ActionBehavior == MappedActionBehaviors.PressOrHold && actionArgs.State == ButtonStates.Hold)))
                        {
                            DoAction(customAction.CustomActionName, actionArgs);
                        }
                    }

                }, mappedAction);
            }
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;

                hc.Logger.LogIfDebug(this, string.Format("Performing PageModel disposing: '{0}'", this.GetType().Name));

                var handler = Disposing;
                if (handler != null)
                    handler(null, null);

                PropertyChanged = null;
                Disposing = null;
            }
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

    public abstract class DialogModelBase : IDialogModel
    {
        public event Action Shown;
        public event Action<DialogResults> Closed;
        public event Action<DialogResults> ButtonClick;

        protected readonly IHostController hc;

        private int shownTime;
        protected int timeout;
        protected DialogResults defaultResult = DialogResults.None;

        public DialogModelBase(IHostController hc)
        {
            this.hc = hc;

            Shown += () => { if (timeout > 0) hc.CreateTimer(1000, TimerTick, true, false); };
        }

        private void TimerTick(IHostTimer timer)
        {
            shownTime += timer.Span;

            RemainingTime = timeout - shownTime;
            OnRemainingTimeChanged();

            if (shownTime >= timeout)
            {
                timer.Dispose();
                OnButtonClick(defaultResult);
            }
        }

        public void OnClosed(DialogResults result)
        {
            var handler = Closed;
            if (handler != null)
                hc.SyncContext.Post(o => handler((DialogResults)o), result);
        }

        public void OnShown()
        {
            var handler = Shown;
            if (handler != null)
                hc.SyncContext.Post(o => handler(), null);
        }

        public Dictionary<DialogResults, string> Buttons
        {
            get;
            set;
        }

        protected void OnButtonClick(DialogResults result)
        {
            var handler = ButtonClick;
            if (handler != null)
                handler(result);
        }


        public string Caption
        {
            get;
            protected set;
        }

        public string Message
        {
            get;
            protected set;
        }

        public int RemainingTime
        {
            get;
            private set;
        }

        private void OnRemainingTimeChanged()
        {
            var handler = RemainingTimeChanged;
            if (handler != null)
            {
                handler(RemainingTime);
            }
        }

        public event Action<int> RemainingTimeChanged;

        public virtual void HardwareButtonClick(Buttons button)
        {
        }
    }
}
