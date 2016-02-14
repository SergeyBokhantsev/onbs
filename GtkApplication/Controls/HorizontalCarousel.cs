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
    public class HorizontalCarousel
    {
        private const string m_ITEM = "<span {0} size='15000'>{1}</span>";

        private readonly HBox box;
        private readonly Style style;
        private readonly IPageModel model;

        internal HorizontalCarousel(ModelBinder binder, HBox box, Style style, string itemsSourceName)
        {
            this.box = box;
            this.style = style;
            this.model = binder.Model;

            binder.BindCustomAction<IListItem[]>(CreateList, itemsSourceName);
        }

        private void CreateList(IListItem[] objects)
        {

            box.BorderWidth = 0;
            box.Spacing = 0;
            box.Homogeneous = false;

            if (box.Children != null)
            {
                foreach (var child in box.Children.ToArray())
                {
                    box.Remove(child);
                }
            }

            if (objects != null)
            {
                var eventBox_prev = new EventBox();
                eventBox_prev.Visible = true;                
                var btn_prev = new FlatButton(eventBox_prev, style.TextBox, TextAligment.CenterMiddle);
                btn_prev.Text = model.GetProperty<string>(ModelNames.ButtonPrevLabel);
                btn_prev.Clicked += () => model.Action(new PageModelActionEventArgs(ModelNames.ButtonPrev, Interfaces.Input.ButtonStates.Press));
                box.Add(eventBox_prev);
                btn_prev.WidthRequest = 30;

                var boxChild = (Gtk.Box.BoxChild)box[eventBox_prev];
                boxChild.Expand = false;
                boxChild.Fill = true;

                var itemWidth = (((Gtk.Widget)box).GdkWindow.FrameExtents.Width - btn_prev.WidthRequest * 2 - 10) / objects.Length;

                int index = 0;

                foreach (var item in objects)
                {
                    var eventBox = new EventBox();
                    box.Add(eventBox);
                    
                    eventBox.ButtonPressEvent += (s, e) =>
                    {
                        var _item = (IListItem)((EventBox)s).Data["item"];
                        _item.Focused = true;
                        _item.Click();
                    };
                   
                    eventBox.Visible = true;
                    eventBox.HeightRequest = 20;
                    eventBox.WidthRequest = itemWidth;
                    eventBox.Data["item"] = item;
                    eventBox.Data["index"] = index;

                    boxChild = (Gtk.Box.BoxChild)box[eventBox];
                    boxChild.Expand = false;
                    boxChild.Fill = true;

                    var button = new Label();
                    eventBox.Add(button);

                    style.CommonButton.Apply(button, eventBox);

                    //var eventBoxChild = (Gtk.Box.ContainerChild)eventBox[button];
                    //eventBoxChild.Parent.HeightRequest = 10;
                    //eventBoxChild.Child.HeightRequest = 10;

                    button.Xalign = 0.5f;
                    button.HeightRequest = 10;
                    button.SetPadding(0, 0);

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
                            style.TextBox.Apply(eventBox);
                    };
                    item.FocusChanged += (s, e) => Application.Invoke(s, e, setFocus);
                    setFocus(null, null);

                    index++;
                }

                var eventBox_next = new EventBox();
                eventBox_next.Visible = true;
                var btn_next = new FlatButton(eventBox_next, style.TextBox, TextAligment.CenterMiddle);
                btn_next.Text = model.GetProperty<string>(ModelNames.ButtonNextLabel);
                btn_next.Clicked += () => model.Action(new PageModelActionEventArgs(ModelNames.ButtonNext, Interfaces.Input.ButtonStates.Press));
                box.Add(eventBox_next);
                btn_next.WidthRequest = 30;

                boxChild = (Gtk.Box.BoxChild)box[eventBox_next];
                boxChild.Expand = false;
                boxChild.Fill = true;
                boxChild.PackType = PackType.End;
            }
        }
    }
}
