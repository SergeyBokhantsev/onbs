using System;
using Interfaces.UI;
using Interfaces;
using GtkApplication.Pages;
using CB = GtkApplication.CommonBindings;
using GtkApplication.Controls;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FileOptionsPage : Gtk.Bin
	{
		private const string m_FileName = "<span {0} {1} size='20000'>{2}</span>";
		private const string m_FileProps = "<span {0} {1} size='14000'>{2}</span>";

        private readonly VerticalRotaryList vrl;

		public FileOptionsPage(IPageModel model, Style style, ILogger logger)
		{
			this.Build ();
			var binder = new ModelBinder (model, logger);

			style.Window.Apply (label_file_name, eventbox_file_name);
			style.Window.Apply (label_file_props, eventbox_file_props);

            vrl = new VerticalRotaryList(binder, vbox2, style, model.GetProperty<string>("items_source_prop_name"));

			binder.InitializeButton(style, eventbox_cancel, style.CancelButton, ModelNames.ButtonCancel, TextAligment.CenterMiddle, 30);

			binder.BindLabelMarkup (label_file_name, "file_name", value => CB.CreateMarkup (m_FileName, CB.m_BG_EMPTY, CB.m_FG_GRAY, value));
			binder.BindLabelMarkup (label_file_props, "file_props", value => CB.CreateMarkup (m_FileProps, CB.m_BG_EMPTY, CB.m_FG_GRAY, value));

            binder.UpdateBindings();
		}
	}
}

