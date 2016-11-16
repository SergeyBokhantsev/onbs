using System;
using GtkApplication.Pages;
using Interfaces;
using Interfaces.UI;

namespace GtkApplication
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class GridPage : Gtk.Bin
	{
		public interface ITextGrigDataModel
		{
			/// <summary>
			/// row, column
			/// </summary>
			event Action<int, int> CellChanged;

			event Action SizeChanged;

			int Rows { get; }
			int Columns { get; }

			string GetColumnTitle(int column);
			string GetValue(int row, int column);
		}

		internal class TextModelBinder
		{
			private readonly ITextGrigDataModel dataModel;

			private readonly Gtk.TreeView view;

			public TextModelBinder(ITextGrigDataModel dataModel, Gtk.TreeView view)
			{
				if (null == dataModel)
					throw new ArgumentNullException("dataModel");

				if (null == view)
					throw new ArgumentNullException("view");

				this.dataModel = dataModel;
				this.view = view;

				dataModel.SizeChanged += SizeChanged;
				dataModel.CellChanged += CellChanged;

				SizeChanged();
			}

			void SizeChanged()
			{
				//removing existing columns
				foreach(var column

				//Creating columns

			}

			void CellChanged(int row, int column)
			{

			}
		}

		public GridPage(IPageModel model, Style style, ILogger logger)
		{
			this.Build();

			Gtk.TreeViewColumn artistColumn = new Gtk.TreeViewColumn();
			artistColumn.Title = "Artist";

			Gtk.TreeViewColumn songColumn = new Gtk.TreeViewColumn();
			songColumn.Title = "Song Title";

			treeview1.AppendColumn(artistColumn);
			treeview1.AppendColumn(songColumn);

			// Create the text cell that will display the artist name
			Gtk.CellRendererText artistNameCell = new Gtk.CellRendererText();

			// Add the cell to the column
			artistColumn.PackStart(artistNameCell, true);

			// Do the same for the song title column
			Gtk.CellRendererText songTitleCell = new Gtk.CellRendererText();
			songColumn.PackStart(songTitleCell, true);


			artistColumn.AddAttribute(artistNameCell, "text", 0);
			songColumn.AddAttribute(songTitleCell, "text", 1);

			// Create a model that will hold two strings - Artist Name and Song Title
			Gtk.ListStore musicListStore = new Gtk.ListStore(typeof(string), typeof(string));

			treeview1.Model = musicListStore;

			// Add some data to the store
			var iter = musicListStore.AppendValues("Garbage", "Dog New Tricks");



			var binder = new ModelBinder(model, logger);

			binder.BindCustomAction<string>(str =>
			{
				//musicListStore.InsertWithValues(0, str, str);

				musicListStore.SetValues(iter, str, str);
			}, "val");
		}
	}
}
