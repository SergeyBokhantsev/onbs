using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Interfaces.UI
{
    public enum MappedActionBehaviors { All = 0, PressOrHold = 1, Press = 43, Hold = 44, Release = 45 }

    public abstract class MappedActionBase
    {
        public string ButtonActionName { get; private set; }
        public string Caption { get; private set; }
        public MappedActionBehaviors ActionBehavior { get; private set; }

        protected MappedActionBase(string buttonActionName, string caption, MappedActionBehaviors behavior)
        {
            ButtonActionName = buttonActionName;
            Caption = caption;
            ActionBehavior = behavior;
        }
    }

    public class MappedPageAction : MappedActionBase
    {
        public string PageName { get; private set; }
        public string ViewName { get; private set; }

        public MappedPageAction(string buttonActionName, string caption, MappedActionBehaviors behavior, string pageName, string viewName)
            :base(buttonActionName, caption, behavior)
        {
            PageName = pageName;
            ViewName = viewName;
        }
    }

    public class MappedCustomAction : MappedActionBase
    {
        public string CustomActionName { get; private set; }

        public MappedCustomAction(string buttonActionName, string caption, MappedActionBehaviors behavior, string customActionName)
            :base(buttonActionName, caption, behavior)
        {
            CustomActionName = customActionName;
        }
    }

    public class MappedPage
    {
        public string Name { get; private set; }
        public string ModelTypeName { get; private set; }
        public string DefaultViewName { get; private set; }
        public List<MappedActionBase> ButtonsMap { get; private set; }

        public MappedPage(string name, string modelTypeName, string defaultViewName, List<MappedActionBase> buttonsMap)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name is not provided");

            if (string.IsNullOrWhiteSpace(modelTypeName))
                throw new ArgumentNullException("modelTypeName is not provided");

            if (string.IsNullOrWhiteSpace(defaultViewName))
                throw new ArgumentNullException("defaultViewName is not provided");

            if (buttonsMap == null)
                throw new ArgumentNullException("buttonsMap");

            Name = name;
            ModelTypeName = modelTypeName;
            DefaultViewName = defaultViewName;
            ButtonsMap = buttonsMap;
        }
    }

    public class ApplicationMap
    {
        public string DefaultPageName { get; private set; }
        public string DefaultPageViewName { get; private set; }

        private List<MappedPage> Pages;

        public ApplicationMap(string applicationMapFilePath)
        {
            const string DEFAULT = "default";
            const string PAGENAME = "name";
            const string MODEL = "model";
            const string VIEW = "view";
            const string DEFAULTVIEW = "default_view";
            const string ACTION = "action";
            const string CAPTION = "caption";
            const string BEHAVIOR = "behavior";
            const string PAGES = "pages";
            const string PAGE = "page";

            Pages = new List<MappedPage>();

            XDocument doc = null;
            using (var stream = File.OpenRead(applicationMapFilePath))
            {
                doc = XDocument.Load(stream);
            }

            var defaultElement = doc.Root.Element(DEFAULT);
            DefaultPageName = defaultElement.Attribute(PAGENAME) != null ? defaultElement.Attribute(PAGENAME).Value : null;
            if (string.IsNullOrWhiteSpace(DefaultPageName))
                throw new Exception("Application default model is not provided");
            
            DefaultPageViewName = defaultElement.Attribute(VIEW) != null ? defaultElement.Attribute(VIEW).Value : null;

            foreach (var pageElement in doc.Root.Element(PAGES).Elements(PAGE))
            {
                var pageName = pageElement.Attribute(PAGENAME).Value;
                var pageModelTypeName = pageElement.Attribute(MODEL).Value;
                var defaultViewName = pageElement.Attribute(DEFAULTVIEW) != null ? pageElement.Attribute(DEFAULTVIEW).Value : null;

                var buttonsMap = new List<MappedActionBase>();

                foreach(var buttonElement in pageElement.Elements())
                {
                    MappedActionBase mappedAction = null;
                    var actionPageName = buttonElement.Attribute(PAGENAME) != null ? buttonElement.Attribute(PAGENAME).Value : null;
                    var view = buttonElement.Attribute(VIEW) != null ? buttonElement.Attribute(VIEW).Value : null;
                    var customAction = buttonElement.Attribute(ACTION) != null ? buttonElement.Attribute(ACTION).Value : null;
                    var caption = buttonElement.Attribute(CAPTION) != null ? buttonElement.Attribute(CAPTION).Value : actionPageName ?? customAction;
                    var behavior = buttonElement.Attribute(BEHAVIOR) != null ? (MappedActionBehaviors)Enum.Parse(typeof(MappedActionBehaviors), buttonElement.Attribute(BEHAVIOR).Value) : MappedActionBehaviors.Press;
                    
                    if (!string.IsNullOrWhiteSpace(customAction))
                    {
                        mappedAction = new MappedCustomAction(buttonElement.Name.LocalName, caption, behavior, customAction);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(actionPageName))
                            throw new ArgumentException(string.Format("Nor action nor model weren't provided for button item {0}", buttonElement.Name));

                        mappedAction = new MappedPageAction(buttonElement.Name.LocalName, caption, behavior, actionPageName, view);
                    }

                    buttonsMap.Add(mappedAction);
                }

                Pages.Add(new MappedPage(pageName, pageModelTypeName, defaultViewName, buttonsMap));
            }
        }

        public MappedPage GetPage(string pageName)
        {
            return Pages.FirstOrDefault(p => p.Name == pageName);
        }

        public string GetMappedButtonForCustomAction(IPageModel model, string actionName)
        {
            if (model != null && actionName != null)
            {
                var typeName = model.GetType().Name;
                var mappedPage = Pages.FirstOrDefault(p => p.ModelTypeName == typeName);
                if (mappedPage != null)
                {
                    var action = mappedPage.ButtonsMap.OfType<MappedCustomAction>().FirstOrDefault(p => p.CustomActionName == actionName);

                    return action != null ? action.ButtonActionName : null;
                }
            }

            return null;
        }

        public MappedActionBase GetMappedActionFor(IPageModel model, PageModelActionEventArgs arg)
        {
            if (model != null && arg != null)
            {
                var typeName = model.GetType().Name;
                var mappedPage = Pages.FirstOrDefault(p => p.ModelTypeName == typeName);
                if (mappedPage != null)
                {
                    var mappedAction = mappedPage.ButtonsMap.FirstOrDefault(b => b.ButtonActionName == arg.ActionName);
                    if (mappedAction != null)
                    {
                        if (mappedAction.ActionBehavior == MappedActionBehaviors.All || (int)mappedAction.ActionBehavior == (int)arg.State)
                            return mappedAction;
                    }
                }
            }

            return null;
        }

        public static void SetCaptions(IPageModel model, MappedPage page)
        {
            foreach (var action in page.ButtonsMap)
            {
                if (!string.IsNullOrWhiteSpace(action.Caption)
                    && model.GetProperty<object>(ModelNames.ResolveButtonLabelName(action.ButtonActionName)) == null)
                {
                    model.SetProperty(ModelNames.ResolveButtonLabelName(action.ButtonActionName), action.Caption);
                }
            }
        }
    }
}
