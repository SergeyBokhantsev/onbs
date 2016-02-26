using System;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
using Interfaces.Input;
using Interfaces.UI;
using System.Reflection;

namespace UIModels
{
    public abstract class ModelBase : IPageModel
    {
        public event PageModelPropertyChangedHandler PropertyChanged;

        //public event PageModelFocusChangedHandler FocusChanged;

        public event EventHandler Disposing;

        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        protected readonly IHostController hc;
        protected readonly MappedPage pageDesctiptor;

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

        protected ModelBase(string viewName, IHostController hc, MappedPage pageDesctiptor)
        {
            if (hc == null)
                throw new ArgumentNullException("IHostController");
            
            if (pageDesctiptor == null)
                throw new ArgumentNullException("pageDescriptor");

            ViewName = viewName;
            this.pageDesctiptor = pageDesctiptor;
            this.hc = hc;
        }

        protected virtual void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
			hc.Logger.Log(this, string.Format("No '{0}' action exists to execute for button {1}, state {2}.", name, actionArgs.ActionName, actionArgs.State), LogLevels.Debug);
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

        protected object GetPropertyFast(string name)
        {
            lock (properties)
            {
                return properties[name];
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

        protected void UpdateLabelForAction(string customActionName, string labelValue)
        {
            var mappedAction = pageDesctiptor.ButtonsMap.OfType<MappedCustomAction>().FirstOrDefault(a => a.CustomActionName == customActionName);
            if (mappedAction != null)
            {
                var buttonLabelPropertyName = ModelNames.ResolveButtonLabelName(mappedAction.ButtonActionName);
                SetProperty(buttonLabelPropertyName, labelValue);
            }
        }

        public static IPageModel CreateModel(IHostController hc, MappedPage pageDescriptor, string viewName, object modelArgument)
        {
            try
            {
                if (pageDescriptor != null)
                {
                    var type = Assembly.GetExecutingAssembly().GetType(string.Concat("UIModels.", pageDescriptor.ModelTypeName));
                    if (type != null)
                    {
                        if (viewName == null)
                        {
                            viewName = pageDescriptor.DefaultViewName;
                        }

                        IPageModel model = null;

                        var constructor = type.GetConstructor(new[] { typeof(string), typeof(IHostController), typeof(MappedPage), typeof(object) });
                        if (constructor != null)
                        {
                            model = constructor.Invoke(new object[] { viewName, hc, pageDescriptor, modelArgument }) as IPageModel;
                        }
                        else
                        {
                            hc.Logger.LogIfDebug(typeof(ModelBase), string.Format("No extended constructor found for model '{0}'. Trying to use default...", pageDescriptor.ModelTypeName));
                            constructor = type.GetConstructor(new[] { typeof(string), typeof(IHostController), typeof(MappedPage) });

                            if (constructor != null)
                            {
                                model = constructor.Invoke(new object[] { viewName, hc, pageDescriptor }) as IPageModel;                               
                            }
                        }

                        if (model != null)
                        {
                            var initializableModel = model as ModelBase;
                            if (initializableModel != null)
                            {
                                initializableModel.Initialize();
                            }

                            ApplicationMap.SetCaptions(model, pageDescriptor);
                            return model;
                        }

                        hc.Logger.Log(typeof(ModelBase), string.Format("Apropriate constructor is not found for model '{0}'", pageDescriptor.ModelTypeName), LogLevels.Warning);
                        return null;
                    }

                    hc.Logger.Log(typeof(ModelBase), string.Format("Page model is not found '{0}'", pageDescriptor.ModelTypeName), LogLevels.Warning);
                    return null;
                }

                hc.Logger.Log(typeof(ModelBase), "pageDescriptor is not provided", LogLevels.Warning);
                return null;
            }
            catch (Exception ex)
            {
                hc.Logger.Log(typeof(ModelBase), string.Format("Exception constructing page model for mapped page '{0}'. Exception was: {1}", pageDescriptor != null ? pageDescriptor.Name : "No pageDescriptor", ex.Message), LogLevels.Error);
                throw;
            }
        }

        protected virtual void Initialize()
        {            
        }

        public void Action(PageModelActionEventArgs actionArgs)
        {
            hc.Logger.LogIfDebug(this, string.Format("Performing PageModel action '{0}'", actionArgs.ActionName));

            var mappedAction = pageDesctiptor.ButtonsMap.FirstOrDefault(b => b.ButtonActionName == actionArgs.ActionName);

            if (mappedAction != null)
            {
                hc.SyncContext.Post(o =>
                {
                    var pageAction = o as MappedPageAction;
                    
                    if (pageAction != null)
                    {
                        if (actionArgs.State == ButtonStates.Press)
                        {
                            hc.GetController<IUIController>().ShowPage(pageAction.PageName, pageAction.ViewName, null);
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

                }, mappedAction, string.Concat("ModelBase.MappedAction, action = ", actionArgs.ActionName));
            }
            else
            {
                hc.SyncContext.Post(o => DoAction(ModelNames.UnmappedAction, actionArgs), null, string.Concat("ModelBase.UnmappedAction, action = ", actionArgs.ActionName));
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
            IEnumerable<string> propNames;

            lock (properties)
            {
                propNames = properties.Keys.ToArray();
            }

            foreach (var pname in propNames)
            {
                OnPropertyChanged(pname);
            }
        }

        protected void OnPropertyChanged(string name)
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

            Shown += () => { if (timeout > 0) hc.CreateTimer(1000, TimerTick, true, false, "dialog timeout timer"); };
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
                hc.SyncContext.Post(o => handler((DialogResults)o), result, "DialogModelBase.OnClosed");
        }

        public void OnShown()
        {
            var handler = Shown;
            if (handler != null)
                hc.SyncContext.Post(o => handler(), null, "DialogModelBase.OnShown");
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
