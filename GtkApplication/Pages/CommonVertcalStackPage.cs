using System;
using Interfaces.UI;
using Gtk;
using Interfaces;
using GtkApplication.Pages;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CommonVertcalStackPage : Gtk.Bin
	{
        private const string m_ITEM = "<span {0} size='15000'>{1}</span>";

		private readonly IPageModel model;
		private readonly Style style;
		private readonly ILogger logger;

        public CommonVertcalStackPage(IPageModel model, Style style, ILogger logger)
		{
			this.model = model;
			this.style = style;
			this.logger = logger;
			var binder = new ModelBinder (model, logger);

			this.Build();

			style.Window.Apply(label_caption, eventbox_caption);

            binder.BindLabelText(label_caption, ModelNames.PageTitle);

            binder.BindCustomAction<IListItem[]>(CreateList, model.GetProperty<string>("items_source_prop_name"));

            binder.InitializeButton(style, eventbox_exit_button, style.CancelButton, ModelNames.ButtonCancel, TextAligment.CenterMiddle);
            binder.InitializeButton(style, eventbox_next_button, style.AcceptButton, ModelNames.ButtonAccept, TextAligment.CenterMiddle);

            CreateUpDownButtons(binder, style);
            
            binder.UpdateBindings();
		}

        private void CreateUpDownButtons(ModelBinder binder, Style style)
        {
            var btn = new FlatButton(eventbox_bUp, style.CommonButton, TextAligment.CenterMiddle);
            btn.WidthRequest = 20;

            binder.BindFlatButtonLabel(btn, ModelNames.ButtonPrevLabel);
            binder.BindFlatButtonClick(btn, ModelNames.ButtonPrev);

            btn = new FlatButton(eventbox_bDown, style.CommonButton, TextAligment.CenterMiddle);
            btn.WidthRequest = 20;

            binder.BindFlatButtonLabel(btn, ModelNames.ButtonNextLabel);
            binder.BindFlatButtonClick(btn, ModelNames.ButtonNext);
        }

        private void SetFocus(EventBox eventBox, bool state)
        {
            if (eventBox != null)
            {
                if (!state)
                    style.CommonButton.Apply(eventBox);
                else
                    style.AcceptButton.Apply(eventBox);
            }
        }

        private void CreateList(IListItem[] objects)
        {
            VBox box = vbox1;

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

                foreach(var item in objects)
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

