using System;
using Gtk;
using Gdk;
using System.Threading.Tasks;

namespace GtkApplication
{
	public class FlatButton
	{
		public event System.Action Clicked;

		private readonly EventBox box;
		private readonly Label label;
		private readonly LookAndFeel scheme;

		public string Text
		{
			get {
				return label.Text;
			}
			set {
				label.Text = value;
			}
		}

		public int HeightRequest
		{
			get {
				return label.HeightRequest;
			}
			set {
				label.HeightRequest = value;
			}
		}

        public int WidthRequest
        {
            get
            {
                return label.WidthRequest;
            }
            set
            {
                label.WidthRequest = value;
            }
        }

        public bool Focused
        {
            set
            {
                throw new NotImplementedException();
            }
        }

		public FlatButton(EventBox box, LookAndFeel scheme)
			:this(box, scheme, TextAligment.CenterMiddle)
		{
		}

		public FlatButton(EventBox box, LookAndFeel scheme, TextAligment labelAligment)
		{
			this.box = box;
			this.scheme = scheme;

			label = new Label ();
			label.SetAlignment(labelAligment.X, labelAligment.Y);
            label.Visible = true;

			box.Add (label);

			box.EnterNotifyEvent +=	EnterNotifyEvent;
			box.LeaveNotifyEvent += LeaveNotifyEvent;
			box.ButtonPressEvent += ButtonPressEvent;

			SetBg (scheme.Bg);
			SetFg (scheme.Fg);
		}

	    void ButtonPressEvent (object o, ButtonPressEventArgs args)
		{
            if (args.Event.Type == EventType.ButtonPress)
            {
                if (Clicked != null)
                    Clicked();
            }
		}

		private void SetBg(Color color)
		{
			box.ModifyBg (StateType.Normal, color);
		}

		private void SetFg(Color color)
		{
			label.ModifyFg (StateType.Normal, color);
		}

		private void EnterNotifyEvent(object sender, EnterNotifyEventArgs args)
		{
			SetBg (scheme.HoverColor);
		}
			
		private void LeaveNotifyEvent (object o, LeaveNotifyEventArgs args)
		{
			SetBg (scheme.Bg);
		}
	}
}

