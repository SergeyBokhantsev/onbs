using System;
using Interfaces.UI;
using Interfaces;
using GtkApplication.Pages;
using System.Collections;
using System.Linq;
using Gtk;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MultilineView : Gtk.Bin
	{
		public MultilineView (IPageModel model, Style style, ILogger logger)
		{
			this.Build();

			style.Window.Apply (eventbox_title);
			style.Window.Apply (eventbox_log);

			textview_log.ModifyBase (Gtk.StateType.Normal, style.Window.Bg);
			textview_log.ModifyText(Gtk.StateType.Normal, style.TextBox.Fg);
			textview_log.ModifyCursor(style.TextBox.Bg, style.TextBox.Fg);

			var binder = new ModelBinder (model, logger);

			binder.BindLabelMarkup (label_title, ModelNames.PageTitle, o => string.Format("<span foreground='#d6d6d6' size='12000'>{0}</span>", o));

			binder.BindCustomAction<System.Collections.Concurrent.ConcurrentQueue<string>>(queue => {
				if (queue != null)
				{
					string line = null;

					while(queue.TryDequeue(out line))
					{
						textview_log.Buffer.Text += string.Concat(line, Environment.NewLine);
					}

					textview_log.ScrollToIter(textview_log.Buffer.EndIter, 0, true, 0, 0);
				}
			}, "lines_queue");

            binder.BindCustomAction<object>(o => textview_log.Buffer.Clear(), "clear");

			binder.UpdateBindings ();
		}
	}
}

