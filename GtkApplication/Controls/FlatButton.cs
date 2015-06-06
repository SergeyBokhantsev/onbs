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

		public FlatButton(EventBox box, LookAndFeel scheme)
		{
			this.box = box;
			this.scheme = scheme;

			label = new Label ();
			box.Add (label);

			box.EnterNotifyEvent +=	EnterNotifyEvent;
			box.LeaveNotifyEvent += LeaveNotifyEvent;
			box.ButtonPressEvent += ButtonPressEvent;

			SetBg (scheme.Bg);
			SetFg (scheme.Fg);
		}

		async void ButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			SetBg (scheme.ClickColor);

			if (Clicked != null)
				Clicked ();

			await Task.Delay(200);
			SetBg (scheme.Bg);
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

