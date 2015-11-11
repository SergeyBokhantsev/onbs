﻿using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UIController
{
    public enum MappedActionBehaviors { All = 0, Press = 43, Hold = 44, Release = 45 }

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
        public string ModelTypeName { get; private set; }
        public string ViewName { get; private set; }

        public MappedPageAction(string buttonActionName, string caption, MappedActionBehaviors behavior, string modelTypeName, string viewName)
            :base(buttonActionName, caption, behavior)
        {
            ModelTypeName = modelTypeName;
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

    internal class MappedPage
    {
        public string ModelTypeName { get; private set; }
        public List<MappedActionBase> ButtonsMap { get; private set; }

        public MappedPage(string modelTypeName, List<MappedActionBase> buttonsMap)
        {
            if (string.IsNullOrWhiteSpace(modelTypeName))
                throw new ArgumentNullException("modelTypeName is not provided");

            if (buttonsMap == null)
                throw new ArgumentNullException("buttonsMap");

            ModelTypeName = modelTypeName;
            ButtonsMap = buttonsMap;
        }
    }

    public class ApplicationMap
    {
        public string DefaultPageModelTypeName { get; private set; }
        public string DefaultPageViewName { get; private set; }

        private List<MappedPage> Pages;

        public ApplicationMap(string applicationMapFilePath)
        {
            const string DEFAULT = "default";
            const string MODEL = "model";
            const string VIEW = "view";
            const string ACTION = "action";
            const string CAPTION = "caption";
            const string BEHAVIOR = "behavior";
            const string PAGES = "pages";
            const string PAGE = "page";

            XDocument doc = null;
            using (var stream = File.OpenRead(applicationMapFilePath))
            {
                doc = XDocument.Load(stream);
            }

            var defaultElement = doc.Root.Element(DEFAULT);
            DefaultPageModelTypeName = defaultElement.Attribute(MODEL).Value;
            if (string.IsNullOrWhiteSpace(DefaultPageModelTypeName))
                throw new Exception("Application default model is not provided");
            
            DefaultPageViewName = defaultElement.Attribute(VIEW).Value;
            if (string.IsNullOrWhiteSpace(DefaultPageViewName))
                DefaultPageViewName = DefaultPageModelTypeName;

            foreach (var pageElement in doc.Root.Element(PAGES).Elements(PAGE))
            {
                var pageModelTypeName = pageElement.Attribute(MODEL).Value;

                var buttonsMap = new List<MappedActionBase>();

                foreach(var buttonElement in pageElement.Elements())
                {
                    MappedActionBase mappedAction = null;
                    var modelTypeName = buttonElement.Attribute(MODEL) != null ? buttonElement.Attribute(MODEL).Value : null;
                    var view = buttonElement.Attribute(VIEW) != null ? buttonElement.Attribute(VIEW).Value : null;
                    var customAction = buttonElement.Attribute(ACTION) != null ? buttonElement.Attribute(ACTION).Value : null;
                    var caption = buttonElement.Attribute(CAPTION) != null ? buttonElement.Attribute(CAPTION).Value : null;
                    var behavior = buttonElement.Attribute(BEHAVIOR) != null ? (MappedActionBehaviors)Enum.Parse(typeof(MappedActionBehaviors), buttonElement.Attribute(BEHAVIOR).Value) : MappedActionBehaviors.Press;
                    
                    if (!string.IsNullOrWhiteSpace(customAction))
                    {
                        mappedAction = new MappedCustomAction(buttonElement.Name.LocalName, caption, behavior, customAction);
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(modelTypeName))
                            throw new ArgumentException(string.Format("Nor action nor model weren't provided for button item {0}", buttonElement.Name));

                        if (string.IsNullOrWhiteSpace(view))
                            view = modelTypeName;

                        mappedAction = new MappedPageAction(buttonElement.Name.LocalName, caption, behavior, modelTypeName, view);
                    }

                    buttonsMap.Add(mappedAction);
                }

                Pages.Add(new MappedPage(pageModelTypeName, buttonsMap));
            }
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

        public void SetCaptions(IPageModel model)
        {
            var typeName = model.GetType().Name;
            var mappedPage = Pages.FirstOrDefault(p => p.ModelTypeName == typeName);
            if (mappedPage != null)
            {
                foreach(var action in mappedPage.ButtonsMap)
                {
                    if (!string.IsNullOrWhiteSpace(action.Caption))
                        model.SetProperty(ModelNames.ResolveButtonLabelName(action.ButtonActionName), action.Caption);
                }
            }
        }
    }

    public static class MapExtensions
    {
        public static void UpdateLabelForAction(this ApplicationMap map, IPageModel model, string customActionName, string labelValue)
        {
            var buttonName = map.GetMappedButtonForCustomAction(model, customActionName);
            if (buttonName != null)
            {
                var buttonLabelPropertyName = ModelNames.ResolveButtonLabelName(buttonName);
                model.SetProperty(buttonLabelPropertyName, labelValue);
            }
        }
    }
}
