using System;

namespace ProcessStateMachine
{
	public static class Extensions
	{
		public static StateDescriptor Then(this StateDescriptor state, StateDescriptor next)
		{
			if (null == state)
				throw new ArgumentNullException ("this");

			if (null == next)
				throw new ArgumentNullException ("next");

			if (!state.Children.Contains (next))
				state.Children.Add (next);

			return next;
		}

		public static void CouldGoToMe(this StateDescriptor child, params StateDescriptor[] parents)
		{
			if (null == child)
				throw new ArgumentNullException ("child");

			foreach (var parent in parents) 
			{
				if (!parent.Children.Contains (child))
					parent.Children.Add (child);
			}
		}
	}
}

