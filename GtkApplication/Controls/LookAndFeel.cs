using System;
using Gdk;
using Gtk;

namespace GtkApplication
{
	public class LookAndFeel
	{
		public Color Fg { 
			get;
			set;
		}

		public Color Bg { 
			get;
			set;
		}

		public Color HoverColor { 
			get;
			set;
		}

		public Color ClickColor { 
			get;
			set;
		}
	}

	public class Style
	{
		public LookAndFeel Window { get; private set; }
		public LookAndFeel CommonButton { get; private set; }
		public LookAndFeel AcceptButton { get; private set; }
		public LookAndFeel CancelButton { get; private set; }
		public LookAndFeel TextBox { get; private set; }

		public Style(LookAndFeel window, LookAndFeel commonButton, LookAndFeel acceptButton, LookAndFeel cancelButton, LookAndFeel textBox)
		{
			Window = window;
			CommonButton = commonButton;
			CancelButton = cancelButton;
			AcceptButton = acceptButton;
			TextBox = textBox;
		}
	}

	public static class StyleExtensions
	{
		public static void Apply(this LookAndFeel lf, Label label, EventBox parent)
		{
			label.ModifyFg (StateType.Normal, lf.Fg);

			if (parent != null) 
			{
				parent.ModifyBg (StateType.Normal, lf.Bg);
			}
		}

		public static void Apply(this LookAndFeel lf, TextView textview, EventBox parent)
		{
			textview.ModifyFg (StateType.Normal, lf.Fg);

			if (parent != null) 
			{
				parent.ModifyBg (StateType.Normal, lf.Bg);
			}
		}
	}
}

