using System;

namespace Yggdrasil.Scripting
{
	/// <summary>
	/// Specifies initialization priority.
	/// </summary>
	public class PriorityAttribute : Attribute
	{
		/// <summary>
		/// Value to sort by.
		/// </summary>
		public int Value { get; }

		/// <summary>
		/// Creates new instance
		/// </summary>
		/// <param name="value"></param>
		public PriorityAttribute(int value)
		{
			this.Value = value;
		}
	}
}
