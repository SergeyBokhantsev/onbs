using System;
using Gtk;
//using System.Xml.Linq;

namespace GtkApplication
{
	public class MarkupLabel
	{
		private Label label;
        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; Update(); }
        }

		public MarkupLabel (Label label)
		{
			this.label = label;

			if (label == null)
				throw new ArgumentNullException ("label");

            ParseMarkup();
            Update();
		}

        private void ParseMarkup()
        {
            try
            {
                //var doc = XDocument.Parse(label.LabelProp);
            }
            catch
            {
                text = "Error parsing";
            }
        }

        private void Update()
        {
            throw new NotImplementedException();
        }
	}
}

