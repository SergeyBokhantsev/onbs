﻿using System;
using System.Linq;
using GtkApplication.Pages;
using Interfaces;
using Interfaces.UI;
using System.Collections.Generic;
using Interfaces.UI.Models;
using System.Threading;

namespace GtkApplication
{
    internal class TextModelBinder
    {
        private readonly ITextGrigDataModel dataModel;

        private readonly Gtk.TreeView view;

        private Gtk.ListStore viewModel;

        private readonly List<Gtk.TreeIter> iters = new List<Gtk.TreeIter>();

		private readonly ManualResetEvent mre = new ManualResetEvent(true);

		private readonly object locker = new object();

        public TextModelBinder(ITextGrigDataModel dataModel, Gtk.TreeView view)
        {
            if (null == dataModel)
                throw new ArgumentNullException("dataModel");

            if (null == view)
                throw new ArgumentNullException("view");

            this.dataModel = dataModel;
            this.view = view;

			dataModel.RowChanged += RowChangedHandler;
			dataModel.CellChanged += CellChangedHandler;
			//dataModel.RowInserted += (obj) => LockingInvoke(() => RowInserted(obj));
			//dataModel.RowRemoved += (obj) => LockingInvoke(() => RowRemoved(obj));

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
			LockingInvoke (RowChangedHandlerInternal, new TableChangedEventArgs { Row = row, Column = column, Value = value });
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
				var column = new Gtk.TreeViewColumn(dataModel.Columns[i], new Gtk.CellRendererText() 
				{ 
					Xalign = 0.5f
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

        void RowRemoved(int index)
        {
            var iter = iters[index];
            viewModel.Remove(ref iter);
            iters.RemoveAt(index);
        }

        void RowInserted(int index)
        {
            iters.Insert(index, viewModel.InsertWithValues(index, dataModel.GetRowValues(index)));
        }
    }


	[System.ComponentModel.ToolboxItem(true)]
	public partial class GridPage : Gtk.Bin
	{
        private readonly TextModelBinder gridBinder;

		public GridPage(IPageModel model, Style style, ILogger logger)
		{
			Build();

            var binder = new ModelBinder(model, logger);

			var tableModel = model.GetProperty<ITextGrigDataModel>("grid");

			if (null != tableModel)
				gridBinder = new TextModelBinder(tableModel, treeview1);
			else
				throw new Exception("'table' property must be initialized with ITextGrigDataModel inctance before creating page");

            //Gtk.TreeViewColumn artistColumn = new Gtk.TreeViewColumn();
            //artistColumn.Title = "Artist";

            //Gtk.TreeViewColumn songColumn = new Gtk.TreeViewColumn();
            //songColumn.Title = "Song Title";

            //treeview1.AppendColumn(artistColumn);
            //treeview1.AppendColumn(songColumn);

            //// Create the text cell that will display the artist name
            //Gtk.CellRendererText artistNameCell = new Gtk.CellRendererText();

            //// Add the cell to the column
            //artistColumn.PackStart(artistNameCell, true);

            //// Do the same for the song title column
            //Gtk.CellRendererText songTitleCell = new Gtk.CellRendererText();
            //songColumn.PackStart(songTitleCell, true);


            //artistColumn.AddAttribute(artistNameCell, "text", 0);
            //songColumn.AddAttribute(songTitleCell, "text", 1);

            //// Create a model that will hold two strings - Artist Name and Song Title
            //Gtk.ListStore musicListStore = new Gtk.ListStore(typeof(string), typeof(string));

            //treeview1.Model = musicListStore;

            //// Add some data to the store
            //var iter = musicListStore.AppendValues("Garbage", "Dog New Tricks");




            //binder.BindCustomAction<string>(str =>
            //{
            //    //musicListStore.InsertWithValues(0, str, str);

            //    musicListStore.SetValues(iter, str, str);
            //}, "val");
		}
	}
}
