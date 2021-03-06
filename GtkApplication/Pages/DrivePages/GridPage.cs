using System;
using System.Linq;
using GtkApplication.Pages;
using Interfaces;
using Interfaces.UI;
using System.Collections.Generic;
using Interfaces.UI.Models;
using System.Threading;

namespace GtkApplication
{
    internal class TextModelBinder : IDisposable
    {
        private readonly ITextGrigDataModel dataModel;

        private readonly Gtk.TreeView view;

        private readonly Style style;

        private Gtk.ListStore viewModel;

        private readonly List<Gtk.TreeIter> iters = new List<Gtk.TreeIter>();

		private readonly ManualResetEvent mre = new ManualResetEvent(true);

		private readonly object locker = new object();

        public TextModelBinder(ITextGrigDataModel dataModel, Gtk.TreeView view, Style style)
        {
            if (null == dataModel)
                throw new ArgumentNullException("dataModel");

            if (null == view)
                throw new ArgumentNullException("view");

            if (null == style)
                throw new ArgumentNullException("style");

            this.dataModel = dataModel;
            this.view = view;
            this.style = style;

			dataModel.RowChanged += RowChangedHandler;
			dataModel.CellChanged += CellChangedHandler;
			dataModel.RowInserted += RowInsertedHandler;
			dataModel.RowRemoved += RowRemovedHandler;

            CreateModel();
        }

		public class TableChangedEventArgs : EventArgs
		{
			public int Row { get; set; }
			public int Column { get; set; }
			public string Value { get; set; }
		}

		private void RowChangedHandler(int row)
		{
			LockingInvoke (RowChangedHandlerInternal, new TableChangedEventArgs { Row = row });
		}

		private void RowChangedHandlerInternal(object sender, EventArgs args)
		{
			var rArgs = args as TableChangedEventArgs;

			try
			{
				viewModel.SetValues(iters[rArgs.Row], dataModel.GetRowValues(rArgs.Row));
			}
			finally 
			{
				mre.Set();	
			}
		}

		private void CellChangedHandler(int row, int column, string value)
		{
			LockingInvoke (CellChangedHandlerInternal, new TableChangedEventArgs { Row = row, Column = column, Value = value });
		}

		private void CellChangedHandlerInternal(object sender, EventArgs args)
		{
			var rArgs = args as TableChangedEventArgs;

			try
			{
				viewModel.SetValue(iters[rArgs.Row], rArgs.Column, rArgs.Value);
			}
			finally 
			{
				mre.Set();	
			}
		}

		private void RowInsertedHandler(int row)
		{
			LockingInvoke(RowInsertedHandlerInternal, new TableChangedEventArgs { Row = row });
		}

		private void RowInsertedHandlerInternal(object sender, EventArgs args)
		{
			var rArgs = args as TableChangedEventArgs;

			try
			{
				RowInserted(rArgs.Row);
			}
			finally
			{
				mre.Set();
			}
		}

		private void RowRemovedHandler(int row)
		{
			LockingInvoke(RowRemovedHandlerInternal, new TableChangedEventArgs { Row = row });
		}

		private void RowRemovedHandlerInternal(object sender, EventArgs args)
		{
			var rArgs = args as TableChangedEventArgs;

			try
			{
				var iter = iters[rArgs.Row];
				viewModel.Remove(ref iter);
				iters.RemoveAt(rArgs.Row);
			}
			finally
			{
				mre.Set();
			}
		}

		private void LockingInvoke(EventHandler handler, EventArgs args)
		{
			lock(locker)
			{
				mre.Reset();

				Gtk.Application.Invoke(this, args, handler);

				mre.WaitOne();
			}
		}

		private void CreateModel()
        {
            //removing existing columns
            foreach (var column in view.Columns)
                view.RemoveColumn(column);

            //Creating columns
			if (dataModel.Columns.Length > 0)
				for (int i = 0; i < dataModel.Columns.Length; ++i)
                {
				    var column = new Gtk.TreeViewColumn(dataModel.Columns[i], 
                        new Gtk.CellRendererText() 
				        {                     
					        Xalign = 0.5f,
							BackgroundGdk = new Gdk.Color(190, 190, 220),
                            FontDesc = new Pango.FontDescription() { Weight = i==0 ? Pango.Weight.Bold : Pango.Weight.Normal },

				        }, "text", i);

					column.Expand = true;
					column.Alignment = 0.5f;

                    view.AppendColumn(column);
                }

            //Create view model
            viewModel = new Gtk.ListStore(dataModel.Columns.Select(o => typeof(string)).ToArray());

            view.Model = viewModel;

            //Filling model
            iters.Clear();

            if (dataModel.Rows > 0)
            {
                for (int i = 0; i < dataModel.Rows; ++i)
                {
                    RowInserted(i);
                }
            }
        }

        void RowInserted(int index)
        {
            iters.Insert(index, viewModel.InsertWithValues(index, dataModel.GetRowValues(index)));
        }

        public void Dispose()
        {
            if (null != mre)
                mre.Dispose();
        }
    }


	[System.ComponentModel.ToolboxItem(true)]
	public partial class GridPage : Gtk.Bin
	{
        private readonly TextModelBinder gridBinder;

        private const string m_TITLE = "<span size='16000'>{0}</span>";

		public GridPage(IPageModel model, Style style, ILogger logger)
		{
			Build();

            this.Destroyed += (s, e) => gridBinder.Dispose();

            style.Window.Apply(eventbox1);
			style.Window.Apply(eventbox2);

            style.TextBox.Apply(label1, eventbox1);

			var tableModel = model.GetProperty<ITextGrigDataModel>("grid");

			if (null != tableModel)
				gridBinder = new TextModelBinder(tableModel, treeview1, style);
			else
				throw new Exception("'table' property must be initialized with ITextGrigDataModel inctance before creating page");

            var binder = new ModelBinder(model, logger);

            binder.BindLabelMarkup(label1, ModelNames.PageTitle, str => string.Format(m_TITLE, str));
		}
	}
}
