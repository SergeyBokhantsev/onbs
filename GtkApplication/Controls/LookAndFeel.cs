using System;
using Gdk;

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

	public class LookAndFeelBag
	{
		public LookAndFeel Window { get; private set; }
		public LookAndFeel Button { get; private set; }
		public LookAndFeel TextBox { get; private set; }
	}
}

