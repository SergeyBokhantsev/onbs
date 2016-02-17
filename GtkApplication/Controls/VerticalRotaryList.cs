using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gtk;
using GtkApplication.Pages;
using Interfaces.UI;

namespace GtkApplication.Controls
{
    public class VerticalRotaryList
    {
        private const string m_ITEM = "<span {0} size='15000'>{1}</span>";

        private readonly Box box;
        private readonly Style style;
        private readonly IPageModel model;

        internal VerticalRotaryList(ModelBinder binder, Box box, Style style, string itemsSourceName)
        {
            this.box = box;
            this.style = style;
            this.model = binder.Model;

            binder.BindCustomAction<IListItem[]>(CreateList, itemsSourceName);
        }

        private void CreateList(IListItem[] objects)
        {
            if (box.Children != null)
            {
                foreach (var child in box.Children)
                {
                    box.Remove(child);
                }
            }

            if (objects != null)
            {
                int index = 0;

                foreach (var item in objects)
                {
                    var eventBox = new EventBox();
                    box.Add(eventBox);

                    eventBox.ButtonPressEvent += (s, e) =>
                    {
                        if (e.Event.Type == Gdk.EventType.ButtonPress)
                        {
                            var _item = (IListItem)((EventBox)s).Data["item"];
                            _item.Focused = true;
                            _item.Click();
                        }
                    };

                    eventBox.Visible = true;
                    eventBox.HeightRequest = 30;
                    eventBox.Data["item"] = item;
                    eventBox.Data["index"] = index;

                    var boxChild = (Gtk.Box.BoxChild)box[eventBox];
                    boxChild.Expand = false;
                    boxChild.Fill = false;

                    var button = new Label();
                    eventBox.Add(button);

                    style.CommonButton.Apply(button, eventBox);

                    button.Xalign = 0.0f;
                    button.SetPadding(40, 0);

                    button.UseMarkup = true;
                    button.Visible = true;

                    EventHandler createMarkup = (s, e) => button.Markup = CommonBindings.CreateMarkup(m_ITEM, CommonBindings.m_FG_GRAY, item.Caption);
                    item.CaptionChanged += (s, e) => Application.Invoke(s, e, createMarkup);
                    createMarkup(null, null);

                    EventHandler setFocus = (s, e) =>
                    {
                        if (item.Focused)
                            style.AcceptButton.Apply(eventBox);
                        else
                            style.CommonButton.Apply(eventBox);
                    };
                    item.FocusChanged += (s, e) => Application.Invoke(s, e, setFocus);
                    setFocus(null, null);

                    index++;
                }
            }
        }
    }
}
