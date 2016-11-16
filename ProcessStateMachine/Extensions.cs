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

            state.Add(next);

			return next;
		}
	}
}

